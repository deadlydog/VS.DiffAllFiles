using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace VS_DiffAllFiles.Adapters
{
	public interface ITfsFileChange : IFileChange
	{
		/// <summary>
		/// Gets the version.
		/// </summary>
		int Version { get; }

		/// <summary>
		/// Gets the name of the shelveset.
		/// </summary>
		string ShelvesetName { get; }

		/// <summary>
		/// Gets the TFS version control server.
		/// </summary>
		VersionControlServer VersionControlServer { get; }

		/// <summary>
		/// Gets the deletion identifier.
		/// </summary>
		int DeletionId { get; }

		/// <summary>
		/// Gets if this file is being branched or not.
		/// </summary>
		bool IsBranch { get; }

		/// <summary>
		/// Gets if this file is being undeleted or not.
		/// </summary>
		bool IsUndelete { get; }

		/// <summary>
		/// Downloads the base file.
		/// </summary>
		/// <param name="filePathToDownloadFileTo">The file path to download file to.</param>
		void DownloadBaseFile(string filePathToDownloadFileTo);

		/// <summary>
		/// Downloads the shelved file.
		/// </summary>
		/// <param name="filePathToDownloadFileTo">The file path to download file to.</param>
		void DownloadShelvedFile(string filePathToDownloadFileTo);
	}
}
