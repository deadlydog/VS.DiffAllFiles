using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace DansKingdom.VS_DiffAllFiles.Code
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
		public static DiffAllFilesSettings CurrentSettings { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiffAllFilesSettings"/> class.
		/// </summary>
		public DiffAllFilesSettings()
		{
			ResetSettings();
		}

		/// <summary>
		/// Should be overridden to reset settings to their default values.
		/// </summary>
		public override void ResetSettings()
		{
			base.ResetSettings();

			// Specify the default values for all of the properties.
			FileExtensionsToIgnoreList = new List<string>() { "exe", "bmp", "gif", "jpg", "jpeg", "png", "raw", "tif", "tiff" };
			CompareFilesNotChanged = true;
			CompareNewFiles = true;
			CompareDeletedFiles = true;
			CompareFilesOneAtATime = true;
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
		public bool CompareFilesOneAtATime { get { return _compareFilesOneAtATime; } set { _compareFilesOneAtATime = value; NotifyPropertyChanged("CompareFilesOneAtATime"); } }
		private bool _compareFilesOneAtATime = false;

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
		/// Gets the Windows Presentation Foundation (WPF) child element to be hosted inside the Options dialog page.
		/// </summary>
		/// <returns>The WPF child element.</returns>
		protected override System.Windows.UIElement Child
		{
			get { return new DiffAllFilesSettingsPageControl(this); }
		}
	}
}
