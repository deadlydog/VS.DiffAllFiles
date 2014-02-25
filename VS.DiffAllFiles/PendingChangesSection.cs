using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.TeamExplorerBaseClasses;
using VS_DiffAllFiles;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;

namespace VS_DiffAllFiles
{
	/// <summary>
	/// Selected file info section.
	/// </summary>
	[TeamExplorerSection(PendingChangesSection.SectionId, TeamExplorerPageIds.PendingChanges, 35)]
	public class PendingChangesSection : TfsDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
#if (VS2012)
		public const string SectionId = "BEAB0FD4-357F-4F4A-868E-8E39FA8B78C9";
#else
		public const string SectionId = "8C62E1EB-19E1-4652-BD83-817179EF82CD";
#endif
		/// <summary>
		/// Handle to the Pending Changes Extensibility service.
		/// </summary>
		private IPendingChangesExt _pendingChangesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="PendingChangesSection"/> class.
		/// </summary>
		public PendingChangesSection()
			: base()
		{
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new PendingChangesControl(this);
		}

		/// <summary>
		/// Initialize override.
		/// </summary>
		public override void Initialize(object sender, SectionInitializeEventArgs e)
		{
			base.Initialize(sender, e);

			// Find the Pending Changes extensibility service and save a handle to it.
			_pendingChangesService = this.GetService<IPendingChangesExt>();

			// Register for property change notifications on the Pending Changes window.
			if (_pendingChangesService != null)
				_pendingChangesService.PropertyChanged += pendingChangesService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (_pendingChangesService != null)
				_pendingChangesService.PropertyChanged -= pendingChangesService_PropertyChanged;
			_pendingChangesService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		private void pendingChangesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IncludedChanges":
				case "FilteredIncludedChanges":
					NotifyPropertyChanged("IsViewIncludedFilesEnabled");
					NotifyPropertyChanged("IsViewAllFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges":
					NotifyPropertyChanged("IsViewExcludedFilesEnabled");
					NotifyPropertyChanged("IsViewAllFilesEnabled");
					break;

				case "SelectedExcludedItems":
				case "SelectedIncludedItems":
					NotifyPropertyChanged("IsViewSelectedFilesEnabled");
					NotifyPropertyChanged("IsViewAllFilesEnabled");
					break;
			}
		}

