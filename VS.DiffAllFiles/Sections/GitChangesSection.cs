using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.TeamFoundation.VersionControl.Client;
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
	[TeamExplorerSection(GitChangesSection.SectionId, TeamExplorerPageIds.GitChanges, 25)]
#endif
	public class GitChangesSection : GitDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "109D4D30-003C-127D-9D55-1B2330171392";

		/// <summary>
		/// Initialize override.
		/// </summary>
		public override void Initialize(object sender, SectionInitializeEventArgs e)
		{
			base.Initialize(sender, e);

			// Find the Pending Changes extensibility service and save a handle to it.
			FileChangesService = new GitChangesService(this.GetService<IChangesExt>());

			// Register for property change notifications on the Changes window.
			if (FileChangesService != null)
				FileChangesService.PropertyChanged += changesService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (FileChangesService != null)
				FileChangesService.PropertyChanged -= changesService_PropertyChanged;
			FileChangesService = null;

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

		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.UnmodifiedVersion };
	}
}
