using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		/// <summary>
		/// The default length of time to sleep a thread, in milliseconds.
		/// </summary>
		protected const int DEFAULT_THREAD_SLEEP_TIME = 100;

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
			set { _isRunningCompareFilesCommand = value; NotifyPropertyChanged("IsRunningCompareFilesCommand"); }
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
				string message = string.Format("Compared {0} of {1} files.", NumberOfFilesCompared, NumberOfFilesToCompare);
				if (NumberOfFilesSkipped > 0)
					message += string.Format(" Skipped {0}.", NumberOfFilesSkipped);

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
		/// Asynchronously launch the diff tools to compare the files.
		/// </summary>
		/// <param name="itemStatusTypesToCompare">The files that should be compared.</param>
		public abstract Task PerformItemDiffs(ItemStatusTypesToCompare itemStatusTypesToCompare);

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
		/// Launches the diff tool to compare the next set of files in the currently running compare files set.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public abstract void CompareNextSetOfFiles();

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
			set { Settings.CompareModeToUse = (value ? CompareModes.CombinedIntoSingleFile : CompareModes.IndividualFiles); }
		}

		#endregion
	}
}
