using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VS_DiffAllFiles;

namespace DansKingdom.VS_DiffAllFiles_VS2022
{
	// Assign the VS 2019 Guid that matches the VSIX manifest to this package.
	[Guid(Guids.guidVS_DiffAllFiles_VS2022PkgString)]
	public sealed class VS_DiffAllFiles_VS2022Package : VS_DiffAllFilesPackage
	{ }
}
