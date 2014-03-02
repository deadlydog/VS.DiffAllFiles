using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VS_DiffAllFiles.Settings;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public interface IDiffAllFilesSection : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		bool IsCompareAllFilesEnabled { get; }

		/// <summary>
		/// Gets if the Compare Selected Files command should be enabled.
		/// </summary>
		bool IsCompareSelectedFilesEnabled { get; }

		/// <summary>
		/// Gets if one of the commands to compare files is currently running.
		/// </summary>
		bool IsRunningCompareFilesCommand { get; }

		/// <summary>
		/// Gets the total number of files that will be compared.
		/// </summary>
		int NumberOfFilesToCompare { get; }

		/// <summary>
		/// Gets the number of files that have been compared already (that have been launched in the diff tool).
		/// </summary>
		int NumberOfFilesCompared { get; }

		/// <summary>
		/// Gets the number of file comparisons skipped due to the current settings configuration.
		/// </summary>
		int NumberOfFilesSkipped { get; }

		/// <summary>
		/// Gets a user-friendly message describing how much progress has been made on comparing all of the files.
		/// </summary>
		string FileComparisonProgressMessage { get; }

		/// <summary>
		/// Gets a user-friendly label to use for the command used to compare the next set of files.
		/// </summary>
		string NextSetOfFilesCommandLabel { get; }

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		IEnumerable<CompareVersion> CompareVersions { get; }

		/// <summary>
		/// Gets if the Version Control provider is available or not.
		/// <para>Commands should be disabled when the version control service is not available, as it is needed in order to compare files.</para>
		/// </summary>
		bool IsVersionControlServiceAvailable { get; }

		/// <summary>
		/// Gets the Settings to use.
		/// </summary>
		DiffAllFilesSettings Settings { get; }

		/// <summary>
		/// Stop launching any more compare operations.
		/// </summary>
		void Cancel();

		/// <summary>
		/// Launches the diff tool to compare the next set of files in the currently running compare files set.
		/// </summary>
		void CompareNextSetOfFiles();

		/// <summary>
		/// Closes any diff tool windows that are still open from the compare operations we launched.
		/// </summary>
		void CloseAllOpenCompareWindows();

		/// <summary>
		/// Gets the number of diff tool windows that we launched and are still open.
		/// </summary>
		int NumberOfCompareWindowsStillOpen { get; }
	}
}
