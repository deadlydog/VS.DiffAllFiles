using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	public interface IDiffAllFilesSection : INotifyPropertyChanged
	{
		bool IsViewAllFilesEnabled { get; }
		bool IsViewSelectedFilesEnabled { get; }
		bool IsViewIncludedFilesEnabled { get; }
		bool IsViewExcludedFilesEnabled { get; }
	}

	public enum CompareVersions
	{
		PreviousVersion = 0,
		WorkspaceVersion = 1,
		LatestVersion = 2
	}
}
