using System.Linq;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public class FileExtensionDiffToolConfiguration
	{
		/// <summary>
		/// The file extensions that are configured to use this diff tool.
		/// </summary>
		public string FileExtension { get; private set; }

		/// <summary>
		/// Gets information required to call the difference tool.
		/// </summary>
		public DiffToolConfiguration DiffToolConfiguration { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileExtensionDiffToolConfiguration"/> class.
		/// </summary>
		/// <param name="fileExtension">The file extension.</param>
		/// <param name="diffToolConfiguration">The difference tool configuration.</param>
		public FileExtensionDiffToolConfiguration(string fileExtension, DiffToolConfiguration diffToolConfiguration)
		{
			FileExtension = fileExtension;
			DiffToolConfiguration = diffToolConfiguration;
		}
	}
}
