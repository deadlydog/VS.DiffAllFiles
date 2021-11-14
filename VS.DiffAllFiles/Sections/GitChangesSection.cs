#if SUPPORTS_GIT_CONTROLS_EXTENSIBILITY
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
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
		private void changesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "StagedChanges":
				case "IncludedChanges":
					NotifyPropertyChanged("IsCompareIncludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "UnstagedChanges":
				case "ExcludedChanges":
					NotifyPropertyChanged("IsCompareExcludedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "SelectedStagedChanges":
				case "SelectedUnstagedChanges":
				case "SelectedIncludedChanges":
				case "SelectedExcludedChanges":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		/// <summary>
		/// Gets the text to use on the Included Files button.
		/// </summary>
		public override string IncludedFilesButtonLabel { get { return "Staged"; } }

		/// <summary>
		/// Gets the text to use on the Excluded Files button.
		/// </summary>
		public override string ExcludedFilesButtonLabel { get { return "Unstaged"; } }

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
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && (FileChangesService.IncludedChanges.Count > 0); }
		}

		/// <summary>
		/// Gets if the Compare Included Files command should be an option for the user to use.
		/// </summary>
		public override bool IsCompareIncludedFilesAvailable { get { return true; } }

		/// <summary>
		/// Gets if the Compare Excluded Files command should be enabled.
		/// </summary>
		public override bool IsCompareExcludedFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable && (FileChangesService.ExcludedChanges.Count > 0); }
		}

		/// <summary>
		/// Gets if the Compare Excluded Files command should be an option for the user to use.
		/// </summary>
		public override bool IsCompareExcludedFilesAvailable { get { return true; } }

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
#endif