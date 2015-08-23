using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using VS_DiffAllFiles.Adapters;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Sections
{
	/// <summary>
	/// Diff All Files section in the Changeset Details window.
	/// </summary>
// VS 2012 doesn't know about anything Git related, as that was all added to be native in VS 2013, so don't try to register the control in VS 2012.
#if (!VS2012)
	[TeamExplorerSection(GitCommitDetailsSection.SectionId, TeamExplorerPageIds.GitCommitDetails, 25)]
#endif
	public class GitCommitDetailsSection : GitDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "027EE930-003B-127D-927E-1760BA9BC3B5";

		/// <summary>
		/// Initialize override.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void Initialize(object sender, SectionInitializeEventArgs e)
		{
			base.Initialize(sender, e);

			// Find the Pending Changes extensibility service and save a handle to it.
			FileChangesService = new GitCommitDetailsService(this.GetService<ICommitDetailsExt>());

			// Register for property change notifications on the Commit Details window.
			if (FileChangesService != null)
				FileChangesService.PropertyChanged += commitDetailsService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (FileChangesService != null)
				FileChangesService.PropertyChanged -= commitDetailsService_PropertyChanged;
			FileChangesService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
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
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.PreviousVersion, CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion };
	}
}
