using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;

namespace VS_DiffAllFiles
{
	public abstract class SupportsIncludedAndExcludedChangesTfsSectionBase : TfsDiffAllFilesSectionBase
	{
		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		protected override void _pendingChangesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base._pendingChangesService_PropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "IncludedChanges":
				case "FilteredIncludedChanges":
					NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges":
					NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
					break;
			}
		}

		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		public override bool IsCompareAllFilesEnabled
		{
			get { return IsCompareIncludedFilesEnabled || IsCompareExcludedFilesEnabled; }
		}

		/// <summary>
		/// Gets if the Compare Selected Files command should be enabled.
		/// </summary>
		public override bool IsCompareSelectedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable &&
					((FileChangesService.SelectedIncludedItems.Length + FileChangesService.SelectedExcludedItems.Length) > 0);
			}
		}

		/// <summary>
		/// Gets if the Compare Included Files command should be enabled.
		/// </summary>
		public override bool IsCompareIncludedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable &&
					((FileChangesService.IncludedChanges.Length + FileChangesService.FilteredIncludedChanges.Length) > 0);
			}
		}

		/// <summary>
		/// Gets if the Compare Excluded Files command should be enabled.
		/// </summary>
		public override bool IsCompareExcludedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable &&
					((FileChangesService.ExcludedChanges.Length + FileChangesService.FilteredExcludedChanges.Length) > 0);
			}
		}

		/// <summary>
		/// Gets if the Compare Included Files command should be an option for the user to use.
		/// </summary>
		public override bool IsCompareIncludedFilesAvailable { get { return true; } }

		/// <summary>
		/// Gets if the Compare Excluded Files command should be an option for the user to use.
		/// </summary>
		public override bool IsCompareExcludedFilesAvailable { get { return true; } }

		/// <summary>
		/// Refresh the section contents.
		/// </summary>
		public override async void Refresh()
		{
			base.Refresh();
			NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
			NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
		}
	}
}
