using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace VS_DiffAllFiles.Adapters
{
	public class TfsFileChange : ITfsFileChange
	{
		/// <summary>
		/// Handle to the pending change.
		/// </summary>
		private readonly PendingChange _pendingChange = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="TfsFileChange"/> class.
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		public TfsFileChange(PendingChange pendingChange)
		{
			if (pendingChange == null)
				throw new ArgumentNullException("pendingChange", "The TFS Pending Change used to initialize a TfsFileChange instance cannot be null.");

			_pendingChange = pendingChange;
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		public int Version
		{
			get { return _pendingChange.Version; }
		}

		/// <summary>
		/// Gets the name of the shelveset.
		/// </summary>
		public string ShelvesetName
		{
			get { return _pendingChange.PendingSetName; }
		}

		/// <summary>
		/// Gets the TFS version control server.
		/// </summary>
		public VersionControlServer VersionControlServer
		{
			get { return _pendingChange.VersionControlServer; }
		}

		/// <summary>
		/// Gets the deletion identifier.
		/// </summary>
		public int DeletionId
		{
			get { return _pendingChange.DeletionId; }
		}

		/// <summary>
		/// Downloads the base file.
		/// </summary>
		/// <param name="filePathToDownloadFileTo">The file path to download file to.</param>
		public void DownloadBaseFile(string filePathToDownloadFileTo)
		{
			_pendingChange.DownloadBaseFile(filePathToDownloadFileTo);
		}

		/// <summary>
		/// Downloads the shelved file.
		/// </summary>
		/// <param name="filePathToDownloadFileTo">The file path to download file to.</param>
		public void DownloadShelvedFile(string filePathToDownloadFileTo)
		{
			_pendingChange.DownloadShelvedFile(filePathToDownloadFileTo);
		}

		/// <summary>
		/// Gets the local file path.
		/// </summary>
		public string LocalFilePath
		{
			get { return _pendingChange.LocalItem; }
		}

		/// <summary>
		/// Gets the server file path.
		/// </summary>
		public string ServerFilePath
		{
			get { return _pendingChange.ServerItem; }
		}

		/// <summary>
		/// Gets the local file path if it is not empty, otherwise it gets the server file path.
		/// </summary>
		public string LocalOrServerFilePath
		{
			get { return _pendingChange.LocalOrServerItem; }
		}

		/// <summary>
		/// Gets the server file path if it is not empty, otherwise it gets the local file path.
		/// </summary>
		public string ServerOrLocalFilePath
		{
			get { return string.IsNullOrWhiteSpace(_pendingChange.ServerItem) ? _pendingChange.LocalItem : _pendingChange.ServerItem; }
		}

		/// <summary>
		/// Get if this file is being added to source control or not.
		/// </summary>
		public bool IsAdd
		{
			get { return _pendingChange.IsAdd; }
		}

		/// <summary>
		/// Get if this file is being deleted from source control or not.
		/// </summary>
		public bool IsDelete
		{
			get { return _pendingChange.IsDelete; }
		}

		/// <summary>
		/// Gets if this file is being branched or not.
		/// </summary>
		public bool IsBranch
		{
			get { return _pendingChange.IsBranch; }
		}

		/// <summary>
		/// Gets if this file is being undeleted or not.
		/// </summary>
		public bool IsUndelete
		{
			get { return _pendingChange.IsUndelete; }
		}
	}
}
