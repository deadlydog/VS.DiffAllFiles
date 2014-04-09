using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles
{
	public class FileExtensionDiffToolConfiguration
	{
		/// <summary>
		/// The file extensions that are configured to use this diff tool.
		/// </summary>
		public string FileExtension { get; private set; }

		/// <summary>
		/// The full path to the diff tool's executable.
		/// </summary>
		public string ExecutableFilePath { get; private set; }

		/// <summary>
		/// The format of the arguments to pass to the diff tool.
		/// </summary>
		public string ExecutableArgumentFormat { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileExtensionDiffToolConfiguration"/> class.
		/// </summary>
		/// <param name="fileExtension">The file extension.</param>
		/// <param name="executableFilePath">The executable file path.</param>
		/// <param name="executableArgumentFormat">The executable argument format.</param>
		public FileExtensionDiffToolConfiguration(string fileExtension, string executableFilePath, string executableArgumentFormat)
		{
			FileExtension = fileExtension;
			ExecutableFilePath = executableFilePath;
			ExecutableArgumentFormat = executableArgumentFormat;
		}
	}
}
