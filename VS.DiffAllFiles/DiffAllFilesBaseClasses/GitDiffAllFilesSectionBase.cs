using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.Sections;

namespace VS_DiffAllFiles
{
	public abstract class GitDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GitDiffAllFilesSectionBase"/> class.
		/// </summary>
		protected GitDiffAllFilesSectionBase()
			: base()
		{
			this.Title = "Diff All Files";
			this.IsExpanded = true;
			this.IsBusy = false;
			this.SectionContent = new DiffAllFilesSectionControl(this);
		}

		protected override async Task<bool> GetIfVersionControlServiceIsAvailable()
		{
			// For now just return true always.
			return true;
		}
	}
}
