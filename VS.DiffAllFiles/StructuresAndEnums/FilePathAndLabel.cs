using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public class FilePathAndLabel
	{
		/// <summary>
		/// The File Path.
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// The File Label.
		/// </summary>
		public FileLabel FileLabel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilePathAndLabel"/> class.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <param name="fileLabel">The file label.</param>
		public FilePathAndLabel(string filePath, FileLabel fileLabel)
		{
			FilePath = filePath;
			FileLabel = fileLabel;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilePathAndLabel"/> class with an Empty FileLabel.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		public FilePathAndLabel(string filePath) : this(filePath, FileLabel.Empty)
		{ }
	}
}
