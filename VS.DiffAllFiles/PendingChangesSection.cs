using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
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
		public const string SectionId = "8C62E1EB-19E1-4652-BD83-817179EF82CD";

		/// <summary>
		/// Handle to the Pending Changes Extensibility service.
		/// </summary>
		private IPendingChangesExt _pendingChangesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="PendingChangesSection"/> class.
		/// </summary>
		public PendingChangesSection() : base()
		{
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new PendingChangesSectionControl(this);
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
					NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges":
					NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "SelectedExcludedItems":
				case "SelectedIncludedItems":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		private readonly List<System.Diagnostics.Process> _externalDiffToolProcessesRunning = new List<System.Diagnostics.Process>();
		private readonly List<string> _vsDiffToolTabsStillOpen = new List<string>();

		public async Task ComparePendingChanges(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			// Set the Busy flag while we work.
			this.IsBusy = true;

			// Notify that we are running one of the compare commands.
			IsRunningCompareFilesCommand = true;

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

			// Get the list of pending changes to compare.
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

			// Filter out added files if they should be skipped.
			if (!settings.CompareNewFiles)
				itemsToCompare = itemsToCompare.Where(p => p.ChangeType != ChangeType.Add && p.ChangeType != ChangeType.Branch && p.ChangeType != ChangeType.Undelete).ToList();

			// Filter out deleted files if they should be skipped.
			if (!settings.CompareDeletedFiles)
				itemsToCompare = itemsToCompare.Where(p => p.ChangeType != ChangeType.Delete && p.ChangeType != ChangeType.SourceRename).ToList();

			// Filter out files that are on the list of file extensions to ignore.
			itemsToCompare = itemsToCompare.Where(p =>
			{
				var extension = System.IO.Path.GetExtension(p.LocalOrServerItem);
				return extension != null && !settings.FileExtensionsToIgnore.Contains(extension.TrimStart('.'));
			}).ToList();

			// If we are supposed to skip files whose contents have not changed and this file's contents have not changed, skip it and move on to the next change.
			// TOODO: Implement this.

			// Update the number of files compared, and the number of files to be compared.
			NumberOfFilesCompared = 0;
			NumberOfFilesToCompare = itemsToCompare.Count;

			// Clear out our list of diff tool windows launched.
			_externalDiffToolProcessesRunning.Clear();
			_vsDiffToolTabsStillOpen.Clear();

			// Loop through and diff each of the pending changes.
			foreach (var pendingChange in itemsToCompare)
			{
				// Increment the number of items that we have compared.
				NumberOfFilesCompared++;

				// If this file type is configured to use an external diff tool.
				if (fileTypesConfiguredToUseExternalDiffTool.Contains(Path.GetExtension(pendingChange.LocalOrServerItem)) ||
					fileTypesConfiguredToUseExternalDiffTool.Contains(".*"))
				{
					// Get the string representation the TF.exe expects for the version to compare against.
					string versionSpecToCompareAgainst = (settings.PendingChangesCompareVersion == CompareVersion.LatestVersion)
						? "T"	// Compare against Latest version.
						: string.Format("W\"{0}\";\"{1}\"", _pendingChangesService.Workspace.Name, _pendingChangesService.Workspace.OwnerName);	// Compare against Workspace version.

					// Launch the configured diff tool to diff this file.
					var diffToolProcess = new System.Diagnostics.Process
					{
						StartInfo =
						{
							FileName = tfFilePath,
							Arguments = string.Format("diff \"{0}\" /version:{1}", pendingChange.LocalOrServerItem, versionSpecToCompareAgainst),
							CreateNoWindow = true,
							UseShellExecute = false
						}
					};
					diffToolProcess.Start();

					// Add this process to our list of external diff tool processes currently running.
					_externalDiffToolProcessesRunning.Add(diffToolProcess);
				}
				// Else this file type is not explicitly configured, so use the default built-in Visual Studio diff tool.
				else
				{
					// Launch the VS diff tool to diff this file.
					var change = pendingChange;
					await Task.Run(() => RunVisualDiff(change, settings, dte2));

					// Add this VS diff tool's window name to our list of open VS diff tool tabs.
					_vsDiffToolTabsStillOpen.Add(dte2.ActiveWindow.Caption);
				}

				// If we have reached the maximum number of diff too instances to launch for this set, and there are still more to launch.
				if (NumberOfFilesCompared % settings.NumberOfFilesToCompareAtATime == 0 && NumberOfFilesCompared < NumberOfFilesToCompare)
				{
					// Get the Tasks to wait for all of the external diff tool processes to be closed.
					var waitForAllExternalDiffToolProcessToCloseTasks = _externalDiffToolProcessesRunning.Select(p => Task.Run(() => p.WaitForExit()));

					// Get the Task to wait for all of the VS diff tool tabs to closed.
					var waitForAllVsDiffToolTabsToCloseTask = Task.Run(() =>
					{
						// Sleep this thread until the user has closed all of the VS Diff Tool tabs. 
						while (_vsDiffToolTabsStillOpen.Count > 0)
						{
							// Sleep the thread for a bit before checking to see if the VS Diff Tabs are all closed or not.
							System.Threading.Thread.Sleep(100);

							// Get the list of open Windows in Visual Studio right now.
							var openWindowsInVS = dte2.Windows;
							
							// Loop through all of the VS Diff Tool Tabs that we launched and remove from our list any that have been closed.
							// We loop through the list backwards so that we can easily remove closed tabs from our list.
							for (int diffWindowIndex = (_vsDiffToolTabsStillOpen.Count - 1); diffWindowIndex >= 0; diffWindowIndex--)
							{
								string vsDiffToolWindowStillOpenCaption = _vsDiffToolTabsStillOpen[diffWindowIndex];

								try
								{
									// Loop through all of the open windows in Visual Studio and see if this VS Diff Tool Tab is still open or not.
									bool windowIsStillOpen = openWindowsInVS.Cast<Window>().Any(window => window.Caption.Equals(vsDiffToolWindowStillOpenCaption, StringComparison.InvariantCulture));

									// If the VS Diff Tool Tab is no longer open, remove it from our list.
									if (!windowIsStillOpen && diffWindowIndex < _vsDiffToolTabsStillOpen.Count)
										_vsDiffToolTabsStillOpen.RemoveAt(diffWindowIndex);
								}
								// Eat any exceptions thrown, as sometimes a Window is already disposed when we try and access it here.
								catch
								{ }
							}
						}
					});

					// Get the Task to wait for the Next Set Of Files or Cancel buttons to be clicked.
					var waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask = Task.Run(() =>
					{
						// Just sleep this thread until the user wants to compare the next set of files, or cancel the compare operations.
						while (!_compareNextSetOfFiles && !_cancelComparingFiles)
							System.Threading.Thread.Sleep(100);
					});

					// Merge the list of tasks waiting for the external and VS diff tool tabs to be closed.
					var waitForAllDiffToolWindowsToCloseTasks = new List<Task>(waitForAllExternalDiffToolProcessToCloseTasks);
					waitForAllDiffToolWindowsToCloseTasks.Add(waitForAllVsDiffToolTabsToCloseTask);

					// Wait for all of the diff windows to be closed, or the "Next Set Of Files" or Cancel button to be pressed.
					await Task.WhenAny(Task.WhenAll(waitForAllDiffToolWindowsToCloseTasks), waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask);

					// We are done comparing this set of files, so reset the flag to start comparing the next set of files.
					_compareNextSetOfFiles = false;

					// If the user wants to cancel comparing the rest of the files in this set, break out of the loop so we stop comparing more files.
					if (_cancelComparingFiles)
					{
						_cancelComparingFiles = false;
						break;
					}
				}
			}

			// Notify that we are done running one of the compare commands.
			IsRunningCompareFilesCommand = false;
			this.IsBusy = false;
		}

		/// <summary>
		/// Runs the built-in Visual Studio Diff Tool in the current instance of Visual Studio.
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="dte2">The dte2.</param>
		private void RunVisualDiff(PendingChange pendingChange, DiffAllFilesSettings settings, DTE2 dte2)
		{
			// Get the Source version to compare the local changes against.
			IDiffItem source = (settings.PendingChangesCompareVersion == CompareVersion.LatestVersion)
				? (IDiffItem)new DiffItemVersionedFile(pendingChange.VersionControlServer, pendingChange.ServerItem, VersionSpec.Latest)
				: new DiffItemPendingChangeBase(pendingChange);

			// Perform the diff using the VS API method to ensure the diff window opens in this instance of VS.
			Difference.VisualDiffItems(pendingChange.VersionControlServer,
				source,
				new DiffItemLocalFile(pendingChange.LocalItem, pendingChange.Encoding, pendingChange.CreationDate, false));

			// We are likely opening several diff windows, so make sure they don't just replace one another in the Preview Tab.
			if (PackageHelper.IsCommandAvailable("Window.KeepTabOpen"))
				dte2.ExecuteCommand("Window.KeepTabOpen");
		}

		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		public override bool IsCompareAllFilesEnabled
		{
			get { return IsCompareIncludedFilesEnabled || IsCompareExcludedFilesEnabled; }
		}

		/// <summary>
		/// Gets if the Compare Selected Files command should be enabled.
		/// </summary>
		public override bool IsCompareSelectedFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && 
				((_pendingChangesService.SelectedIncludedItems.Length + _pendingChangesService.SelectedExcludedItems.Length) > 0); }
		}

		public bool IsCompareIncludedFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && 
				((_pendingChangesService.IncludedChanges.Length + _pendingChangesService.FilteredIncludedChanges.Length) > 0); }
		}

		public bool IsCompareExcludedFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && 
				((_pendingChangesService.ExcludedChanges.Length + _pendingChangesService.FilteredExcludedChanges.Length) > 0); }
		}

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion };

		/// <summary>
		/// Gets if the Version Control provider is available or not.
		/// <para>Commands should be disabled when version control is not available, as it is needed in order to compare files.</para>
		/// </summary>
		public override bool IsVersionControlServiceAvailable
		{
			get { return _isVersionControlAvailable; }
			set
			{
				_isVersionControlAvailable = value;
				NotifyPropertyChanged("IsVersionControlServiceAvailable");
				NotifyPropertyChanged("IsCompareAllFilesEnabled");
				NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
				NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
				NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
			}
		}
		private bool _isVersionControlAvailable = false;

		/// <summary>
		/// Cancel any running operations.
		/// </summary>
		public override void Cancel()
		{
			_cancelComparingFiles = true;
		}
		private bool _cancelComparingFiles = false;

		/// <summary>
		/// Launches the diff tool to compare the next set of files in the currently running compare files set.
		/// </summary>
		public override void CompareNextSetOfFiles()
		{
			_compareNextSetOfFiles = true;
		}
		private bool _compareNextSetOfFiles = false;
	}
}