		public async Task ComparePendingChanges(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			// Set the Busy flag while we work.
			this.IsBusy = true;

			// If we don't have a handle to the Pending Changes service, display an error and exit.
			if (_pendingChangesService == null)
			{
				ShowNotification("Could not get a handle to the Pending Changes window.", NotificationType.Error);
				return;
			}

			// Get a handle to the Automation Model that we can use to interact with the VS IDE.
			var dte2 = PackageHelper.DTE2;
			if (dte2 == null)
			{
				ShowNotification("Could not get a handle to the DTE2 (the Visual Studio IDE Automation Model).", NotificationType.Error);
				return;
			}

			// Use the TFS Configured Diff tool for this version of Visual Studio.
			var tfFilePath = DiffAllFilesHelper.TfFilePath;
			if (!File.Exists(tfFilePath))
			{
				ShowNotification(string.Format("Could not locate TF.exe. Expected to find it at '{0}'.", tfFilePath), NotificationType.Error);
				return;
			}

			// Get all of the file types that are configured to use an external diff tool.
			var fileTypesConfiguredToUseExternalDiffTool = DiffAllFilesHelper.FileTypesWithExplicitDiffToolConfigured;

			// Get a handle to the settings to use.
			var settings = DiffAllFilesSettings.CurrentSettings;

			// Save the list of pending changes to compare to a new list before looping through them.
			List<PendingChange> itemsToCompare = null;
			switch (itemStatusTypesToCompare)
			{
				case ItemStatusTypesToCompare.All: 
					itemsToCompare = _pendingChangesService.IncludedChanges.Union(_pendingChangesService.ExcludedChanges).ToList(); 
					break;

				case ItemStatusTypesToCompare.Selected: 
					itemsToCompare = _pendingChangesService.SelectedIncludedItems.Where(i => i.IsPendingChange).Select(i => i.PendingChange)
						.Union(_pendingChangesService.SelectedExcludedItems.Where(i => i.IsPendingChange).Select(i => i.PendingChange)).ToList(); 
					break;

				case ItemStatusTypesToCompare.Included:
					// If there are filtered included changes, only grab them, else grab all of the included changes.
					itemsToCompare = _pendingChangesService.FilteredIncludedChanges.Length > 0 ? _pendingChangesService.FilteredIncludedChanges.ToList() : _pendingChangesService.IncludedChanges.ToList();
					break;

				case ItemStatusTypesToCompare.Excluded:
					// If there are filtered excluded changes, only grab them, else grab all of the excluded changes.
					itemsToCompare = _pendingChangesService.FilteredExcludedChanges.Length > 0 ? _pendingChangesService.FilteredExcludedChanges.ToList() : _pendingChangesService.ExcludedChanges.ToList();
					break;
			}

			// Only grab the files that are not on the list of file extensions to ignore.
			itemsToCompare = itemsToCompare.Where(p =>
			{
				var extension = System.IO.Path.GetExtension(p.LocalOrServerItem);
				return extension != null && !settings.FileExtensionsToIgnore.Contains(extension.TrimStart('.'));
			}).ToList();

			// Loop through and diff each of the pending changes.
			foreach (var pendingChange in itemsToCompare)
			{
				// If we are supposed to skip new files and this is a new file, then skip it and move on to the next change.
				if (!settings.CompareNewFiles &&
					(pendingChange.ChangeType == ChangeType.Add || pendingChange.ChangeType == ChangeType.Branch || pendingChange.ChangeType == ChangeType.Undelete))
					continue;

				// If we are supposed to skip deleted files and this is a deleted file, then skip it and move on to the next change.
				if (!settings.CompareDeletedFiles &&
					(pendingChange.ChangeType == ChangeType.Delete || pendingChange.ChangeType == ChangeType.SourceRename))
					continue;

				// If we are supposed to skip files whose contents have not changed and this file's contents have not changed, skip it and move on to the next change.
				// TOODO: Implement this.

				// If this file type is configured to use an external diff tool.
				if (fileTypesConfiguredToUseExternalDiffTool.Contains(Path.GetExtension(pendingChange.LocalOrServerItem)) ||
					fileTypesConfiguredToUseExternalDiffTool.Contains(".*"))
				{
					// Get the string representation the TF.exe expects for the version to compare against.
					string versionSpecToCompareAgainst = (settings.PendingChangesCompareVersion == CompareVersion.LatestVersion)
						? "T"	// Comapre against Latest version.
						: string.Format("W\"{0}\";\"{1}\"", _pendingChangesService.Workspace.Name, _pendingChangesService.Workspace.OwnerName);	// Compare against Workspace version.

					// Launch the configured diff tool to diff this file.
					var diffProcess = new System.Diagnostics.Process
					{
						StartInfo =
						{
							FileName = tfFilePath,
							Arguments = string.Format("diff \"{0}\" /version:{1}", pendingChange.LocalOrServerItem, versionSpecToCompareAgainst),
							CreateNoWindow = true,
							UseShellExecute = false
						}
					};
					diffProcess.Start();

					if (settings.CompareFilesOneAtATime)
						diffProcess.WaitForExit();
				}
				// Else this file type is not explicitly configured, so use the default built-in Visual Studio diff tool.
				else
				{
					// If we want to diff the files one by one we cannot do it asynchronously, as it will use a modal window which needs access to the GUI.
					if (settings.CompareFilesOneAtATime)
						RunVisualDiff(pendingChange, settings, dte2);
					// Else diff all of the files asynchronously.
					else
					{
						var change = pendingChange;
						await Task.Run(() => RunVisualDiff(change, settings, dte2)).ConfigureAwait(true);
					}
				}
			}

			this.IsBusy = false;
		}

