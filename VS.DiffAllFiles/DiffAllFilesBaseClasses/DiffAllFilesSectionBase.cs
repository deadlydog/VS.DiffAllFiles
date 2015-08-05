using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using VS_DiffAllFiles.Adapters;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		/// <summary>
		/// List of process IDs hosting any external diff tools that we launched and are still open.
		/// </summary>
		protected readonly ObservableCollection<int> ExternalDiffToolProcessIdsRunning = new ObservableCollection<int>();
		protected static readonly Object ExternalDiffToolProcessIdsRunningLock = new Object();

		/// <summary>
		/// List of Visual Studio window captions of windows hosting any VS diff tools that we launched and are still open.
		/// </summary>
		protected readonly ObservableCollection<string> VsDiffToolTabCaptionsStillOpen = new ObservableCollection<string>();
		protected static readonly Object VsDiffToolTabCaptionsStillOpenLock = new Object();

		/// <summary>
		/// List of process IDs hosting any external diff tools that we launched in the last set of compares and are still open.
		/// </summary>
		protected readonly ObservableCollection<int> ExternalDiffToolProcessIdsRunningInThisSet = new ObservableCollection<int>();

		/// <summary>
		/// List of Visual Studio window captions of windows hosting any VS diff tools that we launched in the last set of compares and are still open.
		/// </summary>
		protected readonly ObservableCollection<string> VsDiffToolTabCaptionsStillOpenInThisSet = new ObservableCollection<string>();

		/// <summary>
		/// Handle to the Pending Changes Extensibility service.
		/// </summary>
		protected IFileChangesService FileChangesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiffAllFilesSectionBase"/> class.
		/// </summary>
		protected DiffAllFilesSectionBase() : base()
		{
			// Listen for changes to the Settings instance, and start watching property changes on the current one.
			DiffAllFilesSettings.CurrentSettingsChanged += DiffAllFilesSettings_CurrentSettingsChanged;
			DiffAllFilesSettings_CurrentSettingsChanged(null, System.EventArgs.Empty);

			// Listen for when we launch new diff tool windows, and when the user closes them.
			ExternalDiffToolProcessIdsRunning.CollectionChanged += DiffToolWindows_CollectionChanged;
			ExternalDiffToolProcessIdsRunningInThisSet.CollectionChanged += DiffToolWindows_CollectionChanged;
			VsDiffToolTabCaptionsStillOpen.CollectionChanged += DiffToolWindows_CollectionChanged;
			VsDiffToolTabCaptionsStillOpenInThisSet.CollectionChanged += DiffToolWindows_CollectionChanged;

			// Set the type of section that this is.
			var sectionType = SectionTypes.None;
			if (this is PendingChangesSection) sectionType = SectionTypes.PendingChanges;
			else if (this is ChangesetDetailsSection) sectionType = SectionTypes.ChangesetDetails;
			else if (this is ShelvesetDetailsSection) sectionType = SectionTypes.ShelvesetDetails;
// VS 2012 doesn't know about anything Git related, as that was all added to be native in VS 2013.
#if (!VS2012)
			else if (this is GitChangesSection) sectionType = SectionTypes.GitChanges;
			else if (this is GitCommitDetailsSection) sectionType = SectionTypes.GitCommitDetails;
#endif
			SectionType = sectionType;

			// Set the section's properties.
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new DiffAllFilesSectionControl(this);
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		public override void Dispose()
		{
			if (ExternalDiffToolProcessIdsRunning != null)
				ExternalDiffToolProcessIdsRunning.CollectionChanged -= DiffToolWindows_CollectionChanged;
			if (ExternalDiffToolProcessIdsRunningInThisSet != null)
				ExternalDiffToolProcessIdsRunningInThisSet.CollectionChanged -= DiffToolWindows_CollectionChanged;
			if (VsDiffToolTabCaptionsStillOpen != null)
				VsDiffToolTabCaptionsStillOpen.CollectionChanged -= DiffToolWindows_CollectionChanged;
			if (VsDiffToolTabCaptionsStillOpenInThisSet != null)
				VsDiffToolTabCaptionsStillOpenInThisSet.CollectionChanged -= DiffToolWindows_CollectionChanged;

			base.Dispose();
		}

		#region Event Handlers

		/// <summary>
		/// Handles the CollectionChanged event of one of the DiffToolWindows lists.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		void DiffToolWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyThatDiffToolWindowCollectionsHaveChanged();
		}

		/// <summary>
		/// Update the UI whenever one of the Diff Tool Window collections have changed.
		/// </summary>
		private void NotifyThatDiffToolWindowCollectionsHaveChanged()
		{
			NotifyPropertyChanged("NumberOfCompareWindowsStillOpen");
			NotifyPropertyChanged("CompareWindowsFromMultipleSetsOfFilesAreOpen");
			NotifyPropertyChanged("CloseLastSetOfFilesCommandLabel");
		}

		/// <summary>
		/// Handles the CurrentSettingsChanged event of the DiffAllFilesSettings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void DiffAllFilesSettings_CurrentSettingsChanged(object sender, System.EventArgs e)
		{
			// Listen for changes to this Settings instance's properties.
			if (Settings != null)
			{
				Settings.PropertyChanged += Settings_PropertyChanged;

				// Notify all of the properties to refresh, since the Settings source was refreshed.
				NotifyPropertyChanged("NextSetOfFilesCommandLabel");
				NotifyPropertyChanged("CompareVersionToUse");
				NotifyPropertyChanged("AllowUserToChooseWhichCompareModeToUse");
				NotifyPropertyChanged("UseCombinedCompareMode");
			}
		}

		/// <summary>
		/// Handles the PropertyChanged event of the Settings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "NumberOfIndividualFilesToCompareAtATime": NotifyPropertyChanged("NextSetOfFilesCommandLabel"); break;

				case "PendingChangesCompareVersion":
				case "ChangesetDetailsCompareVersion":
				case "ShelvesetDetailsCompareVersion":
				case "GitChangesCompareVersion":
				case "GitCommitDetailsCompareVersion":
					NotifyPropertyChanged("CompareVersionToUse"); 
					break;

				case "CompareModesAvailable":
					NotifyPropertyChanged("AllowUserToChooseWhichCompareModeToUse"); 
					NotifyPropertyChanged("UseCombinedCompareMode"); 
					break;
			}
		}

		#endregion

		#region IDiffAllFilesSection Properties
		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		public abstract bool IsCompareAllFilesEnabled { get; }

		/// <summary>
		/// Gets if the Compare Selected Files command should be enabled.
		/// </summary>
		public abstract bool IsCompareSelectedFilesEnabled { get; }

		/// <summary>
		/// Gets if the Compare Included Files command should be enabled.
		/// </summary>
		public virtual bool IsCompareIncludedFilesEnabled
		{
			get { return false; }
		}

		/// <summary>
		/// Gets if the Compare Included Files command should be an option for the user to use.
		/// </summary>
		public virtual bool IsCompareIncludedFilesAvailable
		{
			get { return false; }
		}

		/// <summary>
		/// Gets if the Compare Excluded Files command should be enabled.
		/// </summary>
		public virtual bool IsCompareExcludedFilesEnabled
		{
			get { return false; }
		}

		/// <summary>
		/// Gets if the Compare Excluded Files command should be an option for the user to use.
		/// </summary>
		public virtual bool IsCompareExcludedFilesAvailable
		{
			get { return false; }
		}

		/// <summary>
		/// Gets if one of the commands to compare files is currently running.
		/// </summary>
		public bool IsRunningCompareFilesCommand
		{
			get { return _isRunningCompareFilesCommand; }
			protected set
			{
				_isRunningCompareFilesCommand = value; NotifyPropertyChanged("IsRunningCompareFilesCommand");
				
				// If we are running the Compare Files Command then that means we haven't cancelled it.
				if (_isRunningCompareFilesCommand)
					IsCompareOperationsCancelled = false;
			}
		}
		private bool _isRunningCompareFilesCommand = false;

		/// <summary>
		/// Gets the total number of files that will be compared.
		/// </summary>
		public int NumberOfFilesToCompare
		{
			get { return _numberOfFilesToCompare; }
			set { _numberOfFilesToCompare = value; NotifyPropertyChanged("NumberOfFilesToCompare"); UpdateNumberOfFilesComparedMessages(); }
		}
		private int _numberOfFilesToCompare = 0;

		/// <summary>
		/// Gets the number of files that have been compared already (that have been launched in the diff tool).
		/// </summary>
		public int NumberOfFilesCompared
		{
			get { return _numberOfFilesCompared; }
			set { _numberOfFilesCompared = value; NotifyPropertyChanged("NumberOfFilesCompared"); UpdateNumberOfFilesComparedMessages(); }
		}
		private int _numberOfFilesCompared = 0;

		/// <summary>
		/// Gets the number of file comparisons skipped due to the current settings configuration.
		/// </summary>
		public int NumberOfFilesSkipped
		{
			get { return _numberOfFilesSkipped; }
			set { _numberOfFilesSkipped = value; NotifyPropertyChanged("NumberOfFilesSkipped"); UpdateNumberOfFilesComparedMessages(); }
		}
		private int _numberOfFilesSkipped = 0;

		/// <summary>
		/// Updates any messages that relate to the number of files compared.
		/// </summary>
		private void UpdateNumberOfFilesComparedMessages()
		{
			NotifyPropertyChanged("FileComparisonProgressMessage");
			NotifyPropertyChanged("NextSetOfFilesCommandLabel");
			NotifyPropertyChanged("CloseLastSetOfFilesCommandLabel");
		}

		/// <summary>
		/// Gets a user-friendly message describing how much progress has been made on comparing all of the files.
		/// </summary>
		public string FileComparisonProgressMessage
		{
			get
			{
				// Get a user-friendly string describing how many files have been compared, how many there are to compare, and how many were skipped.
				string message = string.Format("{0} {1} of {2} files.", UseCombinedCompareMode ? "Combined" : "Compared", NumberOfFilesCompared, NumberOfFilesToCompare);
				if (NumberOfFilesSkipped > 0)
					message += string.Format(" Skipped {0}.", NumberOfFilesSkipped);
				if (IsCompareOperationsCancelled)
					message += " Compare cancelled.";

				// If we haven't compared any files yet, change the message to be more user-friendly.
				if (NumberOfFilesCompared == 0 && NumberOfFilesToCompare == 0)
					message = "No files compared yet.";

				return message;
			}
		}

		/// <summary>
		/// Gets a user-friendly message that describes the File Comparison Progress Message and lists the files that were skipped and why.
		/// </summary>
		public string FileComparisonProgressMessageToolTip
		{
			get
			{
				var message = new StringBuilder();
				message.AppendLine("How many file comparisons have been launched so far.");
				message.Append("Files may be skipped due to the settings configuration.");
				return message.ToString();
			}
		}

		/// <summary>
		/// Gets a user-friendly label to use for the command used to compare the next set of files.
		/// </summary>
		public string NextSetOfFilesCommandLabel
		{
			get
			{
				int numberOfFilesToCompareNextSet = Math.Min(Settings.NumberOfIndividualFilesToCompareAtATime, (NumberOfFilesToCompare - NumberOfFilesCompared));
				return (Settings != null && numberOfFilesToCompareNextSet > 1) ? string.Format("Next {0} Files", numberOfFilesToCompareNextSet) : "Next File"; 
			}
		}

		/// <summary>
		/// Gets a user-friendly label to use for the command used to close the last set of files.
		/// </summary>
		public string CloseLastSetOfFilesCommandLabel
		{
			get
			{
				int numberOfFilesStillOpenFromLastSet = ExternalDiffToolProcessIdsRunningInThisSet.Count + VsDiffToolTabCaptionsStillOpenInThisSet.Count;
				return (numberOfFilesStillOpenFromLastSet > 1) ? string.Format("Close Last {0} Files", numberOfFilesStillOpenFromLastSet) : "Close Last File";
			}
		}

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public abstract IEnumerable<CompareVersion> CompareVersions { get; }

		/// <summary>
		/// The file version to compare against.
		/// </summary>
		public CompareVersion CompareVersionToUse
		{
			get
			{
				CompareVersion compareVersionToUse = CompareVersion.UnmodifiedVersion;
				if (SectionType == SectionTypes.PendingChanges) compareVersionToUse = Settings.PendingChangesCompareVersion;
				else if (SectionType == SectionTypes.ChangesetDetails) compareVersionToUse = Settings.ChangesetDetailsCompareVersion;
				else if (SectionType == SectionTypes.ShelvesetDetails) compareVersionToUse = Settings.ShelvesetDetailsCompareVersion;
// VS 2012 doesn't know about anything Git related, as that was all added to be native in VS 2013.
#if (!VS2012)
				else if (SectionType == SectionTypes.GitChanges) compareVersionToUse = Settings.GitChangesCompareVersion;
				else if (SectionType == SectionTypes.GitCommitDetails) compareVersionToUse = Settings.GitCommitDetailsCompareVersion;
#endif
				return compareVersionToUse;
			}

			set
			{
				CompareVersion compareVersionToUse = value;
				if (SectionType == SectionTypes.PendingChanges) Settings.PendingChangesCompareVersion = compareVersionToUse;
				else if (SectionType == SectionTypes.ChangesetDetails) Settings.ChangesetDetailsCompareVersion = compareVersionToUse;
				else if (SectionType == SectionTypes.ShelvesetDetails) Settings.ShelvesetDetailsCompareVersion = compareVersionToUse;
// VS 2012 doesn't know about anything Git related, as that was all added to be native in VS 2013.
#if (!VS2012)
				else if (SectionType == SectionTypes.GitChanges) Settings.GitChangesCompareVersion = compareVersionToUse;
				else if (SectionType == SectionTypes.GitCommitDetails) Settings.GitCommitDetailsCompareVersion = compareVersionToUse;
#endif
			}
		}

		/// <summary>
		/// Gets if the Version Control provider is available or not.
		/// <para>Commands should be disabled when version control is not available, as it is needed in order to compare files.</para>
		/// </summary>
		public bool IsVersionControlServiceAvailable
		{
			get { return _isVersionControlAvailable; }
			set { _isVersionControlAvailable = value; NotifyPropertyChanged("IsVersionControlServiceAvailable"); }
		}
		private bool _isVersionControlAvailable = false;

		/// <summary>
		/// Gets the Settings to use.
		/// </summary>
		public DiffAllFilesSettings Settings
		{
			get { return DiffAllFilesSettings.CurrentSettings; }
		}

		/// <summary>
		/// Refresh the section contents.
		/// </summary>
		public override async void Refresh()
		{
			base.Refresh();

			// Make sure we can still connect to the version control before refreshing any other child controls.
			IsVersionControlServiceAvailable = await GetIfVersionControlServiceIsAvailable();

			// Refresh the commands common to all sections.
			NotifyPropertyChanged("IsCompareAllFilesEnabled");
			NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
		}

		/// <summary>
		/// Gets if the version control service is available or not.
		/// </summary>
		protected abstract Task<bool> GetIfVersionControlServiceIsAvailable();

		/// <summary>
		/// Cancel any running operations.
		/// </summary>
		public override void Cancel()
		{
			base.Cancel();
			IsCompareOperationsCancelled = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the last compare operations were cancelled or not.
		/// </summary>
		public bool IsCompareOperationsCancelled
		{
			get { return _isCompareOperationsCancelled; }
			set { _isCompareOperationsCancelled = value; NotifyPropertyChanged("IsCompareOperationsCancelled"); NotifyPropertyChanged("FileComparisonProgressMessage"); }
		}
		bool _isCompareOperationsCancelled = false;

		/// <summary>
		/// Launches the diff tool to compare the next set of files in the currently running compare files set.
		/// </summary>
		public void CompareNextSetOfFiles()
		{
			_compareNextSetOfFiles = true;
		}
		private bool _compareNextSetOfFiles = false;

		/// <summary>
		/// Closes any diff tool windows that are still open from the compare operations we launched.
		/// </summary>
		public void CloseAllOpenCompareWindows()
		{
			CloseAllOpenWindows(ExternalDiffToolProcessIdsRunning, VsDiffToolTabCaptionsStillOpen);
		}

		/// <summary>
		/// Closes any diff tool windows that are still open from the last set of compare operations we launched.
		/// </summary>
		public void CloseAllOpenCompareWindowsInThisSet()
		{
			CloseAllOpenWindows(ExternalDiffToolProcessIdsRunningInThisSet, VsDiffToolTabCaptionsStillOpenInThisSet);
		}

		private void CloseAllOpenWindows(IList<int> externalDiffToolProcessIds, IList<string> vsDiffToolTabCaptionsStillOpen)
		{
			// Close any open compare windows in the given lists. 
			// We don't need to remove the windows from the given lists here, as they will remove themselves from the list when they are closed.

			// Kill all processes hosting the external diff tools we launched.
			// Loop through a copy of the list, as the list will be modified as we close processes.
			var processes = externalDiffToolProcessIds.ToList();
			for (int processIndex = (processes.Count - 1); processIndex >= 0; processIndex--)
			{
				int processId = processes[processIndex];
				try
				{
					// Get a handle to the diff tool process.
					var diffToolProcess = System.Diagnostics.Process.GetProcessById(processId);
					diffToolProcess.CloseMainWindow();	// Use CloseMainWindow() instead of Kill() to give user a chance to save any changes before it closes.
				}
				// Eat any exceptions thrown when the process does not exist.
				catch (ArgumentException)
				{ }
			}

			// Close all windows that we opened using the built-in VS diff tool and are still open.
			// Loop through the list backwards as it will be modified while we loop through it as we close windows.
			// The windows Item list index starts at 1, not 0.
			var windows = PackageHelper.DTE2.Windows;
			var vsDiffToolCaptionsToLookFor = vsDiffToolTabCaptionsStillOpen.ToList();	// Create a copy of the list so we can remove from it safely when a matching window is found; this avoids us removing too many windows when multiple have the same name.
			for (int windowIndex = windows.Count; windowIndex > 0; windowIndex--)
			{
				var window = windows.Item(windowIndex);
				if (vsDiffToolCaptionsToLookFor.Contains(window.Caption))
				{
					vsDiffToolCaptionsToLookFor.Remove(window.Caption);
					window.Close();
				}
			}
		}

		/// <summary>
		/// Gets the number of diff tool windows that we launched and are still open.
		/// </summary>
		public int NumberOfCompareWindowsStillOpen
		{
			get { return ExternalDiffToolProcessIdsRunning.Count + VsDiffToolTabCaptionsStillOpen.Count; }
		}

		/// <summary>
		/// Gets if there are diff tool windows from multiple file sets currently open.
		/// </summary>
		public bool CompareWindowsFromMultipleSetsOfFilesAreOpen
		{
			get
			{
				return (ExternalDiffToolProcessIdsRunningInThisSet.Count + VsDiffToolTabCaptionsStillOpenInThisSet.Count > 0) && 
					(ExternalDiffToolProcessIdsRunning.Count != ExternalDiffToolProcessIdsRunningInThisSet.Count ||
					VsDiffToolTabCaptionsStillOpen.Count != VsDiffToolTabCaptionsStillOpenInThisSet.Count);
			}
		}

		/// <summary>
		/// Returns the type of section this class is.
		/// </summary>
		protected SectionTypes SectionType { get; private set; }

		/// <summary>
		/// Get if the user should be able to choose which Compare Mode to use from the UI or not.
		/// </summary>
		public bool AllowUserToChooseWhichCompareModeToUse { get { return Settings.CompareModesAvailable == CompareModes.AllowUserToChoose; } }

		/// <summary>
		/// Gets or sets if the Combined Into Single File Compare Mode should be used or not.
		/// </summary>
		public bool UseCombinedCompareMode
		{
			get { return Settings.CompareModeToUse == CompareModes.CombinedIntoSingleFile; }
			set { Settings.CompareModeToUse = (value ? CompareModes.CombinedIntoSingleFile : CompareModes.IndividualFiles); NotifyPropertyChanged("UseCombinedCompareMode"); }
		}

		#endregion

		#region Perform Difference Code

		/// <summary>
		/// Asynchronously launch the diff tools to compare the files.
		/// </summary>
		/// <param name="itemStatusTypesToCompare">The files that should be compared.</param>
		public async Task PerformItemDiffs(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			try
			{
				// Notify that we are running one of the compare commands.
				IsRunningCompareFilesCommand = true;
				this.IsBusy = true;

				// Compare all of the files.
				await CompareItems(itemStatusTypesToCompare);
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

		/// <summary>
		/// Retrieves the File Change items and then compares them.
		/// </summary>
		/// <param name="itemStatusTypesToCompare">The item status types to compare.</param>
		/// <returns></returns>
		private async Task CompareItems(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			// If we don't have a handle to the File Changes Service, display an error and exit.
			if (FileChangesService == null)
			{
				ShowNotification("Could not get a handle to the Visual Studio extensibility service.", NotificationType.Error);
				return;
			}

			// Get the list of pending changes to compare.
			List<IFileChange> itemsToCompare = null;
			switch (itemStatusTypesToCompare)
			{
				case ItemStatusTypesToCompare.All:
					itemsToCompare = FileChangesService.IncludedChanges.Union(FileChangesService.ExcludedChanges).ToList();
					break;

				case ItemStatusTypesToCompare.Selected:
					itemsToCompare = FileChangesService.SelectedIncludedChanges.Union(FileChangesService.SelectedExcludedChanges).ToList();
					break;

				case ItemStatusTypesToCompare.Included:
					// If there are filtered included changes, only grab them, else grab all of the included changes.
					itemsToCompare = FileChangesService.IncludedChanges.ToList();
					break;

				case ItemStatusTypesToCompare.Excluded:
					// If there are filtered excluded changes, only grab them, else grab all of the excluded changes.
					itemsToCompare = FileChangesService.ExcludedChanges.ToList();
					break;
			}

			// Compare all of the files.
			await CompareItems(itemsToCompare);
		}

		/// <summary>
		/// Downloads and compares the given list of items.
		/// </summary>
		/// <param name="itemsToCompare">The items to compare.</param>
		private async Task CompareItems(List<IFileChange> itemsToCompare)
		{
			// Get a handle to the Automation Model that we can use to interact with the VS IDE.
			var dte2 = PackageHelper.DTE2;
			if (dte2 == null)
			{
				ShowNotification("Could not get a handle to the DTE2 (the Visual Studio IDE Automation Model).", NotificationType.Error);
				return;
			}

			// Get all of the file types that are configured to use an external diff tool, and that diff tool's configuration.
			List<FileExtensionDiffToolConfiguration> diffToolConfigurations;
			switch (SectionType)
			{
				// If using Git source control, get the configured Git tools.
				case SectionTypes.GitChanges:
				case SectionTypes.GitCommitDetails:
					diffToolConfigurations = DiffAllFilesHelper.GitDiffToolsConfigured;
					break;

				// Else using TFS source control, so get the configured TFS tools.
				default:
					diffToolConfigurations = DiffAllFilesHelper.TfsDiffToolsConfigured;
					break;
			}

			// Get a handle to the settings to use.
			var settings = DiffAllFilesSettings.CurrentSettings;

			// Save how many files were set to be compared before removing the ones that do not meet the settings requirements.
			int originalNumberOfFilesToCompare = itemsToCompare.Count;

			// Filter out directories, as we can only compare files (not sure how to detect directories only on the server without downloading them).
			itemsToCompare = itemsToCompare.Where(p => !Directory.Exists(p.LocalFilePath ?? string.Empty)).ToList();

			// Filter out added files if they should be skipped.
			if (!settings.CompareNewFiles)
				itemsToCompare = itemsToCompare.Where(p =>
				{
					// If the file is being added to source control, exclude it.
					if (p.IsAdd)
						return false;

					// If the file is not a TFS File Change, there is nothing else to check, so include it.
					var tfsFileChange = p as ITfsFileChange;
					if (tfsFileChange == null)
						return true;

					// This is a TFS File Change, so include it if it is not a Branch and not an Undelete, since those are technically both Adds.
					return !tfsFileChange.IsBranch && !tfsFileChange.IsUndelete;
				}).ToList();

			// Filter out deleted files if they should be skipped.
			if (!settings.CompareDeletedFiles)
				itemsToCompare = itemsToCompare.Where(p => !p.IsDelete).ToList();

			// Filter out files that are on the list of file extensions to ignore.
			itemsToCompare = itemsToCompare.Where(p =>
			{
				// Get the file's extension.
				var extension = System.IO.Path.GetExtension(p.LocalOrServerFilePath);
				if (string.IsNullOrWhiteSpace(extension)) return false;
				extension = extension.TrimStart('.');

				// Return false if the extension is on the list of file extensions to ignore.
				return !settings.FileExtensionsToIgnoreList.Contains(extension, new DansCSharpLibrary.Comparers.StringComparerIgnoreCase());
			}).ToList();

			// If we are supposed to skip files whose contents have not changed and this file's contents have not changed, skip it and move on to the next change.
			// TODO: Implement this.

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

			// Loop through and diff each of the changes.
			foreach (var change in itemsToCompare)
			{
				// Copy the pending change to local variable to avoid problems with using it inside of tasks.
				IFileChange pendingChange = change;

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
				var fileExtension = Path.GetExtension(pendingChange.LocalOrServerFilePath);
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
						OpenFileDiffInVsDiffTool(filePathsAndLabels, tempDiffFilesDirectory, Path.GetFileName(pendingChange.LocalOrServerFilePath), dte2);
					}

					// Stop processing on the UI thread very briefly in order to check and process if the Cancel button was clicked, and to allow any UI updates to be shown.
					await Task.Delay(DiffAllFilesHelper.DEFAULT_UI_THREAD_AWAIT_TIME_TO_ALLOW_OTHER_UI_OPERATIONS_TO_PROCESS);

					// If we have reached the maximum number of diff tool instances to launch for this set AND there are still more to launch, OR the user cancelled the compare operations.
					if (((ExternalDiffToolProcessIdsRunningInThisSet.Count + VsDiffToolTabCaptionsStillOpenInThisSet.Count) % settings.NumberOfIndividualFilesToCompareAtATime == 0 &&
						NumberOfFilesCompared < NumberOfFilesToCompare) || IsCompareOperationsCancelled)
					{
						await WaitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelled(dte2);
					}

					// If the user wants to cancel comparing the rest of the files in this set, break out of the loop so we stop comparing more files.
					if (IsCompareOperationsCancelled)
						break;
				}
				// Else we are combining all of the files together and comparing a single file.
				else
				{
					// Append the source and target file's contents to the Combined files asynchronously.
					await AppendFilesContentsToCombinedFiles(diffToolConfigurationAndExtension, combinedDiffToolConfigurationsAndFilePaths, compareVersion,
						pendingChange, filePathsAndLabels, settings, tempDiffFilesDirectory);

					// If we have finished creating all of the Combined files, or the user has cancelled the combine process, actually perform the diff now with the combined files.
					if (NumberOfFilesCompared == NumberOfFilesToCompare || IsCompareOperationsCancelled)
					{
						// Launch all of the diff tools at the same time.
						await OpenAllCombinedFilesInTheirRespectiveDiffTools(combinedDiffToolConfigurationsAndFilePaths, dte2);
					}

					// If the user wants to cancel comparing the rest of the files in this set, break out of the loop so we stop comparing more files.
					if (IsCompareOperationsCancelled)
						break;
				}
			}
		}

		/// <summary>
		/// Opens all of the Combined files in their respective difference tools.
		/// </summary>
		/// <param name="combinedDiffToolConfigurationsAndFilePaths">The combined difference tool configurations and file paths.</param>
		/// <param name="dte2">The dte2.</param>
		private async Task OpenAllCombinedFilesInTheirRespectiveDiffTools(Dictionary<DiffToolConfiguration, SourceAndTargetFilePathsAndLabels> combinedDiffToolConfigurationsAndFilePaths, DTE2 dte2)
		{
			// Loop through each diff tool to use and launch it.
			foreach (var combinedFileSet in combinedDiffToolConfigurationsAndFilePaths)
			{
				var diffToolConfiguration = combinedFileSet.Key;
				var filesAndLabels = combinedFileSet.Value;
				var tempFilesDirectory = Path.GetDirectoryName(filesAndLabels.SourceFilePathAndLabel.FilePath);

				// If this set should be compared in the default Visual Studio diff tool.
				if (diffToolConfiguration == DiffToolConfiguration.VsBuiltInDiffToolConfiguration)
				{
					// Perform the diff for this file using the built-in Visual Studio diff tool.
					OpenFileDiffInVsDiffTool(filesAndLabels, tempFilesDirectory, string.Empty, dte2);
				}
				else
				{
					// Perform the diff for this file in the configured external tool.
					OpenFileDiffInExternalDiffTool(diffToolConfiguration, filesAndLabels, tempFilesDirectory);
				}
			}

			// Since we compare all of the files at once we don't have separate "file sets", so clear our Set lists out.
			lock (ExternalDiffToolProcessIdsRunningLock)
			{
				ExternalDiffToolProcessIdsRunningInThisSet.Clear();
			}
			lock (VsDiffToolTabCaptionsStillOpenLock)
			{
				VsDiffToolTabCaptionsStillOpenInThisSet.Clear();
			}
		}

		/// <summary>
		/// Creates the Combined files required and appends the Source and Target files contents to them.
		/// </summary>
		/// <param name="diffToolConfigurationAndExtension">The difference tool configuration and extension.</param>
		/// <param name="combinedDiffToolConfigurationsAndFilePaths">The combined difference tool configurations and file paths.</param>
		/// <param name="compareVersion">The compare version.</param>
		/// <param name="fileChange">The file change.</param>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory.</param>
		private async Task AppendFilesContentsToCombinedFiles(FileExtensionDiffToolConfiguration diffToolConfigurationAndExtension, Dictionary<DiffToolConfiguration, SourceAndTargetFilePathsAndLabels> combinedDiffToolConfigurationsAndFilePaths,
			CompareVersion compareVersion, IFileChange fileChange, SourceAndTargetFilePathsAndLabels filePathsAndLabels, DiffAllFilesSettings settings, string tempDiffFilesDirectory)
		{
			// Get the Diff Tool Configuration that should be used to diff this file.
			var diffToolConfigKey = (diffToolConfigurationAndExtension == null)
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
				string sourceVersionForLabel = compareVersion.ToString();
				string targetVersionForLabel = string.Empty;
				switch (this.SectionType)
				{
					case SectionTypes.PendingChanges:
						targetVersionForLabel = "Local Changes";
						break;
					case SectionTypes.ChangesetDetails:
						targetVersionForLabel = string.Format("Changeset {0}", (fileChange as ITfsFileChange).Version);
						break;
					case SectionTypes.ShelvesetDetails:
						targetVersionForLabel = string.Format("Shelveset '{0}'", (fileChange as ITfsFileChange).ShelvesetName);
						break;

					case SectionTypes.GitChanges:
						targetVersionForLabel = "Local Changes";
						break;
					case SectionTypes.GitCommitDetails:
						targetVersionForLabel = string.Format("SHA {0}", (fileChange as IGitFileChange).Version);
						break;
				}
				combinedFilePathsAndLabels = combinedFilePathsAndLabels.GetCopyWithNewFileLabels(new FileLabel("Combined Files", string.Empty, sourceVersionForLabel),
					new FileLabel("Combined Files", string.Empty, targetVersionForLabel));

				// Add this diff tool configuration to our dictionary.
				combinedDiffToolConfigurationsAndFilePaths.Add(diffToolConfigKey, combinedFilePathsAndLabels);
			}

			// Get a shortcut handle to the Combined files to write to.
			var combinedFiles = combinedDiffToolConfigurationsAndFilePaths[diffToolConfigKey];

			// Create the labels to use for the header information.
			var sourceFileLabelString = filePathsAndLabels.SourceFilePathAndLabel.FileLabel.ToString();
			var targetFileLabelString = filePathsAndLabels.TargetFilePathAndLabel.FileLabel.ToString();
			// If the file headers for the Source and Target should match, use the same header for both.
			if (settings.UseSameHeadersForCombinedFiles)
			{
				var fileLabelStringToUseForBothFiles = string.Format("{0} vs. {1}", sourceFileLabelString, targetFileLabelString);

				// If the Prefix and FilePath for both labels match, just include them once and display the different versions.
				if (filePathsAndLabels.SourceFilePathAndLabel.FileLabel.Prefix.Equals(filePathsAndLabels.TargetFilePathAndLabel.FileLabel.Prefix) &&
					filePathsAndLabels.SourceFilePathAndLabel.FileLabel.FilePath.Equals(filePathsAndLabels.TargetFilePathAndLabel.FileLabel.FilePath))
				{
					fileLabelStringToUseForBothFiles = string.Format("{0} vs. {1}", sourceFileLabelString, filePathsAndLabels.TargetFilePathAndLabel.FileLabel.Version);
				}

				// Make both headers match and display both pieces of information.
				sourceFileLabelString = fileLabelStringToUseForBothFiles;
				targetFileLabelString = fileLabelStringToUseForBothFiles;
			}

			// Do heavy I/O operations asynchronously.
			await Task.Run(() =>
			{
				bool wasAbleToReadFileContents = true;

				// Get the original files' contents.
				string sourceFileContents = string.Empty;
				try
				{
					sourceFileContents = File.ReadAllText(filePathsAndLabels.SourceFilePathAndLabel.FilePath);
				}
				catch
				{
					ShowNotification(string.Format("Unable to access file path '{0}', so it will not be compared.", filePathsAndLabels.SourceFilePathAndLabel.FilePath), NotificationType.Warning);
					wasAbleToReadFileContents = false;
				}

				string targetFileContents = string.Empty;
				try
				{
					targetFileContents = File.ReadAllText(filePathsAndLabels.TargetFilePathAndLabel.FilePath);
				}
				catch
				{
					ShowNotification(string.Format("Unable to access file path '{0}', so it will not be compared.", filePathsAndLabels.TargetFilePathAndLabel.FilePath), NotificationType.Warning);
					wasAbleToReadFileContents = false;
				}

				// Now that we've read the original temp files contents in, delete them if they still exist.
				if (!string.IsNullOrWhiteSpace(tempDiffFilesDirectory) && Directory.Exists(tempDiffFilesDirectory))
					Directory.Delete(tempDiffFilesDirectory, true);

				// If we were not able to get the file contents, just exit since we have nothing to append to the Combined files.
				if (!wasAbleToReadFileContents)
					return;

				// Append the source file's contents to the Combined file's contents, along with some header info.
				var combinedSourceFileContentsToAppend = new StringBuilder();
				combinedSourceFileContentsToAppend.AppendLine("~".PadRight(60, '='));
				combinedSourceFileContentsToAppend.AppendLine(sourceFileLabelString);
				combinedSourceFileContentsToAppend.AppendLine("~".PadRight(60, '='));
				combinedSourceFileContentsToAppend.AppendLine(sourceFileContents);
				File.AppendAllText(combinedFiles.SourceFilePathAndLabel.FilePath, combinedSourceFileContentsToAppend.ToString());

				// Append the target file's contents to the Combined file's contents, along with some header info.
				var combinedTargetFileContentsToAppend = new StringBuilder();
				combinedTargetFileContentsToAppend.AppendLine("~".PadRight(60, '='));
				combinedTargetFileContentsToAppend.AppendLine(targetFileLabelString);
				combinedTargetFileContentsToAppend.AppendLine("~".PadRight(60, '='));
				combinedTargetFileContentsToAppend.AppendLine(targetFileContents);
				File.AppendAllText(combinedFiles.TargetFilePathAndLabel.FilePath, combinedTargetFileContentsToAppend.ToString());
			});
		}

		/// <summary>
		/// Waits for all difference tool windows in this set of files to be closed, or for the user to cancel or request the next set of files to be compared.
		/// <para>Returns True if everything finished properly, and False if the user cancelled the operations.</para>
		/// </summary>
		/// <param name="dte2">The dte2.</param>
		private async Task WaitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelled(DTE2 dte2)
		{
			// Get the Cancellation Token to use for the tasks we create.
			var cancellationToken = _waitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelledCancellationTokenSource.Token;

			// Get the Tasks to wait for all of the external diff tool processes to be closed.
			var waitForAllExternalDiffToolProcessFromThisSetToCloseTasks = ExternalDiffToolProcessIdsRunningInThisSet.Select(processId => Task.Run(() =>
			{
				try
				{
					// Loop until the diff tool window is closed.
					System.Diagnostics.Process process = null;
					do
					{
						System.Threading.Thread.Sleep(DiffAllFilesHelper.DEFAULT_THREAD_SLEEP_TIME);
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
					System.Threading.Thread.Sleep(DiffAllFilesHelper.DEFAULT_THREAD_SLEEP_TIME);

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
				while (!cancellationToken.IsCancellationRequested && !_compareNextSetOfFiles && !IsCompareOperationsCancelled)
					System.Threading.Thread.Sleep(DiffAllFilesHelper.DEFAULT_THREAD_SLEEP_TIME);
			});

			// Merge the list of tasks waiting for the external diff tool windows and the VS diff tool tabs to be closed.
			var waitForAllDiffToolWindowsFromThisSetToCloseTasks = new List<Task>(waitForAllExternalDiffToolProcessFromThisSetToCloseTasks);
			waitForAllDiffToolWindowsFromThisSetToCloseTasks.Add(waitForAllVsDiffToolTabsFromThisSetToCloseTask);

			// Wait for all of the diff windows from this set to be closed, or the "Next Set Of Files" or Cancel button to be pressed.
			this.IsBusy = false;
			await Task.WhenAny(Task.WhenAll(waitForAllDiffToolWindowsFromThisSetToCloseTasks), waitForNextSetOfFilesOrCancelCommandsToBeExecutedTask);
			this.IsBusy = true;

			// Now that we are done waiting for this set, cancel any Tasks that are still running and refresh the Cancellation Token Source to use for the next set of Tasks we create.
			_waitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelledCancellationTokenSource.Cancel();
			_waitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelledCancellationTokenSource = new CancellationTokenSource();

			// We are done comparing this set of files, so reset the flag to start comparing the next set of files.
			_compareNextSetOfFiles = false;

			// If the user wants to cancel comparing the rest of the files in this set, break out of the loop so we stop comparing more files.
			if (IsCompareOperationsCancelled)
				return;

			// Now that we are done with this set of compares, clear the list of diff tool windows still open from this set.
			lock (ExternalDiffToolProcessIdsRunningLock)
			{
				ExternalDiffToolProcessIdsRunningInThisSet.Clear();
			}
			lock (VsDiffToolTabCaptionsStillOpenLock)
			{
				VsDiffToolTabCaptionsStillOpenInThisSet.Clear();
			}
		}
		private CancellationTokenSource _waitForAllDiffToolWindowsInThisSetOfFilesToBeClosedOrCancelledCancellationTokenSource = new CancellationTokenSource();

		/// <summary>
		/// Opens the file difference in built-in Visual Studio difference tool.
		/// </summary>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory holding the temp files.</param>
		/// <param name="fileName">Name of the file being compared. Visual Studio typically uses this for diff tool Tab's caption.</param>
		/// <param name="dte2">The dte2 object we can use to hook into Visual Studio with.</param>
		private void OpenFileDiffInVsDiffTool(SourceAndTargetFilePathsAndLabels filePathsAndLabels, string tempDiffFilesDirectory, string fileName, DTE2 dte2)
		{
			// Get if the user should be able to edit the files to save changes back to them; only allow it if they are not temp files.
			bool sourceIsTemp = filePathsAndLabels.SourceFilePathAndLabel.FilePath.StartsWith(tempDiffFilesDirectory);
			bool targetIsTemp = filePathsAndLabels.TargetFilePathAndLabel.FilePath.StartsWith(tempDiffFilesDirectory);

			// Launch the VS diff tool to diff this file.
			// The VisualDiffFiles() function will still display a colon even if a "Tag" is not provided, so use the label's Prefix for the Tag.
			Difference.VisualDiffFiles(filePathsAndLabels.SourceFilePathAndLabel.FilePath, filePathsAndLabels.TargetFilePathAndLabel.FilePath,
				filePathsAndLabels.SourceFilePathAndLabel.FileLabel.Prefix, filePathsAndLabels.TargetFilePathAndLabel.FileLabel.Prefix,
				filePathsAndLabels.SourceFilePathAndLabel.FileLabel.ToStringWithoutPrefix(), filePathsAndLabels.TargetFilePathAndLabel.FileLabel.ToStringWithoutPrefix(),
				sourceIsTemp, targetIsTemp);

			// We are likely opening several diff windows, so make sure they don't just replace one another in the Preview Tab.
			if (PackageHelper.IsCommandAvailable("Window.KeepTabOpen"))
				dte2.ExecuteCommand("Window.KeepTabOpen");

			// If the diff tool was not opened successfully, just exit.
			// The active window's caption will typically contain the filename, except for "Combined" files it will contain the File Labels.
			string diffToolWindowCaption = dte2.ActiveWindow.Caption;
			if (!diffToolWindowCaption.Contains(fileName) &&
				(!diffToolWindowCaption.Contains(filePathsAndLabels.SourceFilePathAndLabel.FileLabel.ToStringWithoutPrefix()) &&
				!diffToolWindowCaption.Contains(filePathsAndLabels.TargetFilePathAndLabel.FileLabel.ToStringWithoutPrefix())))
				return;

			// Add this VS diff tool's window name to our list of open VS diff tool tabs.
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
					System.Threading.Thread.Sleep(DiffAllFilesHelper.DEFAULT_THREAD_SLEEP_TIME);

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
					ForceDeleteDirectory(tempDirectory);
			});
		}

		/// <summary>
		/// Opens the file difference in external difference tool.
		/// </summary>
		/// <param name="diffToolConfiguration">The difference tool configuration to use to diff the files.</param>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory holding the temp files.</param>
		private void OpenFileDiffInExternalDiffTool(DiffToolConfiguration diffToolConfiguration, SourceAndTargetFilePathsAndLabels filePathsAndLabels, string tempDiffFilesDirectory)
		{
			// Get if the user should be able to edit the files to save changes back to them; only allow it if they are not temp files.
			bool sourceIsTemp = filePathsAndLabels.SourceFilePathAndLabel.FilePath.StartsWith(tempDiffFilesDirectory);
			bool targetIsTemp = filePathsAndLabels.TargetFilePathAndLabel.FilePath.StartsWith(tempDiffFilesDirectory);
			if (sourceIsTemp)
				MakeFileReadOnly(filePathsAndLabels.SourceFilePathAndLabel.FilePath);
			if (targetIsTemp)
				MakeFileReadOnly(filePathsAndLabels.TargetFilePathAndLabel.FilePath);

			// Build the arguments to pass to the diff tool's executable.
			string diffToolArguments = diffToolConfiguration.ExecutableArgumentFormat;
			diffToolArguments = diffToolArguments.Replace("%1", string.Format("\"{0}\"", filePathsAndLabels.SourceFilePathAndLabel.FilePath));
			diffToolArguments = diffToolArguments.Replace("%2", string.Format("\"{0}\"", filePathsAndLabels.TargetFilePathAndLabel.FilePath));
			diffToolArguments = diffToolArguments.Replace("%6", string.Format("\"{0}\"", filePathsAndLabels.SourceFilePathAndLabel.FileLabel));
			diffToolArguments = diffToolArguments.Replace("%7", string.Format("\"{0}\"", filePathsAndLabels.TargetFilePathAndLabel.FileLabel));

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
			//// Debugging When Diff Tool Isn't Launched Code Start
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
			//// Debugging When Diff Tool Isn't Launched Code End
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
						System.Threading.Thread.Sleep(DiffAllFilesHelper.DEFAULT_THREAD_SLEEP_TIME);
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
					ForceDeleteDirectory(tempDiffFilesDirectory);
			});
		}

		/// <summary>
		/// Sets the file's read-only attribute to true.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		private void MakeFileReadOnly(string filePath)
		{
			if (File.Exists(filePath))
				File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
		}

		/// <summary>
		/// Deletes the given Directory recursively, even if it contains read-only files.
		/// </summary>
		/// <param name="directoryPath">The path.</param>
		private static void ForceDeleteDirectory(string directoryPath)
		{
			// If the directory doesn't exist, then we can't delete it, so just exit.
			if (!Directory.Exists(directoryPath))
				return;

			// Remove the read-only flag from the directory and any of its files before attempting to delete it so that we avoid Unauthorized Access Exceptions.
			var directoryInfo = new DirectoryInfo(directoryPath) { Attributes = FileAttributes.Normal };
			foreach (var info in directoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
			{
				info.Attributes = FileAttributes.Normal;
			}

			// Delete the directory recursively.
			directoryInfo.Delete(true);
		}

		/// <summary>
		/// Downloads the files to diff and the labels that the diff tool should show.
		/// </summary>
		/// <param name="fileChange">The pending change.</param>
		/// <param name="compareVersion">The compare version.</param>
		/// <param name="filePathsAndLabels">The file paths and labels.</param>
		/// <param name="tempDiffFilesDirectory">The temporary difference files directory.</param>
		private void GetFilesToDiffAndTheirLabels(IFileChange fileChange, CompareVersion compareVersion, out SourceAndTargetFilePathsAndLabels filePathsAndLabels, out string tempDiffFilesDirectory)
		{
			// Get the file paths to save the files to.
			GetTemporarySourceAndTargetFilePaths(Path.GetFileName(fileChange.LocalOrServerFilePath), out filePathsAndLabels, out tempDiffFilesDirectory);

			// Create the files as blank files to make sure they exist.
			File.WriteAllText(filePathsAndLabels.SourceFilePathAndLabel.FilePath, string.Empty);
			File.WriteAllText(filePathsAndLabels.TargetFilePathAndLabel.FilePath, string.Empty);

			FileLabel sourceFileLabel = FileLabel.Empty;
			FileLabel targetFileLabel = FileLabel.Empty;

			// Get the source and target files to use in the compare.
			VersionSpec versionSpec;
			switch (this.SectionType)
			{
				case SectionTypes.PendingChanges:
					// We are going to need to perform TFS specific functionality here, so get the file change as a TFS file change.
					var pendingChange = fileChange as ITfsFileChange;
					if (pendingChange == null)
					{
						ShowNotification("Could not convert the File Change to a TFS Pending Change. This should never happen!", NotificationType.Error);
						return;
					}

					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (pendingChange.IsAdd)
					{
						sourceFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING);
					}
					else
					{
						// Get the source to compare against the file's local changes.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.WorkspaceVersion:
								pendingChange.DownloadBaseFile(filePathsAndLabels.SourceFilePathAndLabel.FilePath);
								sourceFileLabel = new FileLabel("Server", pendingChange.ServerFilePath, string.Format("C{0}", pendingChange.Version));
								break;

							case CompareVersion.Values.LatestVersion:
								versionSpec = VersionSpec.Latest;
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(pendingChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", pendingChange.ServerFilePath, "T") :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(pendingChange.ServerFilePath, versionSpec.DisplayString));
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (pendingChange.IsDelete)
					{
						targetFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING);
					}
					else
					{
						// Get the file's local changes, using the existing local file for the Target file.
						filePathsAndLabels = new SourceAndTargetFilePathsAndLabels(filePathsAndLabels.SourceFilePathAndLabel, new FilePathAndLabel(pendingChange.LocalFilePath));
						targetFileLabel = new FileLabel("Local", pendingChange.LocalFilePath, string.Empty);
					}
					break;

				case SectionTypes.ChangesetDetails:
					// We are going to need to perform TFS specific functionality here, so get the file change as a TFS file change.
					var changesetChange = fileChange as ITfsFileChange;
					if (changesetChange == null)
					{
						ShowNotification("Could not convert the File Change to a TFS Changeset Change. This should never happen!", NotificationType.Error);
						return;
					}

					// We are going to need to perform TFS specific functionality here, so get the file changes service as a TFS file changes service.
					var pendingChangesService = FileChangesService as ITfsPendingChangesService;
					if (pendingChangesService == null)
					{
						ShowNotification("Could not convert the File Changes Service to a TFS Pending Changes Service. This should never happen!", NotificationType.Error);
						return;
					}

					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (changesetChange.IsAdd)
					{
						sourceFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING);
					}
					else
					{
						// Get the source to compare against the Changeset's version.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.PreviousVersion:
								versionSpec = new ChangesetVersionSpec(changesetChange.Version - 1);
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(changesetChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", changesetChange.ServerFilePath, string.Format("C{0}", changesetChange.Version - 1)) :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(changesetChange.ServerFilePath, versionSpec.DisplayString));
								break;

							case CompareVersion.Values.WorkspaceVersion:
								versionSpec = new WorkspaceVersionSpec(pendingChangesService.Workspace);
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(changesetChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", changesetChange.ServerFilePath, string.Format("W{0}", pendingChangesService.Workspace.DisplayName)) :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(changesetChange.ServerFilePath, versionSpec.DisplayString));
								break;

							case CompareVersion.Values.LatestVersion:
								versionSpec = VersionSpec.Latest;
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(changesetChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", changesetChange.ServerFilePath, "T") :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(changesetChange.ServerFilePath, versionSpec.DisplayString));
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (changesetChange.IsDelete)
					{
						targetFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING);
					}
					else
					{
						// Get the Changeset's version of the file.
						versionSpec = new ChangesetVersionSpec(changesetChange.Version);
						targetFileLabel = DownloadFileVersionIfItExistsInVersionControl(changesetChange, versionSpec, filePathsAndLabels.TargetFilePathAndLabel.FilePath) ?
							new FileLabel("Server", changesetChange.ServerFilePath, string.Format("C{0}", changesetChange.Version)) :
							new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(changesetChange.ServerFilePath, versionSpec.DisplayString));
					}
					break;

				case SectionTypes.ShelvesetDetails:
					// We are going to need to perform TFS specific functionality here, so get the file change as a TFS file change.
					var shelvesetChange = fileChange as ITfsFileChange;
					if (shelvesetChange == null)
					{
						ShowNotification("Could not convert the File Change to a TFS Shelveset Change. This should never happen!", NotificationType.Error);
						return;
					}

					// We are going to need to perform TFS specific functionality here, so get the file changes service as a TFS file changes service.
					var pendingChangesServiceForShelveset = FileChangesService as ITfsPendingChangesService;
					if (pendingChangesServiceForShelveset == null)
					{
						ShowNotification("Could not convert the File Changes Service to a TFS Pending Changes Service For Shelveset. This should never happen!", NotificationType.Error);
						return;
					}

					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (shelvesetChange.IsAdd)
					{
						sourceFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING);
					}
					else
					{
						// Get the source to compare against the Shelveset's version.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.UnmodifiedVersion:
								shelvesetChange.DownloadBaseFile(filePathsAndLabels.SourceFilePathAndLabel.FilePath);
								sourceFileLabel = new FileLabel("Server", shelvesetChange.ServerFilePath, string.Format("C{0}", shelvesetChange.Version));
								break;

							case CompareVersion.Values.WorkspaceVersion:
								versionSpec = new WorkspaceVersionSpec(pendingChangesServiceForShelveset.Workspace);
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(shelvesetChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", shelvesetChange.ServerFilePath, string.Format("W{0}", pendingChangesServiceForShelveset.Workspace.DisplayName)) :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(shelvesetChange.ServerFilePath, versionSpec.DisplayString));
								break;

							case CompareVersion.Values.LatestVersion:
								versionSpec = VersionSpec.Latest;
								sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(shelvesetChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", shelvesetChange.ServerFilePath, "T") :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(shelvesetChange.ServerFilePath, versionSpec.DisplayString));
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (shelvesetChange.IsDelete)
					{
						targetFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING);
					}
					else
					{
						// Get the Shelveset's version of the file.
						shelvesetChange.DownloadShelvedFile(filePathsAndLabels.TargetFilePathAndLabel.FilePath);
						targetFileLabel = new FileLabel("Shelved Change", shelvesetChange.ServerFilePath, shelvesetChange.ShelvesetName);
					}
					break;

				case SectionTypes.GitChanges:
					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (fileChange.IsAdd)
					{
						sourceFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING);
					}
					else
					{
						// Get the source to compare against the file's local changes.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.UnmodifiedVersion:
								sourceFileLabel = GitHelper.GetSpecificVersionOfFile(fileChange.ServerOrLocalFilePath, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
									new FileLabel("Server", fileChange.ServerOrLocalFilePath, "HEAD") :
									new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(fileChange.ServerOrLocalFilePath, "HEAD"));
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (fileChange.IsDelete)
					{
						targetFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING);
					}
					else
					{
						// Get the file's local changes, using the existing local file for the Target file.
						filePathsAndLabels = new SourceAndTargetFilePathsAndLabels(filePathsAndLabels.SourceFilePathAndLabel, new FilePathAndLabel(fileChange.LocalFilePath));
						targetFileLabel = new FileLabel("Local", fileChange.LocalFilePath, string.Empty);
					}
					break;

				case SectionTypes.GitCommitDetails:
					// If this file is being added to source control, there is no "source" file to retrieve, so set the label appropriately.
					if (fileChange.IsAdd)
					{
						sourceFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING);
					}
					else
					{
						// Get the source to compare against the Changeset's version.
						switch (compareVersion.Value)
						{
							case CompareVersion.Values.PreviousVersion:
								//versionSpec = new ChangesetVersionSpec(pendingChange.Version - 1);
								//sourceFileLabel = DownloadFileVersionIfItExistsInVersionControl(pendingChange, versionSpec, filePathsAndLabels.SourceFilePathAndLabel.FilePath) ?
								//	new FileLabel("Server", fileChange.ServerItem, string.Format("C{0}", fileChange.Version - 1)) :
								//	new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(fileChange.ServerItem, versionSpec.DisplayString));
								break;
						}
					}

					// If the file is being deleted, there is no "target" file to retrieve, so set the label appropriately.
					if (fileChange.IsDelete)
					{
						targetFileLabel = new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING);
					}
					else
					{
						// Get the Changeset's version of the file.
						//versionSpec = new ChangesetVersionSpec(pendingChange.Version);
						//targetFileLabel = DownloadFileVersionIfItExistsInVersionControl(pendingChange, versionSpec, filePathsAndLabels.TargetFilePathAndLabel.FilePath) ?
						//	new FileLabel("Server", pendingChange.ServerItem, string.Format("C{0}", pendingChange.Version)) :
						//	new FileLabel(DiffAllFilesHelper.NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(pendingChange.ServerItem, versionSpec.DisplayString));
					}
					break;
			}

			// Apply the Labels to use with the files.
			filePathsAndLabels = filePathsAndLabels.GetCopyWithNewFileLabels(sourceFileLabel, targetFileLabel);
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
		/// <para>Returns true if the file was downloaded successfully, false if it failed.</para>
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		/// <param name="versionSpec">The version spec.</param>
		/// <param name="filePathToDownloadTo">The file path to download to.</param>
		private bool DownloadFileVersionIfItExistsInVersionControl(ITfsFileChange pendingChange, VersionSpec versionSpec, string filePathToDownloadTo)
		{
			bool fileDownloaded = true;

			// If the file exists, download it.
			if (pendingChange.VersionControlServer.ServerItemExists(pendingChange.ServerFilePath, versionSpec, DeletedState.NonDeleted, ItemType.File))
			{
				try
				{
					pendingChange.VersionControlServer.DownloadFile(pendingChange.ServerFilePath, pendingChange.DeletionId, versionSpec, filePathToDownloadTo);
				}
				catch (Microsoft.TeamFoundation.VersionControl.Client.VersionControlException)
				{
					fileDownloaded = false;
				}
			}
			// Else the file doesn't exist so update the file's label to specify that.
			else
			{
				fileDownloaded = false;
			}

			// Return if the file was downloaded or not.
			return fileDownloaded;
		}

		#endregion

	}
}
