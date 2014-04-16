using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public class FileLabel
	{
		/// <summary>
		/// The Prefix to use.
		/// </summary>
		public string Prefix { get; private set; }

		/// <summary>
		/// The FilePath to use.
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// The Version to use.
		/// </summary>
		public string Version { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileLabel"/> class.
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <param name="filePath">The file path.</param>
		/// <param name="version">The version.</param>
		public FileLabel(string prefix, string filePath, string version)
		{
			Prefix = prefix;
			FilePath = filePath;
			Version = version;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileLabel"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		public FileLabel(string version) : this(string.Empty, string.Empty, version)
		{ }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			string stringToReturn = string.Empty;

			// If there is a Prefix, use it with a colon after it.
			if (!string.IsNullOrWhiteSpace(Prefix))
				stringToReturn = string.Format("{0}: ", Prefix);

			stringToReturn = string.Format("{0}{1}", stringToReturn, ToStringWithoutPrefix());

			return stringToReturn;
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance without the Prefix.
		/// </summary>
		public string ToStringWithoutPrefix()
		{
			string stringToReturn = string.Empty;

			// If there is both a FilePath and Version, separate them with a semicolon.
			if (!string.IsNullOrWhiteSpace(FilePath) && !string.IsNullOrWhiteSpace(Version))
				stringToReturn = string.Format("{0};{1}", FilePath, Version);
			// Else one or both of the FilePath and Version are empty, so include them without a separator.
			else
				stringToReturn = string.Format("{0}{1}", FilePath, Version);

			return stringToReturn;
		}

		/// <summary>
		/// An Empty FileLabel instance.
		/// </summary>
		public static readonly FileLabel Empty = new FileLabel(string.Empty, string.Empty, string.Empty);
	}
}
