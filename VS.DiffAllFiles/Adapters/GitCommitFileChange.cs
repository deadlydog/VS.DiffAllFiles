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
		/// Initializes a new instance of the <see cref="GitCommitFileChange"/> class.
		/// </summary>
		/// <param name="change">The change.</param>
		public GitCommitFileChange(ICommitDetailsChangeItem change)
		{
			if (change == null)
				throw new ArgumentNullException("change", "The Git Change item used to initialize a GitCommitFileChange instance cannot be null.");

			_change = change;
		}

		public string Version
		{
			get { return _change.Sha; }
		}

		public IReadOnlyList<string> PreviousVersions
		{
			get { return _change.PreviousIds; }
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
			get { return _change.ChangeType == ChangesChangeType.Add; }
		}

		public bool IsDelete
		{
			get { return _change.ChangeType == ChangesChangeType.Delete; }
		}
	}
}
