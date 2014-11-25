using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public interface IFileChange
	{
		string LocalItem { get; }

		string LocalOrServerItem { get; }

		bool IsAdd { get; }

		bool IsDelete { get; }



	}
}
