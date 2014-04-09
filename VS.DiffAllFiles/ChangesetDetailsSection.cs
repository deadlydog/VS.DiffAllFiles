using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;

namespace VS_DiffAllFiles
{
	/// <summary>
	/// Diff All Files section in the Changeset Details window.
	/// </summary>
	[TeamExplorerSection(ChangesetDetailsSection.SectionId, TeamExplorerPageIds.ChangesetDetails, 35)]
	public class ChangesetDetailsSection : TfsDiffAllFilesSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "FB6A1BD4-0247-40A4-B476-7C8708AC0CDE";

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
					((_pendingChangesService.SelectedIncludedItems.Length + _pendingChangesService.SelectedExcludedItems.Length) > 0);
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
