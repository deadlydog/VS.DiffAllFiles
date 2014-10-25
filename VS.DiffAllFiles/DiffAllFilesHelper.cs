using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles
{
	public static class DiffAllFilesHelper
	{
		public const string NO_FILE_TO_COMPARE_NEW_FILE_LABEL_STRING = "[No File To Compare: File is being added to source control]";
		public const string NO_FILE_TO_COMPARE_DELETED_FILE_LABEL_STRING = "[No File To Compare: File is being deleted from source control]";
		private const string _NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL_STRING = "[No File To Compare: File version does not exist in source control]";

		public static string NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL(string filePath, string version)
		{
			return String.Format("{0}: {1};{2}", _NO_FILE_TO_COMPARE_NO_FILE_VERSION_LABEL_STRING, filePath, version);
		}

		/// <summary>
		/// The default length of time to sleep a thread, in milliseconds.
		/// </summary>
		public const int DEFAULT_THREAD_SLEEP_TIME = 100;

		/// <summary>
		/// The default length of time to await the UI thread so that other UI operations can be processed.
		/// </summary>
		public const int DEFAULT_UI_THREAD_AWAIT_TIME_TO_ALLOW_OTHER_UI_OPERATIONS_TO_PROCESS = 25;

		/// <summary>
		/// Gets the full path to TF.exe.
		/// </summary>
		public static string TfFilePath
		{
			get { return Path.Combine(Path.GetDirectoryName(PackageHelper.VisualStudioExecutablePath), "TF.exe"); }
		}

		/// <summary>
		/// Gets all of the diff tool configurations that are specified in the registry for Tfs.
		/// <para>If a file extension does not have a configured diff tool, it should be handled by the built-in Visual Studio diff tool.</para>
		/// </summary>
		public static List<FileExtensionDiffToolConfiguration> TfsDiffToolsConfigured
		{
			get
			{
				var diffToolsConfigured = new List<FileExtensionDiffToolConfiguration>();

				// Get the current Visual Studio version.
				var visualStudioRegistryRoot = PackageHelper.DTE2.RegistryRoot;

				// Check the registry to see which file types are configured to use an external diff tool.
				// File type and diff tool paths are stored in HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\[Version]\TeamFoundation\SourceControl\DiffTools.
				using (var diffToolsKey = Registry.CurrentUser.OpenSubKey(string.Format(@"{0}\TeamFoundation\SourceControl\DiffTools", visualStudioRegistryRoot)))
				{
					// If we couldn't find the configured diff tools, just return an empty array.
					if (diffToolsKey == null) return diffToolsConfigured;

					// Loop through each file extension that has an explicit diff tool configured.
					foreach (var fileExtension in diffToolsKey.GetSubKeyNames())
					{
						// If the extension subkey does not have a "Compare" subkey for us to grab values from, just move on to the next extension subkey.
						var extensionSubKey = diffToolsKey.OpenSubKey(fileExtension);
						if (!extensionSubKey.GetSubKeyNames().Contains("Compare")) continue;
						
						// If the Compare subkey does not have a "Command" value and "Arguments" value, just move on to the next extension subkey.
						var compareSubKey = extensionSubKey.OpenSubKey("Compare");
						if (!compareSubKey.GetValueNames().Contains("Command") || !compareSubKey.GetValueNames().Contains("Arguments")) continue;

						// Get the executable to use for this file extension and the format its arguments should be provided in.
						string executableFilePath = (string)compareSubKey.GetValue("Command");
						string executableArgumentsFormat = (string)compareSubKey.GetValue("Arguments");

						// Add this file extension's configuration to our list.
						diffToolsConfigured.Add(new FileExtensionDiffToolConfiguration(fileExtension, new DiffToolConfiguration(executableFilePath, executableArgumentsFormat)));
					}
				}

				// If the list contains a catch-all file extension, make sure it is at the end of the list.
				var catchAllDiffToolConfiguration = diffToolsConfigured.FirstOrDefault(d => d.FileExtension.Equals(".*"));
				if (catchAllDiffToolConfiguration != null)
				{
					diffToolsConfigured.Remove(catchAllDiffToolConfiguration);
					diffToolsConfigured.Add(catchAllDiffToolConfiguration);
				}

				// Return the configured diff tools.
				return diffToolsConfigured;
			}
		}

		/// <summary>
		/// Gets all of the diff tool configurations that are specified for Git.
		/// <para>If a file extension does not have a configured diff tool, it should be handled by the built-in Visual Studio diff tool.</para>
		/// </summary>
		public static List<FileExtensionDiffToolConfiguration> GitDiffToolsConfigured
		{
			get
			{
				var diffToolsConfigured = new List<FileExtensionDiffToolConfiguration>();
				//throw new NotImplementedException();
				return diffToolsConfigured;
			}
		}

		/// <summary>
		/// Recursively finds the visual children of the given control.
		/// </summary>
		/// <typeparam name="T">The type of control to look for.</typeparam>
		/// <param name="dependencyObject">The dependency object.</param>
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
		{
			if (dependencyObject != null)
			{
				for (int index = 0; index < VisualTreeHelper.GetChildrenCount(dependencyObject); index++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, index);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
	}
}