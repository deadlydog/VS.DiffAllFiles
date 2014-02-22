using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DansKingdom.VS_DiffAllFiles.Code.Base;
using DansKingdom.VS_DiffAllFiles.Code;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	/// <summary>
	/// Selected file info section.
	/// </summary>
	[TeamExplorerSection(PendingChangesSection.SectionId, TeamExplorerPageIds.PendingChanges, 35)]
	public class PendingChangesSection : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		#region Notify Property Changed
		/// <summary>
		/// Inherited event from INotifyPropertyChanged.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the PropertyChanged event of INotifyPropertyChanged with the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property to fire the event against</param>
		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "D7792573-517F-4B52-898C-CA28E7BDE37E";

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

			// Find the Pending Changes extensibility service and sign up for property change notifications.
			var pendingChangesExtensibility = this.GetService<IPendingChangesExt>();
			if (pendingChangesExtensibility != null)
				pendingChangesExtensibility.PropertyChanged += pendingChangesExtensibility_PropertyChanged;
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			var pendingChangesExtensibility = this.GetService<IPendingChangesExt>();
			if (pendingChangesExtensibility != null)
				pendingChangesExtensibility.PropertyChanged -= pendingChangesExtensibility_PropertyChanged;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		private void pendingChangesExtensibility_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedExcludedItems":
				case "SelectedIncludedItems":
					Refresh();
					break;
			}
		}

		/// <summary>
		/// Refresh override.
		/// </summary>
		public async override void Refresh()
		{
			base.Refresh();
			//await RefreshAsync();
		}

		public async Task CompareIncludedPendingChanges()
		{
			// Set the Busy flag while we work.
			this.IsBusy = true;

			// Get a handle to the Automation Model that we can use to interact with the VS IDE.
			var dte2 = PackageHelper.DTE2;
			if (dte2 == null)
			{
				ShowNotification("Could not get a handle to the DTE2 (the Visual Studio IDE Automation Model).", NotificationType.Error);
				return;
			}

			//// Make sure we have a connection to Team Foundation.
			//ITeamFoundationContext context = this.CurrentContext;
			//if (context == null || !context.HasCollection) return;
			
			//// Make sure we can access the Version Control Server.
			//VersionControlServer versionControlServer = null;
			//await Task.Run(() => versionControlServer = context.TeamProjectCollection.GetService<VersionControlServer>());
			//if (versionControlServer == null) return;

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
			
			// Save the list of pending changes before looping through them.
			// Only grab the files that are not on the list of file extensions to ignore.
			var pendingChanges = GetService<IPendingChangesExt>();
			if (pendingChanges == null) return;
			var includedItemsList = pendingChanges.IncludedChanges.Where(p => !settings.FileExtensionsToIgnore.Contains(System.IO.Path.GetExtension(p.LocalOrServerItem).TrimStart('.'))).ToList();

			// Loop through and diff each of the pending changes.
			foreach (var pendingChange in includedItemsList)
			{
				// If this file type is configured to use an external diff tool.
				if (fileTypesConfiguredToUseExternalDiffTool.Contains(Path.GetExtension(pendingChange.LocalOrServerItem)) ||
					fileTypesConfiguredToUseExternalDiffTool.Contains(".*"))
				{
					// Launch the configured diff tool to diff this file.
					var diffProcess = new System.Diagnostics.Process
					{
						StartInfo =
						{
							FileName = tfFilePath,
							Arguments = string.Format("diff \"{0}\"", pendingChange.LocalOrServerItem),
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
			// Perform the diff using the VS API method to ensure the diff window opens in this instance of VS.
			Difference.VisualDiffItems(pendingChange.VersionControlServer, new DiffItemPendingChangeBase(pendingChange),
				new DiffItemLocalFile(pendingChange.LocalItem, pendingChange.Encoding, pendingChange.CreationDate, false), settings.CompareFilesOneAtATime);

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


		public bool IsViewAllFilesEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsViewSelectedFilesEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsViewIncludedFilesEnabled
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsViewExcludedFilesEnabled
		{
			get { throw new NotImplementedException(); }
		}
	}
}
