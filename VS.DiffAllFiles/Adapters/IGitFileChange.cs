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
		/// Gets the version.
		/// </summary>
		string Version { get; }
	}
}
