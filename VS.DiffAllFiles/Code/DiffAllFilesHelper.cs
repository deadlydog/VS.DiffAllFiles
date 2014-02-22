using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	public static class DiffAllFilesHelper
	{
		/// <summary>
		/// Gets the full path to TF.exe.
		/// </summary>
		public static string TfFilePath
		{
			get { return Path.Combine(Path.GetDirectoryName(PackageHelper.VisualStudioExecutablePath), "TF.exe"); }
		}

		/// <summary>
		/// Gets all of the file types that are explicitly configured to use a diff tool (i.e. to not use the built-in Visual Studio diff tool).
		/// </summary>
		public static List<string> FileTypesWithExplicitDiffToolConfigured
		{
			get
			{
				var fileTypes = new List<string>();

				// Get the current Visual Studio version.
				var visualStudioRegistryRoot = PackageHelper.DTE2.RegistryRoot;

				// Check the registry to see which file types are configured to use an external diff tool.
				// File type and diff tool paths are stored in HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\[Version]\TeamFoundation\SourceControl\DiffTools.
				using (var diffToolsKey = Registry.CurrentUser.OpenSubKey(string.Format(@"{0}\TeamFoundation\SourceControl\DiffTools", visualStudioRegistryRoot)))
				{
					// Get all of the file extensions that have an explicit diff tool configured.
					if (diffToolsKey == null) return fileTypes;
					fileTypes = diffToolsKey.GetSubKeyNames().ToList();
				}

				// Return the file types.
				return fileTypes;
			}
		}
	}
}