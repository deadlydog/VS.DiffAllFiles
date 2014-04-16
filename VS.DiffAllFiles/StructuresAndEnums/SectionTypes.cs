using System.Linq;

namespace VS_DiffAllFiles.StructuresAndEnums
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
