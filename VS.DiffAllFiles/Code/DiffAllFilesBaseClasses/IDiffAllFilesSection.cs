using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DansKingdom.VS_DiffAllFiles.Code.Settings;

namespace DansKingdom.VS_DiffAllFiles.Code.DiffAllFilesBaseClasses
{
	public interface IDiffAllFilesSection : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets if the View All Files command should be enabled.
		/// </summary>
		bool IsViewAllFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Selected Files command should be enabled.
		/// </summary>
		bool IsViewSelectedFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Included Files command should be enabled.
		/// </summary>
		bool IsViewIncludedFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Excluded Files command should be enabled.
		/// </summary>
		bool IsViewExcludedFilesEnabled { get; }

		/// <summary>
		/// The versions of files to compare against.
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
	}
}
