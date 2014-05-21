using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Sections
{
	/// <summary>
	/// Diff All Files section in the Changeset Details window.
	/// </summary>
	[TeamExplorerSection(GitChangesSection.SectionId, TeamExplorerPageIds.GitChanges, 25)]
	public class GitChangesSection : GitDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "109D4D30-003C-127D-9D55-1B2330171392";

		/// <summary>
		/// Handle to the Changes Extensibility service.
		/// </summary>
		private IChangesExt _changesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="PendingChangesSection"/> class.
		/// </summary>
		public GitChangesSection() : base()
		{
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new DiffAllFilesSectionControl(this);
		}

		/// <summary>
		/// Initialize override.
		/// </summary>
		public override void Initialize(object sender, SectionInitializeEventArgs e)
		{
			base.Initialize(sender, e);

			// Find the Pending Changes extensibility service and save a handle to it.
			_changesService = this.GetService<IChangesExt>();

			// Register for property change notifications on the Pending Changes window.
			if (_changesService != null)
				_changesService.PropertyChanged += changesService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (_changesService != null)
				_changesService.PropertyChanged -= changesService_PropertyChanged;
			_changesService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		private void changesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IncludedChanges":
					NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "ExcludedChanges":
					NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
	
				case "SelectedIncludedChanges":
				case "SelectedExcludedChanges":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		public override async Task PerformItemDiffs(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			var a = _changesService;
			var b = a;

			var k = _changesService.IncludedChanges[0].ChangeType == ChangesChangeType.Add;
			var i = _changesService.IncludedChanges[0].LocalItem;
			var j = _changesService.IncludedChanges[0].SourceLocalItem;
		}

		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		public override bool IsCompareAllFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable; }
		}

		/// <summary>
		/// Gets if the Compare Selected Files command should be enabled.
		/// </summary>
		public override bool IsCompareSelectedFilesEnabled
		{
			get
			{
				return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable &&
					((_changesService.SelectedIncludedChanges.Count + _changesService.SelectedExcludedChanges.Count) > 0);
			}
		}

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.UnmodifiedVersion };

		public override void Cancel()
		{
			base.Cancel();
		}

		public override async void Refresh()
		{
			base.Refresh();
		}

		/// <summary>
		/// Launches the diff tool to compare the next set of files in the currently running compare files set.
		/// </summary>
		public override void CompareNextSetOfFiles()
		{
			throw new NotImplementedException();
		}
	}
}
