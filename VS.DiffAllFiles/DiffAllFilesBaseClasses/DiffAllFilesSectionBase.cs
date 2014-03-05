using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		/// <summary>
		/// List of process IDs hosting any external diff tools that we launched and are still open.
		/// </summary>
		protected readonly ObservableCollection<int> ExternalDiffToolProcessIdsRunning = new ObservableCollection<int>();

		/// <summary>
		/// List of Visual Studio window captions of windows hosting any VS diff tools that we launched and are still open.
		/// </summary>
		protected readonly ObservableCollection<string> VsDiffToolTabCaptionsStillOpen = new ObservableCollection<string>();

		/// <summary>
		/// List of process IDs hosting any external diff tools that we launched in the last set of compares and are still open.
		/// </summary>
		protected readonly List<int> ExternalDiffToolProcessIdsRunningInThisSet = new List<int>();
		
		/// <summary>
		/// List of Visual Studio window captions of windows hosting any VS diff tools that we launched in the last set of compares and are still open.
		/// </summary>
		protected readonly List<string> VsDiffToolTabCaptionsStillOpenInThisSet = new List<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DiffAllFilesSectionBase"/> class.
		/// </summary>
		protected DiffAllFilesSectionBase() : base()
		{
			// Listen for changes to the Settings instance, and start watching property changes on the current one.
			DiffAllFilesSettings.CurrentSettingsChanged += DiffAllFilesSettings_CurrentSettingsChanged;
			DiffAllFilesSettings_CurrentSettingsChanged(null, System.EventArgs.Empty);

			// Listen for when we launch new diff tool windows, and when the user closes them.
			ExternalDiffToolProcessIdsRunning.CollectionChanged += ExternalDiffToolProcessIdsRunning_CollectionChanged;
			VsDiffToolTabCaptionsStillOpen.CollectionChanged += VsDiffToolTabCaptionsStillOpen_CollectionChanged;
		}

		#region Event Handlers

		/// <summary>
		/// Handles the CollectionChanged event of the VsDiffToolTabCaptionsStillOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		void VsDiffToolTabCaptionsStillOpen_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("NumberOfCompareWindowsStillOpen");
		}

		/// <summary>
		/// Handles the CollectionChanged event of the ExternalDiffToolProcessesRunning control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		void ExternalDiffToolProcessIdsRunning_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("NumberOfCompareWindowsStillOpen");
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
				NotifyPropertyChanged("NextSetOfFilesCommandLabel");
			}
		}

		/// <summary>
		/// Handles the PropertyChanged event of the Settings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("NumberOfFilesToCompareAtATime"))
				NotifyPropertyChanged("NextSetOfFilesCommandLabel");
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
		}

		/// <summary>
		/// Gets a user-friendly message describing how much progress has been made on comparing all of the files.
		/// </summary>
		public string FileComparisonProgressMessage
		{
			get
			{
				string message = string.Format("Compared {0} of {1} files.", NumberOfFilesCompared, NumberOfFilesToCompare);
				if (NumberOfFilesSkipped > 0)
					message += string.Format(" Skipping {0}.", NumberOfFilesSkipped);
				return message;
			}
		}

		/// <summary>
		/// Gets a user-friendly label to use for the command used to compare the next set of files.
		/// </summary>
		public string NextSetOfFilesCommandLabel
		{
			get
			{
				int numberOfFilesToCompareNextSet = System.Math.Min(Settings.NumberOfFilesToCompareAtATime, (NumberOfFilesToCompare - NumberOfFilesCompared));
				return (Settings != null && numberOfFilesToCompareNextSet > 1) ? string.Format("Next {0} Files", numberOfFilesToCompareNextSet) : "Next File"; 
			}
		}

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public abstract IEnumerable<CompareVersion> CompareVersions { get; }

		/// <summary>
		/// Gets if the Version Control provider is available or not.
		/// <para>Commands should be disabled when version control is not available, as it is needed in order to compare files.</para>
		/// </summary>
		public abstract bool IsVersionControlServiceAvailable { get; set; }

		/// <summary>
		/// Gets the Settings to use.
		/// </summary>
		public DiffAllFilesSettings Settings
		{
			get { return DiffAllFilesSettings.CurrentSettings; }
		}

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

		private void CloseAllOpenWindows(IList<int> externalDiffToolProcessIdsRunning, IList<string> vsDiffToolTabCaptionsStillOpen)
		{
			// Close any open compare windows in the given lists. 
			// We don't need to remove the windows from the given lists here, as they will remove themselves from the list when they are closed.

			// Kill all processes hosting the external diff tools we launched.
			// Loop through the list backwards as it may be modified by another task while we loop through it.
			for (int processIndex = (externalDiffToolProcessIdsRunning.Count - 1); processIndex >= 0; processIndex--)
			{
				int processId = externalDiffToolProcessIdsRunning[processIndex];
				try
				{
					// Get a handle to the parent TF.exe process.
					var tfProcess = System.Diagnostics.Process.GetProcessById(processId);

					// Get a handle to the diff tool process created by the TF.exe process and close it.
					// Once the child diff tool process closes the parent TF.exe process will close as well.
					foreach (var diffToolProcess in DiffAllFilesHelper.GetChildProcesses(tfProcess))
						diffToolProcess.CloseMainWindow();	// Use CloseMainWindow() instead of Kill() to give user a chance to save any changes before it closes.
				}
				// Catch any exceptions thrown when the process does not exist.
				catch (ArgumentException)
				{
					// Since the process is already closed, remove it from our list.
					externalDiffToolProcessIdsRunning.Remove(processId);
				}
			}

			// Close all windows that we opened using the built-in VS diff tool and are still open.
			// Loop through the list backwards as it may be modified by another task while we loop through it.
			// The windows Item list index starts at 1, not 0.
			var windows = PackageHelper.DTE2.Windows;
			for (int windowIndex = windows.Count; windowIndex > 0; windowIndex--)
			{
				var window = windows.Item(windowIndex);
				if (vsDiffToolTabCaptionsStillOpen.Contains(window.Caption, new DansCSharpLibrary.Comparers.StringComparerIgnoreCase()))
					window.Close();
			}
		}

		/// <summary>
		/// Gets the number of diff tool windows that we launched and are still open.
		/// </summary>
		public int NumberOfCompareWindowsStillOpen
		{
			get { return ExternalDiffToolProcessIdsRunning.Count + VsDiffToolTabCaptionsStillOpen.Count; }
		}

		#endregion
	}
}
