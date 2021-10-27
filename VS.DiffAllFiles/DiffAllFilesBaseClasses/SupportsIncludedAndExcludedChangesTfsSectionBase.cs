using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
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
				case "FilteredIncludedChanges": // I believe the "Filtered" events and properties are deprecated, but we'll leave this case here for now; no harm.
				case "SelectedIncludedItems":	// New non-deprecated events to listen to as of VS 2019.
					NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges": // I believe the "Filtered" events and properties are deprecated, but we'll leave this case here for now; no harm.
				case "SelectedExcludedItems":   // New non-deprecated events to listen to as of VS 2019.
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
					((FileChangesService.SelectedIncludedChanges.Count + FileChangesService.SelectedExcludedChanges.Count) > 0);
			}
		}

		/// <summary>
		/// Gets if the Compare Included Files command should be enabled.
		/// </summary>
		public override bool IsCompareIncludedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && (FileChangesService.IncludedChanges.Count > 0);
			}
		}

		/// <summary>
		/// Gets if the Compare Excluded Files command should be enabled.
		/// </summary>
		public override bool IsCompareExcludedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && (FileChangesService.ExcludedChanges.Count > 0);
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
		public override void Refresh()
		{
			base.Refresh();
			NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
			NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
		}
	}
}
