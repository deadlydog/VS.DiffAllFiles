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
		/// The path to the source file and its label.
		/// </summary>
		public FilePathAndLabel SourceFilePathAndLabel { get; private set; }

		/// <summary>
		/// The path to the target file and its label.
		/// </summary>
		public FilePathAndLabel TargetFilePathAndLabel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceAndTargetFilePathsAndLabels"/> class.
		/// </summary>
		/// <param name="sourceFilePath">The source file path.</param>
		/// <param name="targetFilePath">The target file path.</param>
		public SourceAndTargetFilePathsAndLabels(string sourceFilePath, string targetFilePath)
			: this(new FilePathAndLabel(sourceFilePath), new FilePathAndLabel(targetFilePath))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceAndTargetFilePathsAndLabels"/> class.
		/// </summary>
		/// <param name="sourceFilePathAndLabel">The source file path and label.</param>
		/// <param name="targetFilePathAndLabel">The target file path and label.</param>
		public SourceAndTargetFilePathsAndLabels(FilePathAndLabel sourceFilePathAndLabel, FilePathAndLabel targetFilePathAndLabel)
		{
			SourceFilePathAndLabel = sourceFilePathAndLabel;
			TargetFilePathAndLabel = targetFilePathAndLabel;
		}

		/// <summary>
		/// Gets a copy of this instance with new file labels.
		/// </summary>
		/// <param name="sourceFileLabel">The source file label.</param>
		/// <param name="targetFileLabel">The target file label.</param>
		public SourceAndTargetFilePathsAndLabels GetCopyWithNewFileLabels(FileLabel sourceFileLabel, FileLabel targetFileLabel)
		{
			return new SourceAndTargetFilePathsAndLabels(
				new FilePathAndLabel(SourceFilePathAndLabel.FilePath, sourceFileLabel), new FilePathAndLabel(TargetFilePathAndLabel.FilePath, targetFileLabel));
		}
	}
}
