using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public class SourceAndTargetFilePathsAndLabels
	{
		/// <summary>
		/// The path to the source file.
		/// </summary>
		public string SourceFilePath { get; private set; }

		/// <summary>
		/// The path to the target file.
		/// </summary>
		public string TargetFilePath { get; private set; }

		/// <summary>
		/// The label for the source file.
		/// </summary>
		public string SourceFileLabel { get; private set; }

		/// <summary>
		/// The label for the target file.
		/// </summary>
		public string TargetFileLabel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceAndTargetFilePathsAndLabels"/> class.
		/// </summary>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="targetFilePath">The target file path.</param>
		public SourceAndTargetFilePathsAndLabels(string sourceFilePath, string targetFilePath)
			: this(sourceFilePath, targetFilePath, string.Empty, string.Empty)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceAndTargetFilePathsAndLabels"/> class.
		/// </summary>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="targetFilePath">The target file path.</param>
		/// <param name="sourceFileLabel">The source file label.</param>
		/// <param name="targetFileLabel">The target file label.</param>
		public SourceAndTargetFilePathsAndLabels(string sourceFilePath, string targetFilePath, string sourceFileLabel, string targetFileLabel)
		{
			SourceFilePath = sourceFilePath;
			TargetFilePath = targetFilePath;
			SourceFileLabel = sourceFileLabel;
			TargetFileLabel = targetFileLabel;
		}
	}
}
