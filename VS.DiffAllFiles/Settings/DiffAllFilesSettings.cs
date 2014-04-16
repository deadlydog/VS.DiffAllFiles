using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using Microsoft.VisualStudio.Shell;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Settings
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]	// Special guid to tell it that this is a custom Options dialog page, not the built-in grid dialog page.
	public class DiffAllFilesSettings : UIElementDialogPage, INotifyPropertyChanged
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
		/// Gets or sets the Current Settings of the package.
		/// </summary>
		public static DiffAllFilesSettings CurrentSettings { get { return _currentSettings; } set { _currentSettings = value; CurrentSettingsChanged(null, System.EventArgs.Empty); } }
		private static DiffAllFilesSettings _currentSettings = null;

		/// <summary>
		/// Fires when the Current Settings instance is changed.
		/// </summary>
		public static event System.EventHandler CurrentSettingsChanged = delegate { }; 

		/// <summary>
		/// Initializes a new instance of the <see cref="DiffAllFilesSettings"/> class.
		/// </summary>
		public DiffAllFilesSettings()
		{
			// Initialize Global Settings.
			ResetGlobalSettings();

			// Initialize Per-Window Settings.
			PendingChangesCompareVersionValue = CompareVersion.WorkspaceVersion.Value;
			ChangesetDetailsCompareVersionValue = CompareVersion.PreviousVersion.Value;
			ShelvesetDetailsCompareVersionValue = CompareVersion.UnmodifiedVersion.Value;
			GitChangesCompareVersionValue = CompareVersion.UnmodifiedVersion.Value;
			GitCommitDetailsCompareVersionValue = CompareVersion.UnmodifiedVersion.Value;
		}

		#region Global Extension Settings

		/// <summary>
		/// Resets all of the extensions Global settings to their default values.
		/// </summary>
		private void ResetGlobalSettings()
		{
			// Specify the default values for all of the properties.
			FileExtensionsToIgnoreList = new List<string>() { "dll", "exe", "bmp", "gif", "jpg", "jpeg", "png", "raw", "tif", "tiff", "nupkg" };
			CompareFilesNotChanged = true;
			CompareNewFiles = true;
			CompareDeletedFiles = true;
			NumberOfFilesToCompareAtATime = 1;
			CompareModesAvailable = CompareModes.AllowUserToChoose;
			CompareModeToUse = CompareModes.IndividualFiles;
			UseSameHeaderForBothFiles = false;
		}

		/// <summary>
		/// Get / Set if new files being added to source control should be compared.
		/// </summary>
		public bool CompareNewFiles { get { return _compareNewFiles; } set { _compareNewFiles = value; NotifyPropertyChanged("CompareNewFiles"); } }
		private bool _compareNewFiles = false;

		/// <summary>
		/// Get / Set if files being deleted from source control should be compared.
		/// </summary>
		public bool CompareDeletedFiles { get { return _compareDeletedFiles; } set { _compareDeletedFiles = value; NotifyPropertyChanged("CompareDeletedFiles"); } }
		private bool _compareDeletedFiles = false;

		/// <summary>
		/// Get / Set if files whose contents have not been changed should be compared.
		/// </summary>
		public bool CompareFilesNotChanged { get { return _compareFilesNotChanged; } set { _compareFilesNotChanged = value; NotifyPropertyChanged("CompareFilesNotChanged"); } }
		private bool _compareFilesNotChanged = false;

		/// <summary>
		/// Get / Set the number of files to compare at a time.
		/// </summary>
		public int NumberOfFilesToCompareAtATime { get { return _numberOfFilesToCompareAtATime; } set { _numberOfFilesToCompareAtATime = value; NotifyPropertyChanged("NumberOfFilesToCompareAtATime"); } }
		private int _numberOfFilesToCompareAtATime = 0;

		/// <summary>
		/// Get / Set the list of file extensions to not compare.
		/// </summary>
		public List<string> FileExtensionsToIgnoreList
		{
			get { return _fileExtensionsToIgnoreList; }
			set
			{
				_fileExtensionsToIgnoreList = value;
				NotifyPropertyChanged("FileExtensionsToIgnoreList");
				NotifyPropertyChanged("FileExtensionsToIgnore");
			}
		}
		private List<string> _fileExtensionsToIgnoreList = new List<string>();

		/// <summary>
		/// Get / Set the list of file extensions to ignore as a comma separated string.
		/// </summary>
		public string FileExtensionsToIgnore
		{
			get { return string.Join(",", FileExtensionsToIgnoreList.ToArray()); }
			set
			{
				// Set the values in the list to the given values, separating them by comma, trimming whitespace, and removing duplicates.
				FileExtensionsToIgnoreList.Clear();
				if (value != null)
					FileExtensionsToIgnoreList = value.Split(',').Select(s => s.Trim()).Distinct().ToList();

				// Remove any empty entries (i.e. the string ended with a comma).
				FileExtensionsToIgnoreList.RemoveAll(string.IsNullOrWhiteSpace);
			}
		}

		/// <summary>
		/// Get / Set the available Compare Modes that may be used compare files.
		/// <para>This should not be used to decide which compare mode to use; instead use the CompareModeToUse property.</para>
		/// </summary>
		public CompareModes CompareModesAvailable { get { return _compareModesAvailable; } set { _compareModesAvailable = value; NotifyPropertyChanged("CompareModesAvailable"); } }
		private CompareModes _compareModesAvailable = CompareModes.AllowUserToChoose;

		/// <summary>
		/// Get / Set the Compare Mode to use to compare files, based on which ones are Available and which one was used last time.
		/// </summary>
		public CompareModes CompareModeToUse
		{
			get
			{
				if (CompareModesAvailable == CompareModes.AllowUserToChoose) return _compareModeToUse;
				else return CompareModesAvailable;
			}
			set { _compareModeToUse = value; NotifyPropertyChanged("CompareModeToUse"); }
		}
		private CompareModes _compareModeToUse = CompareModes.IndividualFiles;

		/// <summary>
		/// If true, the file headers placed in the Combined files will be the same when in the CombinedIntoSingleFile Compare Mode.
		/// </summary>
		public bool UseSameHeaderForBothFiles { get { return _useSameHeaderForBothFiles; } set { _useSameHeaderForBothFiles = value; NotifyPropertyChanged("UseSameHeaderForBothFiles"); } }
		private bool _useSameHeaderForBothFiles = false;

		#endregion

		#region Per-Window Settings

		// Any time a change is made to a Per-Window setting we save the changes immediately, so that we can always restore these settings with the values they had last.
		// This should work properly since the Tools -> Options window is modal, so Global and Per-Window settings cannot be modified at the same time.

		// We have to use CompareVersion.Values because CompareVersion is a class that the DialogPage does not know how to save, so we save the int enum value.

		/// <summary>
		/// The version to compare against when in the Pending Changes window.
		/// </summary>
		public CompareVersion.Values PendingChangesCompareVersionValue
		{
			get { return _pendingChangesCompareVersionValue; }
			set { _pendingChangesCompareVersionValue = value; NotifyPropertyChanged("PendingChangesCompareVersionValue"); NotifyPropertyChanged("PendingChangesCompareVersion"); }
		}
		private CompareVersion.Values _pendingChangesCompareVersionValue = CompareVersion.Values.WorkspaceVersion;
		
		public CompareVersion PendingChangesCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(PendingChangesCompareVersionValue); }
			set { PendingChangesCompareVersionValue = value.Value; SaveSettingsToStorage(); }
		}

		/// <summary>
		/// The version to compare against when in the Changeset Details window.
		/// </summary>
		public CompareVersion.Values ChangesetDetailsCompareVersionValue
		{
			get { return _changesetDetailsCompareVersionValue; }
			set { _changesetDetailsCompareVersionValue = value; NotifyPropertyChanged("ChangesetDetailsCompareVersionValue"); NotifyPropertyChanged("ChangesetDetailsCompareVersion"); }
		}
		private CompareVersion.Values _changesetDetailsCompareVersionValue = CompareVersion.Values.PreviousVersion;

		public CompareVersion ChangesetDetailsCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(ChangesetDetailsCompareVersionValue); }
			set { ChangesetDetailsCompareVersionValue = value.Value; SaveSettingsToStorage(); }
		}

		/// <summary>
		/// The version to compare against when in the Shelveset Details window.
		/// </summary>
		public CompareVersion.Values ShelvesetDetailsCompareVersionValue
		{
			get { return _shelvesetDetailsCompareVersionValue; }
			set { _shelvesetDetailsCompareVersionValue = value; NotifyPropertyChanged("ShelvesetDetailsCompareVersionValue"); NotifyPropertyChanged("ShelvesetDetailsCompareVersion"); }
		}
		private CompareVersion.Values _shelvesetDetailsCompareVersionValue = CompareVersion.Values.UnmodifiedVersion;

		public CompareVersion ShelvesetDetailsCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(ShelvesetDetailsCompareVersionValue); }
			set { ShelvesetDetailsCompareVersionValue = value.Value; SaveSettingsToStorage(); }
		}

		/// <summary>
		/// The version to compare against when in the Git Changes window.
		/// </summary>
		public CompareVersion.Values GitChangesCompareVersionValue
		{
			get { return _gitChangesCompareVersionValue; }
			set { _gitChangesCompareVersionValue = value; NotifyPropertyChanged("GitChangesCompareVersionValue"); NotifyPropertyChanged("GitChangesCompareVersion"); }
		}
		private CompareVersion.Values _gitChangesCompareVersionValue = CompareVersion.Values.UnmodifiedVersion;

		public CompareVersion GitChangesCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(GitChangesCompareVersionValue); }
			set { GitChangesCompareVersionValue = value.Value; SaveSettingsToStorage(); }
		}

		/// <summary>
		/// The version to compare against when in the Git Commit Details window.
		/// </summary>
		public CompareVersion.Values GitCommitDetailsCompareVersionValue
		{
			get { return _gitCommitDetailsCompareVersionValue; }
			set { _gitCommitDetailsCompareVersionValue = value; NotifyPropertyChanged("GitCommitDetailsCompareVersionValue"); NotifyPropertyChanged("GitCommitDetailsCompareVersion"); }
		}
		private CompareVersion.Values _gitCommitDetailsCompareVersionValue = CompareVersion.Values.UnmodifiedVersion;

		public CompareVersion GitCommitDetailsCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(GitCommitDetailsCompareVersionValue); }
			set { GitCommitDetailsCompareVersionValue = value.Value; SaveSettingsToStorage(); }
		}

		#endregion

		#region Overridden Functions

		/// <summary>
		/// Gets the Windows Presentation Foundation (WPF) child element to be hosted inside the Options dialog page.
		/// </summary>
		/// <returns>The WPF child element.</returns>
		protected override System.Windows.UIElement Child
		{
			get { return new DiffAllFilesSettingsPageControl(this); }
		}

		/// <summary>
		/// Should be overridden to reset settings to their default values.
		/// </summary>
		public override void ResetSettings()
		{
			ResetGlobalSettings();
			base.ResetSettings();
		}

		#endregion
	}
}