		private void RunVisualDiff(PendingChange pendingChange, DiffAllFilesSettings settings, DTE2 dte2)
		{
			// Get the Source version to compare the local changes against.
			IDiffItem source = (settings.PendingChangesCompareVersion == CompareVersion.LatestVersion)
				? (IDiffItem)new DiffItemVersionedFile(pendingChange.VersionControlServer, pendingChange.ServerItem, VersionSpec.Latest)
				: new DiffItemPendingChangeBase(pendingChange);

			// Perform the diff using the VS API method to ensure the diff window opens in this instance of VS.
			Difference.VisualDiffItems(pendingChange.VersionControlServer,
				source,
				new DiffItemLocalFile(pendingChange.LocalItem, pendingChange.Encoding, pendingChange.CreationDate, false),
				settings.CompareFilesOneAtATime);

			// We are likely opening several diff windows, so make sure they don't just replace one another in the Preview Tab.
			if (PackageHelper.IsCommandAvailable("Window.KeepTabOpen"))
				dte2.ExecuteCommand("Window.KeepTabOpen");
		}

		///// <summary>
		///// Refresh the changeset data asynchronously.
		///// </summary>
		//private async Task RefreshAsync()
		//{
		//	try
		//	{
		//		// Set our busy flag and clear the previous data
		//		this.IsBusy = true;
		//		this.ServerPath = null;
		//		this.LocalPath = null;
		//		this.LatestVersion = null;
		//		this.WorkspaceVersion = null;
		//		this.Encoding = null;

		//		// Temp variables to hold the data as we retrieve it
		//		string serverPath = null, localPath = null;
		//		string latestVersion = null, workspaceVersion = null;
		//		string encoding = null;

		//		// Grab the selected included item from the Pending Changes extensibility object
		//		PendingChangesItem selectedItem = null;
		//		IPendingChangesExt pendingChanges = GetService<IPendingChangesExt>();
		//		if (pendingChanges != null && pendingChanges.SelectedIncludedItems.Length > 0)
		//		{
		//			selectedItem = pendingChanges.SelectedIncludedItems[0];
		//		}

		//		if (selectedItem != null && selectedItem.IsPendingChange && selectedItem.PendingChange != null)
		//		{
		//			// Check for rename
		//			if (selectedItem.PendingChange.IsRename && selectedItem.PendingChange.SourceServerItem != null)
		//			{
		//				serverPath = selectedItem.PendingChange.SourceServerItem;
		//			}
		//			else
		//			{
		//				serverPath = selectedItem.PendingChange.ServerItem;
		//			}

		//			localPath = selectedItem.ItemPath;
		//			workspaceVersion = selectedItem.PendingChange.Version.ToString();
		//			encoding = selectedItem.PendingChange.EncodingName;
		//		}
		//		else
		//		{
		//			serverPath = String.Empty;
		//			localPath = selectedItem != null ? selectedItem.ItemPath : String.Empty;
		//			latestVersion = String.Empty;
		//			workspaceVersion = String.Empty;
		//			encoding = String.Empty;
		//		}

		//		// Go get any missing data from the server
		//		if (latestVersion == null || encoding == null)
		//		{
		//			// Make the server call asynchronously to avoid blocking the UI
		//			await Task.Run(() =>
		//			{
		//				ITeamFoundationContext context = this.CurrentContext;
		//				if (context != null && context.HasCollection)
		//				{
		//					VersionControlServer vcs = context.TeamProjectCollection.GetService<VersionControlServer>();
		//					if (vcs != null)
		//					{
		//						Item item = vcs.GetItem(serverPath);
		//						if (item != null)
		//						{
		//							latestVersion = latestVersion ?? item.ChangesetId.ToString();
		//							encoding = encoding ?? FileType.GetEncodingName(item.Encoding);
		//						}
		//					}
		//				}
		//			});
		//		}

