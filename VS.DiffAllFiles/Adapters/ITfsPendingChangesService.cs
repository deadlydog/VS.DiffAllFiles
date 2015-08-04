using Microsoft.TeamFoundation.VersionControl.Client;

namespace VS_DiffAllFiles.Adapters
{
	public interface ITfsPendingChangesService : IFileChangesService
	{
		/// <summary>
		/// Gets the workspace.
		/// </summary>
		Workspace Workspace { get; }
	}
}
