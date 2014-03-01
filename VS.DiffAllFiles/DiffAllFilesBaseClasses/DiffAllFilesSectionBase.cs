using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		protected DiffAllFilesSectionBase() : base()
		{
			// Listen for changes to the Settings instance, and start watching property changes on the current one.
			DiffAllFilesSettings.CurrentSettingsChanged += DiffAllFilesSettings_CurrentSettingsChanged;
			DiffAllFilesSettings_CurrentSettingsChanged(null, System.EventArgs.Empty);
		}

		void DiffAllFilesSettings_CurrentSettingsChanged(object sender, System.EventArgs e)
		{
			// Listen for changes to this Settings instance's properties.
			if (Settings != null)
			{
				Settings.PropertyChanged += Settings_PropertyChanged;
				NotifyPropertyChanged("NextSetOfFilesCommandLabel");
			}
		}

		void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("NumberOfFilesToCompareAtATime"))
				NotifyPropertyChanged("NextSetOfFilesCommandLabel");
		}

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
			set { _numberOfFilesToCompare = value; NotifyPropertyChanged("NumberOfFilesToCompare"); NotifyPropertyChanged("FileComparisonProgressMessage"); }
		}
		private int _numberOfFilesToCompare = 0;

		/// <summary>
		/// Gets the number of files that have been compared already (that have been launched in the diff tool).
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public int NumberOfFilesCompared
		{
			get { return _numberOfFilesCompared; }
			set { _numberOfFilesCompared = value; NotifyPropertyChanged("NumberOfFilesCompared"); NotifyPropertyChanged("FileComparisonProgressMessage"); }
		}
		private int _numberOfFilesCompared = 0;

		/// <summary>
		/// Gets a user-friendly message describing how much progress has been made on comparing all of the files.
		/// </summary>
		public string FileComparisonProgressMessage
		{
			get { return string.Format("Compared {0} of {1} files.", NumberOfFilesCompared, NumberOfFilesToCompare); }
		}

		/// <summary>
		/// Gets a user-friendly label to use for the command used to compare the next set of files.
		/// </summary>
		public string NextSetOfFilesCommandLabel
		{
			get { return (Settings != null && Settings.NumberOfFilesToCompareAtATime > 1) ? string.Format("Next {0} Files", Settings.NumberOfFilesToCompareAtATime) : "Next File"; }
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

		#endregion
	}
}
