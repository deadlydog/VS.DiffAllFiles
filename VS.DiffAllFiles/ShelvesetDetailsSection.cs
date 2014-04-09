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
