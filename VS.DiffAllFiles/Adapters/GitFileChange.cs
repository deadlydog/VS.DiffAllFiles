using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;

namespace VS_DiffAllFiles.Adapters
{
	public class GitFileChange : IGitFileChange
	{
		/// <summary>
		/// Handle to the pending change.
		/// </summary>
		private readonly IChangesPendingChangeItem _pendingChange = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="GitFileChange"/> class.
		/// </summary>
		/// <param name="pendingChange">The pending change.</param>
		public GitFileChange(IChangesPendingChangeItem pendingChange)
		{
			if (pendingChange == null)
				throw new ArgumentNullException("pendingChange", "The Git Pending Change used to initialize a GitFileChange instance cannot be null.");

			_pendingChange = pendingChange;
		}

		public string Version
		{
			get { return null; }
		}

		public IReadOnlyList<string> PreviousVersions
		{
			get { return new List<string>(); }
		}

		public string LocalFilePath
		{
			get { return _pendingChange.LocalItem; }
		}

		public string ServerFilePath
		{
			get { return _pendingChange.SourceLocalItem; }
		}

		public string LocalOrServerFilePath
		{
			get { return string.IsNullOrWhiteSpace(_pendingChange.LocalItem) ? _pendingChange.SourceLocalItem : _pendingChange.LocalItem; }
		}

		public string ServerOrLocalFilePath
		{
			get { return string.IsNullOrWhiteSpace(_pendingChange.SourceLocalItem) ? _pendingChange.LocalItem : _pendingChange.SourceLocalItem; }
		}

		public bool IsAdd
		{
			get { return (_pendingChange.ChangeType & ChangesChangeType.Add) == ChangesChangeType.Add; }
		}

		public bool IsDelete
		{
			get { return (_pendingChange.ChangeType & ChangesChangeType.Delete) == ChangesChangeType.Delete; }
		}
	}
}
