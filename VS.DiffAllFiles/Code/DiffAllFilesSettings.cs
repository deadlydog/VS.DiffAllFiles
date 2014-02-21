using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[CLSCompliant(false), ComVisible(true)]
	public class DiffAllFilesSettings : DialogPage
	{
		/// <summary>
		/// Gets or sets the Settings of the package.
		/// </summary>
		public static DiffAllFilesSettings Settings { get; set; }

		[Category("Diff All Files Category")]
		[DisplayName("File Extensions To Ignore")]
		[Description("Files with these extensions will not be compared.")]
		public List<string> FileExtensionsToIgnore { get; set; }

		[Category("Diff All Files Category")]
		[DisplayName("Compare Files Not Changed")]
		[Description("If files that are checked out, but not actually changed should be compared.")]
		public bool CompareItemsNotChanged { get; set; }

		[Category("Diff All Files Category")]
		[DisplayName("Compare New Files")]
		[Description("If files being added to source control should be compared.")]
		public bool CompareNewItems { get; set; }

		[Category("Diff All Files Category")]
		[DisplayName("Compare Deleted Files")]
		[Description("If files being deleted from source control should be compared.")]
		public bool CompareDeletedItems { get; set; }

		[Category("Diff All Files Category")]
		[DisplayName("Try To Compare Files One At A Time")]
		[Description("If we should try and launch the diff application one file at a time, or all files at once. This may or may not work depending on your selected diff application.")]
		public bool CompareOneAtATime { get { return _compareOneAtATime; } set { _compareOneAtATime = value; } }
		private bool _compareOneAtATime = false;
	}
}
