using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;

namespace VS_DiffAllFiles
{
	public abstract class GitDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		protected override async Task<bool> GetIfVersionControlServiceIsAvailable()
		{
			// For now just return true always.
			return true;
		}
	}
}
