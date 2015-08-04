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
		/// Gets the version (i.e. Sha ID).
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Gets the previous versions (i.e. Sha IDs) of the file.
		/// </summary>
		IReadOnlyList<string> PreviousVersions { get; }
	}
}
