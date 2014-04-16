using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Controls;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Sections
{
	/// <summary>
	/// Diff All Files section in the Pending Changes window.
	/// </summary>
	[TeamExplorerSection(PendingChangesSection.SectionId, TeamExplorerPageIds.PendingChanges, 35)]
	public class PendingChangesSection : SupportsIncludedAndExcludedChangesTfsSectionBase
	{
		/// <summary>
		/// The unique ID of this section.
		/// </summary>
		public const string SectionId = "8C62E1EB-19E1-4652-BD83-817179EF82CD";

		/// <summary>
		/// The possible file versions to compare against.
		/// </summary>
		public override IEnumerable<CompareVersion> CompareVersions
		{
			get { return _compareVersions; }
		}
		private readonly List<CompareVersion> _compareVersions = new List<CompareVersion> { CompareVersion.WorkspaceVersion, CompareVersion.LatestVersion };
	}
}
