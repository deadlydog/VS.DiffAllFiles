using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class TfsDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		/// <summary>
		/// Handle to the Pending Changes Extensibility service.
		/// </summary>
		protected IPendingChangesExt _pendingChangesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="TfsDiffAllFilesSectionBase"/> class.
		/// </summary>
		protected TfsDiffAllFilesSectionBase()
			: base()
		{
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new DiffAllFilesSectionControl(this);
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
				_pendingChangesService.PropertyChanged += _pendingChangesService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (_pendingChangesService != null)
				_pendingChangesService.PropertyChanged -= _pendingChangesService_PropertyChanged;
			_pendingChangesService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		protected virtual void _pendingChangesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IncludedChanges":
				case "FilteredIncludedChanges":
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges":
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "SelectedIncludedItems":
				case "SelectedExcludedItems":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		protected override async Task<bool> GetIfVersionControlServiceIsAvailable()
		{
			// Make sure we have a connection to Team Foundation.
			ITeamFoundationContext context = this.CurrentContext;
			if (context == null || !context.HasCollection)
				return false;

			// Make sure we can access the Version Control Server.
			VersionControlServer versionControlService = null;
			await Task.Run(() => versionControlService = context.TeamProjectCollection.GetService<VersionControlServer>());
			if (versionControlService == null)
				return false;

			// If we got this far we could connect to Version Control.
			return true;
		}

		/// <summary>
		/// Asynchronously launch the diff tools to compare the files.
		/// </summary>
		/// <param name="itemStatusTypesToCompare">The files that should be compared.</param>
		public override async Task PerformItemDiffs(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			try
			{
				// Notify that we are running one of the compare commands.
				IsRunningCompareFilesCommand = true;
				this.IsBusy = true;

				// If we don't have a handle to the Pending Changes service, display an error and exit.
				if (_pendingChangesService == null)
				{
					ShowNotification("Could not get a handle to the Visual Studio extensibility service.", NotificationType.Error);
					return;
				}

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
						itemsToCompare = _pendingChangesService.FilteredIncludedChanges.Length > 0
							? _pendingChangesService.FilteredIncludedChanges.ToList()
							: _pendingChangesService.IncludedChanges.ToList();
						break;

					case ItemStatusTypesToCompare.Excluded:
						// If there are filtered excluded changes, only grab them, else grab all of the excluded changes.
						itemsToCompare = _pendingChangesService.FilteredExcludedChanges.Length > 0
							? _pendingChangesService.FilteredExcludedChanges.ToList()
							: _pendingChangesService.ExcludedChanges.ToList();
						break;
				}

				// Compare all of the files.
				await CompareItems(itemsToCompare);
			}
			catch (Exception ex)
			{
				var errors = DansCSharpLibrary.Exceptions.ExceptionHelper.GetExceptionMessagesBasedOnDebugging(ex);
				ShowNotification(string.Format("Unexpected Error Occurred:\n{0}", errors), NotificationType.Error);
			}
			finally
			{
				// Notify that we are done running one of the compare commands.
				IsRunningCompareFilesCommand = false;
				this.IsBusy = false;
			}
		}

		private CancellationTokenSource _compareTasksCancellationTokenSource = new CancellationTokenSource();

		private async Task CompareItems(List<PendingChange> itemsToCompare)
		{
			// Get a handle to the Automation Model that we can use to interact with the VS IDE.
			var dte2 = PackageHelper.DTE2;
			if (dte2 == null)
			{
				ShowNotification("Could not get a handle to the DTE2 (the Visual Studio IDE Automation Model).", NotificationType.Error);
				return;
			}

			// Get all of the file types that are configured to use an external diff tool, and that diff tool's configuration.
			var diffToolConfigurations = DiffAllFilesHelper.DiffToolsConfigured;

			// Get a handle to the settings to use.
			var settings = DiffAllFilesSettings.CurrentSettings;

			// Save how many files were set to be compared before removing the ones that do not meet the settings requirements.
			int originalNumberOfFilesToCompare = itemsToCompare.Count;

			// Filter out added files if they should be skipped.
			if (!settings.CompareNewFiles)
				itemsToCompare = itemsToCompare.Where(p => !p.IsAdd && !p.IsBranch && !p.IsUndelete).ToList();

			// Filter out deleted files if they should be skipped.
			if (!settings.CompareDeletedFiles)
				itemsToCompare = itemsToCompare.Where(p => !p.IsDelete).ToList();

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

			// Get the version to compare the files against.
			CompareVersion compareVersion = CompareVersionToUse;

			// Clear out any diff tool windows from a previous file Set that are still open, as we will be starting a new Set now.
			lock (ExternalDiffToolProcessIdsRunningLock)
			{ ExternalDiffToolProcessIdsRunningInThisSet.Clear(); }
			lock (VsDiffToolTabCaptionsStillOpenLock)
			{ VsDiffToolTabCaptionsStillOpenInThisSet.Clear(); }
			
			// Create a dictionary to hold all of the diff tool configurations and files required to do a "Combined" diff.
			var combinedDiffToolConfigurationsAndFilePaths = new Dictionary<DiffToolConfiguration, SourceAndTargetFilePathsAndLabels>();

			// Loop through and diff each of the pending changes.
			foreach (var change in itemsToCompare)
			{
				// Copy the pending change to local variable to avoid problems with using it inside of tasks.
				PendingChange pendingChange = change;

				// Increment the number of items that we have compared.
				NumberOfFilesCompared++;

				// Get the source and target files to compare, and the labels to use for each of them.
				SourceAndTargetFilePathsAndLabels filePathsAndLabels = null;
				var tempDiffFilesDirectory = string.Empty;
				try
				{
					await Task.Run(() => GetFilesToDiffAndTheirLabels(pendingChange, compareVersion, out filePathsAndLabels, out tempDiffFilesDirectory));
				}
				// Catch any errors thrown when trying to get the files from source control.
				catch (Exception ex)
				{
					var errors = DansCSharpLibrary.Exceptions.ExceptionHelper.GetExceptionMessagesBasedOnDebugging(ex);
					ShowNotification(string.Format("Unexpected Error Occurred:\n{0}", errors), NotificationType.Error);

					// Move on to the next item to compare.
					continue;
				}

				// Get the diff tool to use for this file's extension.
				var fileExtension = Path.GetExtension(pendingChange.LocalOrServerItem);
				var diffToolConfigurationAndExtension = diffToolConfigurations.FirstOrDefault(d => d.FileExtension.Equals(fileExtension) || d.FileExtension.Equals(".*"));

				// If we should compare the files individually.
				if (Settings.CompareModeToUse == CompareModes.IndividualFiles)
				{
					// If this file type is configured to use an external diff tool.
					if (diffToolConfigurationAndExtension != null)
					{
						// Perform the diff for this file in the configured external tool.
						OpenFileDiffInExternalDiffTool(diffToolConfigurationAndExtension.DiffToolConfiguration, filePathsAndLabels, tempDiffFilesDirectory);
					}
					// Else this file type is not explicitly configured, so use the default built-in Visual Studio diff tool.
					else
					{
						// Perform the diff for this file using the built-in Visual Studio diff tool.
						OpenFileDiffInVsDiffTool(filePathsAndLabels, tempDiffFilesDirectory, pendingChange.FileName, dte2);
					}

					// If we have reached the maximum number of diff tool instances to launch for this set, and there are still more to launch.
					if ((ExternalDiffToolProcessIdsRunningInThisSet.Count + VsDiffToolTabCaptionsStillOpenInThisSet.Count) % settings.NumberOfFilesToCompareAtATime == 0 &&
						NumberOfFilesCompared < NumberOfFilesToCompare)
					{
						bool cancelCompareOperations = !(await WaitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelled(dte2));
						if (cancelCompareOperations)
							break;
					}
				}
				// Else we are combining all of the files together and comparing a single file.
				else
				{
					// Get the Diff Tool Configuration that should be used to diff this file.
					var diffToolConfigKey = (diffToolConfigurationAndExtension == null )
						? DiffToolConfiguration.VsBuiltInDiffToolConfiguration
						: diffToolConfigurationAndExtension.DiffToolConfiguration;

					// If we haven't created the Combine files for this diff tool configuration yet, create them.
					if (!combinedDiffToolConfigurationsAndFilePaths.ContainsKey(diffToolConfigKey))
					{
						// Create the Combined files to hold all of their contents.
						SourceAndTargetFilePathsAndLabels combinedFilePathsAndLabels;
						string combinedTempDiffFilesDirectory;
						GetTemporarySourceAndTargetFilePaths("CombinedFiles", out combinedFilePathsAndLabels, out combinedTempDiffFilesDirectory);

						// Create and apply the labels to use for the Combined files.
						string sourceVersionForLabel = string.Empty;
						switch (this.SectionType)
						{
							case SectionTypes.PendingChanges: sourceVersionForLabel = "Local Changes"; break;
							case SectionTypes.ChangesetDetails: sourceVersionForLabel = string.Format("Changeset {0}", pendingChange.Version); break;
							case SectionTypes.ShelvesetDetails: sourceVersionForLabel = string.Format("Shelveset \"{0}\"", pendingChange.PendingSetName); break;
						}
						string targetVersionForLabel = compareVersion.ToString();
						combinedFilePathsAndLabels = new SourceAndTargetFilePathsAndLabels(combinedFilePathsAndLabels.SourceFilePath, combinedFilePathsAndLabels.TargetFilePath, 
							string.Format("Combined Files: {0}", sourceVersionForLabel), string.Format("Combined Files: {0}", targetVersionForLabel));
						
						// Add this diff tool configuration to our dictionary.
						combinedDiffToolConfigurationsAndFilePaths.Add(diffToolConfigKey, combinedFilePathsAndLabels);
					}

					// Get a shortcut handle to the Combined files to write to.
					var combinedFiles = combinedDiffToolConfigurationsAndFilePaths[diffToolConfigKey];

					// Create the labels to use for the header information.
					var sourceFileLabel = filePathsAndLabels.SourceFileLabel;
					var targetFileLabel = filePathsAndLabels.TargetFileLabel;
					if (settings.UseSameHeaderForBothFiles)
					{
						// Make both headers match and display both pieces of information.
						sourceFileLabel = string.Format("{0} vs. {1}", sourceFileLabel, targetFileLabel);
						targetFileLabel = sourceFileLabel;
					}

					// Get this file's contents and append it to the Combined file's contents, along with some header info.
					var combinedSourceFileContentsToAppend = new StringBuilder();
					combinedSourceFileContentsToAppend.AppendLine("~".PadRight(60, '='));
					combinedSourceFileContentsToAppend.AppendLine(sourceFileLabel);
					combinedSourceFileContentsToAppend.AppendLine("~".PadRight(60, '='));
					combinedSourceFileContentsToAppend.AppendLine(File.ReadAllText(filePathsAndLabels.SourceFilePath));
					File.AppendAllText(combinedFiles.SourceFilePath, combinedSourceFileContentsToAppend.ToString());

					var combinedTargetFileContentsToAppend = new StringBuilder();
					combinedTargetFileContentsToAppend.AppendLine("~".PadRight(60, '='));
					combinedTargetFileContentsToAppend.AppendLine(targetFileLabel);
					combinedTargetFileContentsToAppend.AppendLine("~".PadRight(60, '='));
					combinedTargetFileContentsToAppend.AppendLine(File.ReadAllText(filePathsAndLabels.TargetFilePath));
					File.AppendAllText(combinedFiles.TargetFilePath, combinedTargetFileContentsToAppend.ToString());

					// Delete the original temp files if they still exist.
					if (!string.IsNullOrWhiteSpace(tempDiffFilesDirectory) && Directory.Exists(tempDiffFilesDirectory))
						Directory.Delete(tempDiffFilesDirectory, true);

					// If we have combined all of the files' contents, actually perform the diff now.
					if (NumberOfFilesCompared == NumberOfFilesToCompare)
					{
						foreach (var combinedFileSet in combinedDiffToolConfigurationsAndFilePaths)
						{
							var diffToolConfiguration = combinedFileSet.Key;
							var filesAndLabels = combinedFileSet.Value;
							var tempFilesDirectory = Path.GetDirectoryName(filesAndLabels.SourceFilePath);

							// If this set should be compared in the default Visual Studio diff tool.
							if (diffToolConfiguration == DiffToolConfiguration.VsBuiltInDiffToolConfiguration)
							{
								// Perform the diff for this file using the built-in Visual Studio diff tool.
								OpenFileDiffInVsDiffTool(filesAndLabels, tempFilesDirectory, "Diff All Combined Files", dte2);
							}
							else
							{
								// Perform the diff for this file in the configured external tool.
								OpenFileDiffInExternalDiffTool(diffToolConfiguration, filesAndLabels, tempFilesDirectory);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Waits for all difference tool windows in this set of files to be closed, or for the user to cancel or request the next set of files to be compared.
		/// <para>Returns True if everything finished properly, and False if the user cancelled the operations.</para>
		/// </summary>
		/// <param name="dte2">The dte2.</param>
		private async Task<bool> WaitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelled(DTE2 dte2)
		{
			// Get the Cancellation Token to use for the tasks we create.
			var cancellationToken = _compareTasksCancellationTokenSource.Token;

			// Get the Tasks to wait for all of the external diff tool processes to be closed.
			var waitForAllExternalDiffToolProcessFromThisSetToCloseTasks = ExternalDiffToolProcessIdsRunningInThisSet.Select(processId => Task.Run(() =>
			{
				try
				{
					// Loop until the diff tool window is closed.
					System.Diagnostics.Process process = null;
					do
					{
						System.Threading.Thread.Sleep(DEFAULT_THREAD_SLEEP_TIME);
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
				while (!cancellationToken.IsCancellationRequested && VsDiffToolTabCaptionsStillOpenInThisSet.Count > 0)
				{
					// Sleep the thread for a bit before checking to see if the VS Diff Tabs are all closed or not.
					System.Threading.Thread.Sleep(DEFAULT_THREAD_SLEEP_TIME);

					// Get the list of open Windows in Visual Studio right now.
					var openWindowsInVs = dte2.Windows;

					// Loop through all of the VS Diff Tool Tabs that we launched and remove from our list any that have been closed.
					// We loop through the list backwards so that we can easily remove closed tabs from our list.
					for (int diffWindowIndex = (VsDiffToolTabCaptionsStillOpenInThisSet.Count - 1); diffWindowIndex >= 0; diffWindowIndex--)
					{
						// Make sure this item still exists in our list.
						if (diffWindowIndex > (VsDiffToolTabCaptionsStillOpenInThisSet.Count - 1))
							break;

						string vsDiffToolWindowStillOpenCaption = VsDiffToolTabCaptionsStillOpenInThisSet[diffWindowIndex];

						try
						{
							// Loop through all of the open windows in Visual Studio and see if this VS Diff Tool Tab is still open or not.
							bool windowIsStillOpen = openWindowsInVs.Cast<Window>().Any(window => window.Caption.Equals(vsDiffToolWindowStillOpenCaption));

							// If the VS Diff Tool Tab is no longer open, remove it from our list.
							if (!windowIsStillOpen && diffWindowIndex < VsDiffToolTabCaptionsStillOpenInThisSet.Count)
								VsDiffToolTabCaptionsStillOpenInThisSet.RemoveAt(diffWindowIndex);
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
					System.Threading.Thread.Sleep(DEFAULT_THREAD_SLEEP_TIME);
			});

			// Merge the list of tasks waiting for the external diff tool windows and the VS diff tool tabs to be closed.
			var waitForAllDiffToolWindowsFromThisSetToCloseTasks = new List<Task>(waitForAllExternalDiffToolProcessFromThisSetToCloseTasks);
			waitForAllDiffToolWindowsFromThisSetToCloseTasks.Add(waitForAllVsDiffToolTabsFromThisSetToCloseTask);

			// Wait for all of the diff windows from this set to be closed, or the "Next Set Of Files" or Cancel button to be pressed.
			this.IsBusy = false;
			await Task.WhenAny(Task.WhenAll(waitForAllDiffToolWindowsFromThisSetToCloseTasks), waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask);
			this.IsBusy = true;

			// Now that we are done waiting for this set, cancel any Tasks that are still running and refresh the Cancellation Token Source to use for the next set of Tasks we create.
			_compareTasksCancellationTokenSource.Cancel();
			_compareTasksCancellationTokenSource = new CancellationTokenSource();

			// We are done comparing this set of files, so reset the flag to start comparing the next set of files.
			_compareNextSetOfFiles = false;

			// If the user wants to cancel comparing the rest of the files in this set, break out of the loop so we stop comparing more files.
			if (_cancelComparingFiles)
			{
				_cancelComparingFiles = false;
				return false;
			}

			// Now that we are done with this set of compares, clear the list of diff tool windows still open from this set.
			lock (ExternalDiffToolProcessIdsRunningLock)
			{
				ExternalDiffToolProcessIdsRunningInThisSet.Clear();
			}
			lock (VsDiffToolTabCaptionsStillOpenLock)
			{
				VsDiffToolTabCaptionsStillOpenInThisSet.Clear();
			}

			// Return that everything completed successfully without the user cancelling the operations.
			return true;
		}

		/// <summary>
		/// Opens the file difference in built-in Visual Studio difference tool.
		/// </summary>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory holding the temp files.</param>
		/// <param name="fileName">Name of the file being compared. This will be used as the Visual Studio diff tool Tab's label.</param>
		/// <param name="dte2">The dte2 object we can use to hook into Visual Studio with.</param>
		private void OpenFileDiffInVsDiffTool(SourceAndTargetFilePathsAndLabels filePathsAndLabels, string tempDiffFilesDirectory, string fileName, DTE2 dte2)
		{
			// Get if the user should be able to edit the files to save changes back to them; only allow it if they are not temp files.
			bool sourceIsTemp = filePathsAndLabels.SourceFilePath.StartsWith(tempDiffFilesDirectory);
			bool targetIsTemp = filePathsAndLabels.TargetFilePath.StartsWith(tempDiffFilesDirectory);

			// Strip the Server/Local/Shelved prefix off of the label and use it as the "tag", as the VisualDiffFiles() function will still display a colon even if one is not provided.
			int sourceFileLabelsFirstColonIndex = filePathsAndLabels.SourceFileLabel.IndexOf(':');
			string sourceFileLabelPrefix = filePathsAndLabels.SourceFileLabel.Substring(0, sourceFileLabelsFirstColonIndex);
			string sourceFileLabelWithoutPrefix = filePathsAndLabels.SourceFileLabel.Remove(0, sourceFileLabelsFirstColonIndex + 1).Trim();

			int targetFileLabelsFirstColonIndex = filePathsAndLabels.TargetFileLabel.IndexOf(':');
			string targetFileLabelPrefix = filePathsAndLabels.TargetFileLabel.Substring(0, targetFileLabelsFirstColonIndex);
			string targetFileLabelWithoutPrefix = filePathsAndLabels.TargetFileLabel.Remove(0, targetFileLabelsFirstColonIndex + 1).Trim();

			// Launch the VS diff tool to diff this file.
			Difference.VisualDiffFiles(filePathsAndLabels.SourceFilePath, filePathsAndLabels.TargetFilePath, sourceFileLabelPrefix, targetFileLabelPrefix, sourceFileLabelWithoutPrefix, targetFileLabelWithoutPrefix, sourceIsTemp, targetIsTemp);

			// We are likely opening several diff windows, so make sure they don't just replace one another in the Preview Tab.
			if (PackageHelper.IsCommandAvailable("Window.KeepTabOpen"))
				dte2.ExecuteCommand("Window.KeepTabOpen");

			// If the diff tool successfully opened, add this VS diff tool's window name to our list of open VS diff tool tabs.
			string diffToolWindowCaption = dte2.ActiveWindow.Caption;
			if (diffToolWindowCaption.Contains(fileName))
			{
				lock (VsDiffToolTabCaptionsStillOpenLock)
				{
					VsDiffToolTabCaptionsStillOpen.Add(diffToolWindowCaption);
					VsDiffToolTabCaptionsStillOpenInThisSet.Add(diffToolWindowCaption);
				}

				// Start a new Task to monitor for when this VS diff tool tab is closed, and remove it from our list of open VS diff tool tabs.
				Task.Run(() =>
				{
					string vsDiffToolWindowStillOpenCaption = diffToolWindowCaption;
					var tempDirectory = tempDiffFilesDirectory;
					bool windowIsStillOpen = true;

					// Keep looping until the VS diff tool tab window is closed.
					do
					{
						// Sleep the thread for a bit before checking to see if the VS Diff Tabs are all closed or not.
						System.Threading.Thread.Sleep(DEFAULT_THREAD_SLEEP_TIME);

						// Get the list of open Windows in Visual Studio right now.
						var openWindowsInVS = dte2.Windows;

						try
						{
							// Loop through all of the open windows in Visual Studio and see if this VS Diff Tool Tab is still open or not.
							windowIsStillOpen = openWindowsInVS.Cast<Window>().Any(window => window.Caption.Equals(vsDiffToolWindowStillOpenCaption));
						}
						// Sometimes a Window is already disposed when we try and access it here, so just eat those exceptions.
						catch
						{ }
					} while (windowIsStillOpen);

					// Now that the diff tool tab window has been closed, remove it from our lists.
					lock (VsDiffToolTabCaptionsStillOpenLock)
					{
						if (VsDiffToolTabCaptionsStillOpen.Contains(vsDiffToolWindowStillOpenCaption))
							VsDiffToolTabCaptionsStillOpen.Remove(vsDiffToolWindowStillOpenCaption);
						if (VsDiffToolTabCaptionsStillOpenInThisSet.Contains(vsDiffToolWindowStillOpenCaption))
							VsDiffToolTabCaptionsStillOpenInThisSet.Remove(vsDiffToolWindowStillOpenCaption);
					}

					// Delete the temp files if they still exist.
					if (!string.IsNullOrWhiteSpace(tempDirectory) && Directory.Exists(tempDirectory))
						Directory.Delete(tempDirectory, true);
				});
			}
		}

		/// <summary>
		/// Opens the file difference in external difference tool.
		/// </summary>
		/// <param name="diffToolConfiguration">The difference tool configuration to use to diff the files.</param>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory holding the temp files.</param>
		private void OpenFileDiffInExternalDiffTool(DiffToolConfiguration diffToolConfiguration, SourceAndTargetFilePathsAndLabels filePathsAndLabels, string tempDiffFilesDirectory)
		{
			// Build the arguments to pass to the diff tool's executable.
			string diffToolArguments = diffToolConfiguration.ExecutableArgumentFormat;
			diffToolArguments = diffToolArguments.Replace("%1", string.Format("\"{0}\"", filePathsAndLabels.SourceFilePath));
			diffToolArguments = diffToolArguments.Replace("%2", string.Format("\"{0}\"", filePathsAndLabels.TargetFilePath));
			diffToolArguments = diffToolArguments.Replace("%6", string.Format("\"{0}\"", filePathsAndLabels.SourceFileLabel));
			diffToolArguments = diffToolArguments.Replace("%7", string.Format("\"{0}\"", filePathsAndLabels.TargetFileLabel));

			// Launch the configured diff tool to diff this file.
			var diffToolProcess = new System.Diagnostics.Process
			{
				StartInfo =
				{
					FileName = diffToolConfiguration.ExecutableFilePath,
					Arguments = diffToolArguments,
					CreateNoWindow = true,
					UseShellExecute = false
				}
			};
			diffToolProcess.Start();

			////////////////////////////////
			//// Debugging We Diff Tool Isn't Launched Code Start
			//var diffToolProcess = new System.Diagnostics.Process
			//{
			//	StartInfo =
			//	{
			//		FileName = diffToolConfiguration.ExecutableFilePath,
			//		Arguments = diffToolArguments,
			//		CreateNoWindow = true,
			//		UseShellExecute = false,
			//		RedirectStandardError = true
			//	}
			//};
			//diffToolProcess.Start();
			//string error = diffToolProcess.StandardError.ReadToEnd();
			//diffToolProcess.WaitForExit();
			//// Debugging We Diff Tool Isn't Launched Code Start
			////////////////////////////////

			// Add this process to our list of external diff tool processes currently running.
			int diffToolProcessId = diffToolProcess.Id;
			lock (ExternalDiffToolProcessIdsRunningLock)
			{
				ExternalDiffToolProcessIdsRunning.Add(diffToolProcessId);
				ExternalDiffToolProcessIdsRunningInThisSet.Add(diffToolProcessId);
			}

			// Start a new Task to monitor for when this external diff tool window is closed, and remove it from our list of running processes.
			Task.Run(() =>
			{
				try
				{
					// Loop until the diff tool window is closed.
					System.Diagnostics.Process process = null;
					do
					{
						System.Threading.Thread.Sleep(DEFAULT_THREAD_SLEEP_TIME);
						process = System.Diagnostics.Process.GetProcessById(diffToolProcessId);
					} while (!process.HasExited);
				}
				// Catch and eat the exception thrown if the process no longer exists.
				catch (ArgumentException)
				{ }

				// Now that the diff tool window has been closed, remove it from our lists of running diff tool processes.
				lock (ExternalDiffToolProcessIdsRunningLock)
				{
					if (ExternalDiffToolProcessIdsRunning.Contains(diffToolProcessId))
						ExternalDiffToolProcessIdsRunning.Remove(diffToolProcessId);
					if (ExternalDiffToolProcessIdsRunningInThisSet.Contains(diffToolProcessId))
						ExternalDiffToolProcessIdsRunningInThisSet.Remove(diffToolProcessId);
				}

				// Delete the temp files if they still exist.
				if (!string.IsNullOrWhiteSpace(tempDiffFilesDirectory) && Directory.Exists(tempDiffFilesDirectory))
					Directory.Delete(tempDiffFilesDirectory, true);
			});
		}

		/// <summary>
		/// Downloads the files to diff and the labels that the diff tool should show.
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		/// <param name="compareVersion">The compare version.</param>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory.</param>
		private void GetFilesToDiffAndTheirLabels(PendingChange pendingChange, CompareVersion compareVersion, out SourceAndTargetFilePathsAndLabels filePathsAndLabels, out string tempDiffFilesDirectory)
		{
			// Get the file paths to save the files to.
			GetTemporarySourceAndTargetFilePaths(Path.GetFileName(pendingChange.LocalOrServerItem), out filePathsAndLabels, out tempDiffFilesDirectory);
			string sourceFileLabel = filePathsAndLabels.SourceFilePath;	// This value should never actually be used for the label, but set it in case something gets missed.
			string targetFileLabel = filePathsAndLabels.TargetFilePath;	// This value should never actually be used for the label, but set it in case something gets missed.

			// Create the files as blank files to make sure they exist.
			File.WriteAllText(filePathsAndLabels.SourceFilePath, string.Empty);
			File.WriteAllText(filePathsAndLabels.TargetFilePath, string.Empty);

			// Get the source and target files to use in the compare.
			switch (this.SectionType)
			{
				case SectionTypes.PendingChanges:
					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (pendingChange.IsAdd)
					{
						sourceFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL;
					}
					else
					{
						// Get the source to compare against the file's local changes.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.WorkspaceVersion:
								pendingChange.DownloadBaseFile(filePathsAndLabels.SourceFilePath);
								sourceFileLabel = string.Format("Server: {0};C{1}", pendingChange.ServerItem, pendingChange.Version);
								break;

							case CompareVersion.Values.LatestVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, VersionSpec.Latest, filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};T", pendingChange.ServerItem);
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (pendingChange.IsDelete)
					{
						targetFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL;
					}
					else
					{
						// Get the file's local changes, using the existing local file for the Target file.
						filePathsAndLabels = new SourceAndTargetFilePathsAndLabels(filePathsAndLabels.SourceFilePath, pendingChange.LocalItem);
						targetFileLabel = string.Format("Local: {0}", pendingChange.LocalItem);
					}
					break;

				case SectionTypes.ChangesetDetails:
					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (pendingChange.IsAdd)
					{
						sourceFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL;
					}
					else
					{
						// Get the source to compare against the Changeset's version.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.PreviousVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, new ChangesetVersionSpec(pendingChange.Version - 1), filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};C{1}", pendingChange.ServerItem, pendingChange.Version - 1);
								break;

							case CompareVersion.Values.WorkspaceVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, new WorkspaceVersionSpec(_pendingChangesService.Workspace), filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};W{1}", pendingChange.ServerItem, _pendingChangesService.Workspace.DisplayName);
								break;

							case CompareVersion.Values.LatestVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, VersionSpec.Latest, filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};T", pendingChange.ServerItem);
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (pendingChange.IsDelete)
					{
						targetFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL;
					}
					else
					{
						// Get the Changeset's version of the file.
						if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, new ChangesetVersionSpec(pendingChange.Version), filePathsAndLabels.TargetFilePath, ref targetFileLabel))
							targetFileLabel = string.Format("Server: {0};C{1}", pendingChange.ServerItem, pendingChange.Version);
					}
					break;

				case SectionTypes.ShelvesetDetails:
					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (pendingChange.IsAdd)
					{
						sourceFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL;
					}
					else
					{
						// Get the source to compare against the Shelveset's version.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.UnmodifiedVersion:
								pendingChange.DownloadBaseFile(filePathsAndLabels.SourceFilePath);
								sourceFileLabel = string.Format("Server: {0};C{1}", pendingChange.ServerItem, pendingChange.Version);
								break;

							case CompareVersion.Values.WorkspaceVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, new WorkspaceVersionSpec(_pendingChangesService.Workspace), filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};W{1}", pendingChange.ServerItem, _pendingChangesService.Workspace.DisplayName);
								break;

							case CompareVersion.Values.LatestVersion:
								if (DownloadFileVersionIfItExistsInVersionControl(pendingChange, VersionSpec.Latest, filePathsAndLabels.SourceFilePath, ref sourceFileLabel))
									sourceFileLabel = string.Format("Server: {0};T", pendingChange.ServerItem);
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (pendingChange.IsDelete)
					{
						targetFileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL;
					}
					else
					{
						// Get the Shelveset's version of the file.
						pendingChange.DownloadShelvedFile(filePathsAndLabels.TargetFilePath);
						targetFileLabel = string.Format("Shelved Change: {0};{1}", pendingChange.ServerItem, pendingChange.PendingSetName);
					}
					break;
			}

			// Record the Labels to use with the files.
			filePathsAndLabels = new SourceAndTargetFilePathsAndLabels(filePathsAndLabels.SourceFilePath, filePathsAndLabels.TargetFilePath, sourceFileLabel, targetFileLabel);
		}

		/// <summary>
		/// Gets the temporary source and target file paths for the given file name.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="filePaths">The file paths.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory.</param>
		private static void GetTemporarySourceAndTargetFilePaths(string fileName, out SourceAndTargetFilePathsAndLabels filePaths, out string tempDiffFilesDirectory)
		{
			// Assume we will be downloading both the source and target files from version control into a temp directory, and generate the paths to download them to.
			tempDiffFilesDirectory = Path.Combine(Path.GetTempPath(), "DiffAllFilesTemp", Path.GetRandomFileName());
			Directory.CreateDirectory(tempDiffFilesDirectory);
			string sourceFilePath = Path.Combine(tempDiffFilesDirectory, string.Format("source_{0}", fileName));
			string targetFilePath = Path.Combine(tempDiffFilesDirectory, string.Format("target_{0}", fileName));
			filePaths = new SourceAndTargetFilePathsAndLabels(sourceFilePath, targetFilePath);
		}

		/// <summary>
		/// Downloads the specific version of a file if it exists in version control.
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		/// <param name="versionSpec">The version spec.</param>
		/// <param name="filePathToDownloadTo">The file path to download to.</param>
		/// <param name="fileLabel">If the file is not available to download from the server, this label will be updated to say so.</param>
		private bool DownloadFileVersionIfItExistsInVersionControl(PendingChange pendingChange, VersionSpec versionSpec, string filePathToDownloadTo, ref string fileLabel)
		{
			bool fileDownloaded = true;

			// If the file exists, download it.
			if (pendingChange.VersionControlServer.ServerItemExists(pendingChange.ServerItem, versionSpec, DeletedState.NonDeleted, ItemType.File))
			{
				try
				{
					pendingChange.VersionControlServer.DownloadFile(pendingChange.ServerItem, pendingChange.DeletionId, versionSpec, filePathToDownloadTo);
				}
				catch (Microsoft.TeamFoundation.VersionControl.Client.VersionControlException ex)
				{
					fileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(pendingChange.ServerItem, versionSpec.DisplayString);
					fileDownloaded = false;
				}
			}
			// Else the file doesn't exist so update the file's label to specify that.
			else
			{
				fileLabel = DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(pendingChange.ServerItem, versionSpec.DisplayString);
				fileDownloaded = false;
			}

			// Return if the file was downloaded or not.
			return fileDownloaded;
		}

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
