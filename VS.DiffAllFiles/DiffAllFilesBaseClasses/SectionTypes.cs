using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public enum SectionTypes
	{
		None = 0,
		PendingChanges = 1,
		ChangesetDetails = 2,
		ShelvesetDetails = 3,
		GitChanges = 4,
		GitCommitDetails = 5
	}
}
