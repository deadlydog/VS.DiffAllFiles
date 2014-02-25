using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
	{
		#region IDiffAllFilesSection Properties
		/// <summary>
		/// Gets if the View All Files command should be enabled.
		/// </summary>
		public abstract bool IsViewAllFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Selected Files command should be enabled.
		/// </summary>
		public abstract bool IsViewSelectedFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Included Files command should be enabled.
		/// </summary>
		public abstract bool IsViewIncludedFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Excluded Files command should be enabled.
		/// </summary>
		public abstract bool IsViewExcludedFilesEnabled { get; }

		/// <summary>
		/// Gets if the View Included Files command should be visible.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public abstract bool IsViewIncludedFilesVisible { get; }

		/// <summary>
		/// Gets if the View Excluded Files command should be visible.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public abstract bool IsViewExcludedFilesVisible { get; }

		/// <summary>
		/// The versions of files to compare against.
		/// </summary>
		public abstract IEnumerable<CompareVersion> CompareVersions { get; }

		/// <summary>
		/// Gets if the Version Control provider is available or not.
		/// <para>Commands should be disabled when version control is not available, as it is needed in order to compare files.</para>
		/// </summary>
		public abstract bool IsVersionControlServiceAvailable { get; set; }

		/// <summary>
		/// Gets the Settings to use.
		/// </summary>
		public DiffAllFilesSettings Settings
		{
			get { return DiffAllFilesSettings.CurrentSettings; }
		}
		#endregion
	}
}
