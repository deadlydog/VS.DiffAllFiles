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
	[TeamExplorerSection(GitCommitDetailsSection.SectionId, TeamExplorerPageIds.GitCommitDetails, 25)]
	public class GitCommitDetailsSection : GitDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "027EE930-003B-127D-927E-1760BA9BC3B5";

		/// <summary>
		/// Handle to the Commit Details Extensibility service.
		/// </summary>
		private ICommitDetailsExt _commitDetailsService = null;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PendingChangesSection"/> class.
		/// </summary>
		public GitCommitDetailsSection() : base()
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
			_commitDetailsService = this.GetService<ICommitDetailsExt>();

			// Register for property change notifications on the Pending Changes window.
			if (_commitDetailsService != null)
				_commitDetailsService.PropertyChanged += commitDetailsService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (_commitDetailsService != null)
				_commitDetailsService.PropertyChanged -= commitDetailsService_PropertyChanged;
			_commitDetailsService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		private void commitDetailsService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Changes":
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "SelectedChanges":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		public override async Task PerformItemDiffs(ItemStatusTypesToCompare itemStatusTypesToCompare)
		{
			var a = _commitDetailsService;
			var b = a;
			var c = b;
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
					(_commitDetailsService.SelectedChanges.Count > 0);
			}
		}

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.PreviousVersion, CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion };

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
