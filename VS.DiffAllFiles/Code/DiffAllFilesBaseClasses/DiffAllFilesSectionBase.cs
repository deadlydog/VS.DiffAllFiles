using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DansKingdom.VS_DiffAllFiles.Code.Settings;
using DansKingdom.VS_DiffAllFiles.Code.TeamExplorerBaseClasses;

namespace DansKingdom.VS_DiffAllFiles.Code.DiffAllFilesBaseClasses
{
	public abstract class DiffAllFilesSectionBase : TeamExplorerBaseSection, IDiffAllFilesSection
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
