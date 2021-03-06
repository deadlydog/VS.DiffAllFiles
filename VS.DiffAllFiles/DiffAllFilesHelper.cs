﻿using System;
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
			get
			{
				string vsIdeDirectoryPath = Path.GetDirectoryName(PackageHelper.VisualStudioExecutablePath);

				string potentialPathToTfExe = Path.Combine(vsIdeDirectoryPath, @"CommonExtensions\Microsoft\TeamFoundation\Team Explorer\TF.exe");
				if (File.Exists(potentialPathToTfExe))
				{
					return potentialPathToTfExe;
				}

				potentialPathToTfExe = Path.Combine(vsIdeDirectoryPath, "TF.exe");
				if (File.Exists(potentialPathToTfExe))
				{
					return potentialPathToTfExe;
				}

				return null;
			}
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

// VS 2012 doesn't know about anything Git related, as that was all added to be native in VS 2013, so don't try to register the control in VS 2012.
#if (!VS2012)
		private const string _defaultExeArgumentFormat = "\"%1\" \"%2\"";

		/// <summary>
		/// Gets all of the diff tool configurations that are specified for Git.
		/// <para>If a file extension does not have a configured diff tool, it should be handled by the built-in Visual Studio diff tool.</para>
		/// </summary>
		/// <param name="gitRepositoryPath">The path to the Git repository, or a file within it. This is needed to get a handle to the Git repo.</param>
		/// <returns></returns>
		public static List<FileExtensionDiffToolConfiguration> GetGitDiffToolsConfigured(string gitRepositoryPath)
		{
			var diffToolsConfigured = new List<FileExtensionDiffToolConfiguration>();

			// Get the name of the diff tool entry to use.
			var configurationEntries = GitHelper.GetGitConfigurationEntries(gitRepositoryPath);
			var diffToolEntry = GetGitDiffToolEntry(configurationEntries);
			if (diffToolEntry == null) return diffToolsConfigured;

			// If the diff tool has a specific command to use to launch it, get the executable file path and arguments from it.
			var diffToolCmdEntry = GetGitDiffToolCmdEntry(configurationEntries, diffToolEntry);
			if (diffToolCmdEntry != null)
			{
				var diffToolExePath = string.Empty;
				var diffToolExeArgumentFormat = string.Empty;
				GetGitExePathAndArgumentsFormatFromCmdValue(diffToolCmdEntry.Value, out diffToolExePath, out diffToolExeArgumentFormat);
				diffToolExePath = GetSanitizedExecutableFilePath(diffToolExePath);

				if (!string.IsNullOrWhiteSpace(diffToolExePath))
					diffToolsConfigured.Add(new FileExtensionDiffToolConfiguration(".*", new DiffToolConfiguration(diffToolExePath, diffToolExeArgumentFormat)));
			}
			// Else a specific command to use to launch the diff tool does not exist, so let's assume the exe takes the default arguments.
			else
			{
				var diffToolPathEntry = GetGitDiffToolPathEntry(configurationEntries, diffToolEntry);
				if (diffToolPathEntry == null) return diffToolsConfigured;

				// Make sure the executable path is using the correct slashes for a path, and that it is wrapped in double quotes in case it contains spaces.
				var diffToolExePath = GetSanitizedExecutableFilePath(diffToolPathEntry.Value);
				diffToolsConfigured.Add(new FileExtensionDiffToolConfiguration(".*", new DiffToolConfiguration(diffToolExePath, _defaultExeArgumentFormat)));
			}

			return diffToolsConfigured;
		}

		/// <summary>
		/// Makes sure the file path is using the correct slashes for a path, and that it is wrapped in double quotes in case it contains spaces.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		private static string GetSanitizedExecutableFilePath(string filePath)
		{
			// Make sure the file path is using the correct slashes for a path.
			filePath = filePath.Replace("/", @"\");

			// No leading or trailing whitespace.
			filePath = filePath.Trim();

			// Remove any single quotes around the file path. It is valid syntax in the .gitconfig file for Git Bash, but we don't want it here.
			filePath = filePath.Trim("'".ToCharArray());
			
			// Make sure the path is wrapped in double quotes in case it contains spaces.
			filePath = string.Format("\"{0}\"", filePath.Trim("\"".ToCharArray()));

			return filePath;
		}

		/// <summary>
		/// Gets the executable path and arguments format from command value.
		/// </summary>
		/// <param name="diffToolCmdValue">The difference tool command value.</param>
		/// <param name="diffToolExePath">The difference tool executable path.</param>
		/// <param name="diffToolExeArgumentFormat">The difference tool executable argument format.</param>
		private static void GetGitExePathAndArgumentsFormatFromCmdValue(string diffToolCmdValue, out string diffToolExePath, out string diffToolExeArgumentFormat)
		{
			diffToolExePath = string.Empty;
			diffToolExeArgumentFormat = string.Empty;

			// If the cmd references an executable, we want to get the executable's path and the format it expects the arguments to the exe in, otherwise just exit.
			int exeStartIndex = diffToolCmdValue.IndexOf(".exe");
			if (exeStartIndex < 0)
				return;

			// Find where the executable path ends and the arguments begin.
			int indexOfDoubleQuoteAfterExePath = diffToolCmdValue.IndexOf('"', exeStartIndex);
			int indexOfSpaceAfterExePath = diffToolCmdValue.IndexOf(' ', exeStartIndex);
			int indexAfterExePath = exeStartIndex + 4;
			if (indexOfSpaceAfterExePath > 0)
				indexAfterExePath = indexOfSpaceAfterExePath;
			else if (indexOfDoubleQuoteAfterExePath > 0)
				indexAfterExePath = indexOfDoubleQuoteAfterExePath;

			// Get the executable path.
			diffToolExePath = diffToolCmdValue.Substring(0, indexAfterExePath);
			diffToolExePath = diffToolExePath.Replace("\"", string.Empty).Replace("/", @"\").Trim();

			// Get the arguments format.
			int argumentStartIndex = Math.Min(indexAfterExePath + 1, diffToolCmdValue.Length);
			diffToolExeArgumentFormat = diffToolCmdValue.Substring(argumentStartIndex).Trim();

			// Remove any double quotes from around the keywords, since we add them back in later when doing the TFS-keyword replacement.
			diffToolExeArgumentFormat = diffToolExeArgumentFormat.Replace("\"$LOCAL\"", "$LOCAL");
			diffToolExeArgumentFormat = diffToolExeArgumentFormat.Replace("\"$REMOTE\"", "$REMOTE");

			// Change the git-specific keywords to the TFS-specific keywords so that the same keyword-replacement code can be used for both TFS and Git.
			diffToolExeArgumentFormat = diffToolExeArgumentFormat.Replace("$LOCAL", "%1");
			diffToolExeArgumentFormat = diffToolExeArgumentFormat.Replace("$REMOTE", "%2");

			// If the argument format was not specified in the cmd value, use the default.
			if (string.IsNullOrWhiteSpace(diffToolExeArgumentFormat))
				diffToolExeArgumentFormat = _defaultExeArgumentFormat;
		}

		private static LibGit2Sharp.ConfigurationEntry<string> GetGitDiffToolEntry(List<LibGit2Sharp.ConfigurationEntry<string>> configurationEntries)
		{
			return GetMostLocalGitConfigurationEntryByKey(configurationEntries, "diff.tool");
		}

		private static LibGit2Sharp.ConfigurationEntry<string> GetGitDiffToolCmdEntry(List<LibGit2Sharp.ConfigurationEntry<string>> configurationEntries, LibGit2Sharp.ConfigurationEntry<string> diffToolEntry)
		{
			var diffToolCmdKey = string.Format("difftool.{0}.cmd", diffToolEntry.Value);
			return GetMostLocalGitConfigurationEntryByKey(configurationEntries, diffToolCmdKey);

		}

		private static LibGit2Sharp.ConfigurationEntry<string> GetGitDiffToolPathEntry(List<LibGit2Sharp.ConfigurationEntry<string>> configurationEntries, LibGit2Sharp.ConfigurationEntry<string> diffToolEntry)
		{
			var diffToolCmdKey = string.Format("difftool.{0}.path", diffToolEntry.Value);
			return GetMostLocalGitConfigurationEntryByKey(configurationEntries, diffToolCmdKey);

		}

		private static LibGit2Sharp.ConfigurationEntry<string> GetMostLocalGitConfigurationEntryByKey(List<LibGit2Sharp.ConfigurationEntry<string>> configurationEntries, string key)
		{
			// Local settings take precendence over global ones.
			return configurationEntries.FirstOrDefault(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && e.Level == LibGit2Sharp.ConfigurationLevel.Local)
				?? configurationEntries.FirstOrDefault(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && e.Level == LibGit2Sharp.ConfigurationLevel.Global)
				?? configurationEntries.FirstOrDefault(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && e.Level == LibGit2Sharp.ConfigurationLevel.System);
		}
#endif

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