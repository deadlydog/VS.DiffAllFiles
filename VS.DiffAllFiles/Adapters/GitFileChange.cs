using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.Adapters
{
	public class GitFileChange : IGitFileChange
	{
		// will probably also need a GitCommitFileChange class as well.


		public string Version
		{
			get { throw new NotImplementedException(); }
		}

		public string LocalFilePath
		{
			get { throw new NotImplementedException(); }
		}

		public string ServerFilePath
		{
			get { throw new NotImplementedException(); }
		}

		public string LocalOrServerFilePath
		{
			get { throw new NotImplementedException(); }
		}

		public string ServerOrLocalFilePath
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsAdd
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsDelete
		{
			get { throw new NotImplementedException(); }
		}
	}
}
