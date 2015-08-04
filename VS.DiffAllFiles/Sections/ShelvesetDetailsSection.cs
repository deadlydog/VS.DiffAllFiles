using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Controls;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Sections
{
	/// <summary>
	/// Diff All Files section in the Changeset Details window.
	/// </summary>
	[TeamExplorerSection(ShelvesetDetailsSection.SectionId, TeamExplorerPageIds.ShelvesetDetails, 35)]
	public class ShelvesetDetailsSection : SupportsIncludedAndExcludedChangesTfsSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "A40E6620-003D-127D-92DD-1E1E15B157A7";

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.UnmodifiedVersion, CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion };
	}
}
