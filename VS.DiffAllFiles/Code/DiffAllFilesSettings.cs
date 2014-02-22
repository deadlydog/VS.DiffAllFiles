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
	public class DiffAllFilesSettings : DialogPage
	{
		/// <summary>
		/// Gets or sets the Settings of the package.
		/// </summary>
		public static DiffAllFilesSettings Settings { get; set; }

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
		public bool CompareNewFiles { get; set; }

		/// <summary>
		/// Get / Set if files being deleted from source control should be compared.
		/// </summary>
		public bool CompareDeletedFiles { get; set; }

		/// <summary>
		/// Get / Set if files whose contents have not been changed should be compared.
		/// </summary>
		public bool CompareFilesNotChanged { get; set; }

		/// <summary>
		/// Get / Set if we should try and compare files one at a time, rather than launching the diff tool for all files at once.
		/// </summary>
		public bool CompareFilesOneAtATime { get; set; }

		/// <summary>
		/// Get / Set the list of file extensions to not compare.
		/// </summary>
		public List<string> FileExtensionsToIgnoreList { get; set; }

		/// <summary>
		/// Get / Set the list of file extensions to ignore as a comma separated string.
		/// </summary>
		public string FileExtensionsToIgnore
		{
			get { return string.Join(",", FileExtensionsToIgnoreList.ToArray()); }
			set
			{
				FileExtensionsToIgnoreList.Clear();
				if (value != null)
					FileExtensionsToIgnoreList = value.Split(',').ToList();

				// Remove any empty entries (i.e. the string ended with a comma).
				FileExtensionsToIgnoreList.RemoveAll(string.IsNullOrWhiteSpace);
			}
		}

		/// <summary>
		/// Gets the window that is used as the user interface of the dialog page.
		/// </summary>
		protected override System.Windows.Forms.IWin32Window Window
		{
			get { return new DiffAllFilesSettingsPageControl(); }
		}
	}
}
