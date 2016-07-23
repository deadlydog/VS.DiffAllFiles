using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.Adapters
{
	public interface IGitFileChange : IFileChange
	{
		/// <summary>
		/// Gets the file version (i.e. Sha ID).
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Gets the previous versions (i.e. Sha IDs) of the file.
		/// </summary>
		IReadOnlyList<string> PreviousVersions { get; }

		/// <summary>
		/// Gets the Commit version (i.e. Sha ID) that the file was part of.
		/// </summary>
		string CommitVersion { get; }

		/// <summary>
		/// Get if this file is staged or not, so we know whether to compare the previous version against the local changes or the Staged changes.
		/// </summary>
		bool IsStaged { get; }
	}
}
