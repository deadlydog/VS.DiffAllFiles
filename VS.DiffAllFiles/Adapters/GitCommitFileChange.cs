using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;

namespace VS_DiffAllFiles.Adapters
{
	public class GitCommitFileChange : IGitFileChange
	{
		/// <summary>
		/// Handle to the change.
		/// </summary>
		private readonly ICommitDetailsChangeItem _change = null;

		/// <summary>
		/// Stores the Commit version (i.e. SHA), as opposed to each individual file's SHA within the commit.
		/// </summary>
		private readonly string _commitSha = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="GitCommitFileChange" /> class.
		/// </summary>
		/// <param name="change">The change.</param>
		/// <param name="commitSha">The commit sha.</param>
		/// <exception cref="System.ArgumentNullException">change;The Git Change item used to initialize a GitCommitFileChange instance cannot be null.</exception>
		public GitCommitFileChange(ICommitDetailsChangeItem change, string commitSha)
		{
			if (change == null)
				throw new ArgumentNullException("change", "The Git Change item used to initialize a GitCommitFileChange instance cannot be null.");

			_change = change;
			_commitSha = commitSha;
		}

		public string Version
		{
			get { return _change.Sha; }
		}

		public IReadOnlyList<string> PreviousVersions
		{
			get { return _change.PreviousIds; }
		}

		public string CommitVersion
		{
			get { return _commitSha; }
		}

		public string LocalFilePath
		{
			get { return _change.RelativeItem; }
		}

		public string ServerFilePath
		{
			get { return _change.SourceRelativeItem; }
		}

		public string LocalOrServerFilePath
		{
			get { return string.IsNullOrWhiteSpace(_change.RelativeItem) ? _change.SourceRelativeItem : _change.RelativeItem; }
		}

		public string ServerOrLocalFilePath
		{
			get { return string.IsNullOrWhiteSpace(_change.SourceRelativeItem) ? _change.RelativeItem : _change.SourceRelativeItem; }
		}

		public bool IsAdd
		{
			get { return (_change.ChangeType & ChangesChangeType.Add) == ChangesChangeType.Add; }
		}

		public bool IsDelete
		{
			get { return (_change.ChangeType & ChangesChangeType.Delete) == ChangesChangeType.Delete; }
		}
	}
}