		//		// Now back on the UI thread, update the view data
		//		this.ServerPath = serverPath;
		//		this.LocalPath = localPath;
		//		this.LatestVersion = latestVersion;
		//		this.WorkspaceVersion = workspaceVersion;
		//		this.Encoding = encoding;
		//	}
		//	catch (Exception ex)
		//	{
		//		ShowNotification(ex.Message, NotificationType.Error);
		//	}
		//	finally
		//	{
		//		// Always clear our busy flag when done
		//		this.IsBusy = false;
		//	}
		//}

		///// <summary>
		///// Get/set the server path.
		///// </summary>
		//public string ServerPath
		//{
		//	get { return m_serverPath; }
		//	set { m_serverPath = value; RaisePropertyChanged("ServerPath"); }
		//}
		//private string m_serverPath = String.Empty;

		///// <summary>
		///// Get/set the local path.
		///// </summary>
		//public string LocalPath
		//{
		//	get { return m_localPath; }
		//	set { m_localPath = value; RaisePropertyChanged("LocalPath"); }
		//}
		//private string m_localPath = String.Empty;

		///// <summary>
		///// Get/set the latest version.
		///// </summary>
		//public string LatestVersion
		//{
		//	get { return m_latestVersion; }
		//	set { m_latestVersion = value; RaisePropertyChanged("LatestVersion"); }
		//}
		//private string m_latestVersion = String.Empty;

		///// <summary>
		///// Get/set the workspace version.
		///// </summary>
		//public string WorkspaceVersion
		//{
		//	get { return m_workspaceVersion; }
		//	set { m_workspaceVersion = value; RaisePropertyChanged("WorkspaceVersion"); }
		//}
		//private string m_workspaceVersion = String.Empty;

		///// <summary>
		///// Get/set the encoding.
		///// </summary>
		//public string Encoding
		//{
		//	get { return m_encoding; }
		//	set { m_encoding = value; RaisePropertyChanged("Encoding"); }
		//}
		//private string m_encoding = String.Empty;


		public override bool IsViewAllFilesEnabled
		{
			get { return IsViewIncludedFilesEnabled || IsViewExcludedFilesEnabled; }
		}

		public override bool IsViewSelectedFilesEnabled
		{
			get { return IsVersionControlServiceAvailable && ((_pendingChangesService.SelectedIncludedItems.Length + _pendingChangesService.SelectedExcludedItems.Length) > 0); }
		}

		public override bool IsViewIncludedFilesEnabled
		{
			get { return IsVersionControlServiceAvailable && ((_pendingChangesService.IncludedChanges.Length + _pendingChangesService.FilteredIncludedChanges.Length) > 0); }
		}

		public override bool IsViewExcludedFilesEnabled
		{
			get { return IsVersionControlServiceAvailable && ((_pendingChangesService.ExcludedChanges.Length + _pendingChangesService.FilteredExcludedChanges.Length) > 0); }
		}

		public override bool IsViewIncludedFilesVisible
		{
			get { return true; }
		}

		public override bool IsViewExcludedFilesVisible
		{
			get { return true; }
		}

		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> {CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion};

		public override bool IsVersionControlServiceAvailable
		{
			get { return _isVersionControlAvailable; }
			set
			{
				_isVersionControlAvailable = value;
				NotifyPropertyChanged("IsVersionControlServiceAvailable");
				NotifyPropertyChanged("IsViewAllFilesEnabled");
				NotifyPropertyChanged("IsViewSelectedFilesEnabled");
				NotifyPropertyChanged("IsViewIncludedFilesEnabled");
				NotifyPropertyChanged("IsViewExcludedFilesEnabled");
			}
		}
		private bool _isVersionControlAvailable = false;
	}
}
