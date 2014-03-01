using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using Microsoft.VisualStudio.Shell;

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
		}

		#region Global Extension Settings

		/// <summary>
		/// Resets all of the extensions Global settings to their default values.
		/// </summary>
		private void ResetGlobalSettings()
		{
			// Specify the default values for all of the properties.
			FileExtensionsToIgnoreList = new List<string>() { "exe", "bmp", "gif", "jpg", "jpeg", "png", "raw", "tif", "tiff" };
			CompareFilesNotChanged = true;
			CompareNewFiles = true;
			CompareDeletedFiles = true;
			CompareFilesOneAtATime = true;
			MultipleNumberOfFilesToCompareAtATime = 3;
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
		/// Get / Set if we should try and compare files one at a time, rather than launching the diff tool for all files at once.
		/// </summary>
		public bool CompareFilesOneAtATime { get { return _compareFilesOneAtATime; } set { _compareFilesOneAtATime = value; NotifyPropertyChanged("CompareFilesOneAtATime"); NotifyPropertyChanged("NumberOfFilesToCompareAtATime"); } }
		private bool _compareFilesOneAtATime = false;

		/// <summary>
		/// Get / Set the number of files to try and compare at a time, if not set to compare files one at a time.
		/// </summary>
		public int MultipleNumberOfFilesToCompareAtATime { get { return _multipleNumberOfFilesToCompareAtATime; } set { _multipleNumberOfFilesToCompareAtATime = value; NotifyPropertyChanged("MultipleNumberOfFilesToCompareAtATime"); NotifyPropertyChanged("NumberOfFilesToCompareAtATime"); } }
		private int _multipleNumberOfFilesToCompareAtATime = 0;

		/// <summary>
		/// Get the number of files to compare at a time.
		/// </summary>
		public int NumberOfFilesToCompareAtATime { get { return CompareFilesOneAtATime ? 1 : MultipleNumberOfFilesToCompareAtATime; } }

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
		private CompareVersion.Values _pendingChangesCompareVersionValue = CompareVersion.Values.PreviousVersion;
		
		public CompareVersion PendingChangesCompareVersion
		{
			get { return CompareVersion.GetCompareVersionFromValue(PendingChangesCompareVersionValue); }
			set { PendingChangesCompareVersionValue = value.Value; SaveSettingsToStorage(); }
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
