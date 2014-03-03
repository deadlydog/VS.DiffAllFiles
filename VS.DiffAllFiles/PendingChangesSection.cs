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

		private CancellationTokenSource _compareTasksCancellationTokenSource = new CancellationTokenSource();

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

			// Save how many files were set to be compared before removing the ones that do not meet the settings requirements.
			int originalNumberOfFilesToCompare = itemsToCompare.Count;

			// Filter out added files if they should be skipped.
			if (!settings.CompareNewFiles)
				itemsToCompare = itemsToCompare.Where(p => p.ChangeType != ChangeType.Add && p.ChangeType != ChangeType.Branch && p.ChangeType != ChangeType.Undelete).ToList();

			// Filter out deleted files if they should be skipped.
			if (!settings.CompareDeletedFiles)
				itemsToCompare = itemsToCompare.Where(p => p.ChangeType != ChangeType.Delete && p.ChangeType != ChangeType.SourceRename).ToList();

			// Filter out files that are on the list of file extensions to ignore.
			itemsToCompare = itemsToCompare.Where(p =>
			{
				// Get the file's extension.
				var extension = System.IO.Path.GetExtension(p.LocalOrServerItem);
				if (string.IsNullOrWhiteSpace(extension)) return false;
				extension = extension.TrimStart('.');

				// Return false if the extension is on the list of file extensions to ignore.
				return !settings.FileExtensionsToIgnoreList.Contains(extension, new DansCSharpLibrary.Comparers.StringComparerIgnoreCase());
			}).ToList();

			// If we are supposed to skip files whose contents have not changed and this file's contents have not changed, skip it and move on to the next change.
			// TOODO: Implement this.

			// Update the number of files compared, and the number of files to be compared.
			NumberOfFilesCompared = 0;
			NumberOfFilesToCompare = itemsToCompare.Count;
			NumberOfFilesSkipped = originalNumberOfFilesToCompare - NumberOfFilesToCompare;

			// Create the lists to hold all of the diff tool windows launched in a single set of files.
			var externalDiffToolProcessIdsRunningFromThisSet = new List<int>();
			var vsDiffToolTabCaptionsStillOpenFromThisSet = new List<string>();

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
						? "T" // Compare against Latest version.
						: string.Format("W\"{0}\";\"{1}\"", _pendingChangesService.Workspace.Name, _pendingChangesService.Workspace.OwnerName); // Compare against Workspace version.

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
					int diffToolProcessId = diffToolProcess.Id;
					externalDiffToolProcessIdsRunningFromThisSet.Add(diffToolProcessId);
					ExternalDiffToolProcessIdsRunning.Add(diffToolProcessId);

					// Start a new Task to monitor for when this external diff tool window is closed, and remove it from our list of running processes.
					Task.Run(() =>
					{
						int processId = diffToolProcessId;
						System.Diagnostics.Process process = null;
						try
						{
							// Loop until the diff tool window is closed.
							do
							{
								System.Threading.Thread.Sleep(100);
								process = System.Diagnostics.Process.GetProcessById(processId);
							} while (!process.HasExited);
						}
						// Catch and eat the exception thrown if the process no longer exists.
						catch (ArgumentException)
						{ }

						// Now that the diff tool window has been closed, remove it from our list of running diff tool windows.
						ExternalDiffToolProcessIdsRunning.Remove(processId);
					});
				}
				// Else this file type is not explicitly configured, so use the default built-in Visual Studio diff tool.
				else
				{
					// Launch the VS diff tool to diff this file.
					var change = pendingChange;
					await Task.Run(() => RunVisualDiff(change, settings, dte2));

					// If the diff tool successfully opened, add this VS diff tool's window name to our list of open VS diff tool tabs.
					string diffToolWindowCaption = dte2.ActiveWindow.Caption;
					if (diffToolWindowCaption.Contains(pendingChange.FileName))
					{
						vsDiffToolTabCaptionsStillOpenFromThisSet.Add(diffToolWindowCaption);
						VsDiffToolTabCaptionsStillOpen.Add(diffToolWindowCaption);

						// Start a new Task to monitor for when this VS diff tool tab is closed, and remove it from our list of open VS diff tool tabs.
						Task.Run(() =>
						{
							string vsDiffToolWindowStillOpenCaption = diffToolWindowCaption;
							bool windowIsStillOpen = true;

							// Keep looping until the VS diff tool tab window is closed.
							do
							{
								// Sleep the thread for a bit before checking to see if the VS Diff Tabs are all closed or not.
								System.Threading.Thread.Sleep(100);

								// Get the list of open Windows in Visual Studio right now.
								var openWindowsInVS = dte2.Windows;

								try
								{
									// Loop through all of the open windows in Visual Studio and see if this VS Diff Tool Tab is still open or not.
									windowIsStillOpen = openWindowsInVS.Cast<Window>().Any(window => window.Caption.Equals(vsDiffToolWindowStillOpenCaption, StringComparison.InvariantCulture));
								}
								// Sometimes a Window is already disposed when we try and access it here, so just eat those exceptions.
								catch
								{ }
							} while (windowIsStillOpen);

							// Now that the diff tool tab window has been closed, remove it from our list.
							VsDiffToolTabCaptionsStillOpen.Remove(vsDiffToolWindowStillOpenCaption);
						});
					}
				}

				// If we have reached the maximum number of diff too instances to launch for this set, and there are still more to launch.
				if (NumberOfFilesCompared % settings.NumberOfFilesToCompareAtATime == 0 && NumberOfFilesCompared < NumberOfFilesToCompare)
				{
					// Get the Cancellation Token to use for the tasks we create.
					var cancellationToken = _compareTasksCancellationTokenSource.Token;

					// Get the Tasks to wait for all of the external diff tool processes to be closed.
					var waitForAllExternalDiffToolProcessFromThisSetToCloseTasks = externalDiffToolProcessIdsRunningFromThisSet.Select(id => Task.Run(() =>
					{
						int processId = id;
						System.Diagnostics.Process process = null;
						try
						{
							// Loop until the diff tool window is closed.
							do
							{
								System.Threading.Thread.Sleep(100);
								process = System.Diagnostics.Process.GetProcessById(processId);
							} while (!cancellationToken.IsCancellationRequested && !process.HasExited);
						}
						// Catch and eat the exception thrown if the process no longer exists.
						catch (ArgumentException)
						{ }
					}, cancellationToken));

					// Get the Task to wait for all of the VS diff tool tabs to closed.
					var waitForAllVsDiffToolTabsFromThisSetToCloseTask = Task.Run(() =>
					{
						// Sleep this thread until the user has closed all of the VS Diff Tool tabs. 
						while (!cancellationToken.IsCancellationRequested && vsDiffToolTabCaptionsStillOpenFromThisSet.Count > 0)
						{
							// Sleep the thread for a bit before checking to see if the VS Diff Tabs are all closed or not.
							System.Threading.Thread.Sleep(100);

							// Get the list of open Windows in Visual Studio right now.
							var openWindowsInVS = dte2.Windows;
							
							// Loop through all of the VS Diff Tool Tabs that we launched and remove from our list any that have been closed.
							// We loop through the list backwards so that we can easily remove closed tabs from our list.
							for (int diffWindowIndex = (vsDiffToolTabCaptionsStillOpenFromThisSet.Count - 1); diffWindowIndex >= 0; diffWindowIndex--)
							{
								// Make sure this item still exists in our list.
								if (diffWindowIndex > (vsDiffToolTabCaptionsStillOpenFromThisSet.Count - 1))
									break;

								string vsDiffToolWindowStillOpenCaption = vsDiffToolTabCaptionsStillOpenFromThisSet[diffWindowIndex];

								try
								{
									// Loop through all of the open windows in Visual Studio and see if this VS Diff Tool Tab is still open or not.
									bool windowIsStillOpen = openWindowsInVS.Cast<Window>().Any(window => window.Caption.Equals(vsDiffToolWindowStillOpenCaption, StringComparison.InvariantCulture));

									// If the VS Diff Tool Tab is no longer open, remove it from our list.
									if (!windowIsStillOpen && diffWindowIndex < vsDiffToolTabCaptionsStillOpenFromThisSet.Count)
										vsDiffToolTabCaptionsStillOpenFromThisSet.RemoveAt(diffWindowIndex);
								}
								// Sometimes a Window is already disposed when we try and access it here, so just break out of the loop so we can refresh our list of VS windows and try again.
								catch
								{
									break;
								}
							}
						}
					});

					// Get the Task to wait for the Next Set Of Files or Cancel buttons to be clicked.
					var waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask = Task.Run(() =>
					{
						// Just sleep this thread until the user wants to compare the next set of files, or cancels the compare operations.
						while (!cancellationToken.IsCancellationRequested && !_compareNextSetOfFiles && !_cancelComparingFiles)
							System.Threading.Thread.Sleep(100);
					});

					// Merge the list of tasks waiting for the external diff tool windows and the VS diff tool tabs to be closed.
					var waitForAllDiffToolWindowsFromThisSetToCloseTasks = new List<Task>(waitForAllExternalDiffToolProcessFromThisSetToCloseTasks);
					waitForAllDiffToolWindowsFromThisSetToCloseTasks.Add(waitForAllVsDiffToolTabsFromThisSetToCloseTask);

					// Wait for all of the diff windows from this set to be closed, or the "Next Set Of Files" or Cancel button to be pressed.
					await Task.WhenAny(Task.WhenAll(waitForAllDiffToolWindowsFromThisSetToCloseTasks), waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask);

					// Now that we are done waiting for this set, cancel any Tasks that are still running and refresh the Cancellation Token Source to use for the next set of Tasks we create.
					_compareTasksCancellationTokenSource.Cancel();
					_compareTasksCancellationTokenSource = new CancellationTokenSource();

					// Now that we are done with this set of compares, clear the list of diff tool windows still open from this set.
					externalDiffToolProcessIdsRunningFromThisSet.Clear();
					vsDiffToolTabCaptionsStillOpenFromThisSet.Clear();

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

		/// <summary>
		/// Closes any diff tool windows that are still open from the compare operations we launched.
		/// </summary>
		public override void CloseAllOpenCompareWindows()
		{
			// Kill all processes hosting the external diff tools we launched.
			// Loop through the list backwards as it may be modified by another task while we loop through it.
			for (int processIndex = (ExternalDiffToolProcessIdsRunning.Count - 1); processIndex >= 0; processIndex--)
			{
				int processId = ExternalDiffToolProcessIdsRunning[processIndex];
				try
				{
					// Get a handle to the TF.exe process.
					var tfProcess = System.Diagnostics.Process.GetProcessById(processId);
					if (!tfProcess.HasExited)
					{
						// Get a handle to the diff tool process created by the TF.exe process and close it.
						foreach (var diffToolProcess in DiffAllFilesHelper.GetChildProcesses(tfProcess))
							diffToolProcess.Kill();
					}
				}
				// Catch any exceptions thrown when the process does not exist.
				catch (ArgumentException)
				{
					// Since the process is already closed, remove it from our list.
					ExternalDiffToolProcessIdsRunning.Remove(processId);
				}
			}

			// Close all windows that we opened using the built-in VS diff tool and are still open.
			// Loop through the list backwards as it may be modified by another task while we loop through it.
			// The windows list index starts at 1, not 0.
			var windows = PackageHelper.DTE2.Windows;
			for (int windowIndex = windows.Count; windowIndex > 0; windowIndex--)
			{
				var window = windows.Item(windowIndex);
				if (VsDiffToolTabCaptionsStillOpen.Contains(window.Caption, new DansCSharpLibrary.Comparers.StringComparerIgnoreCase()))
					window.Close();
			}
		}
	}
}
