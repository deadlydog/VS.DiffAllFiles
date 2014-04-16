using System.Collections.Generic;
using System.Linq;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	public class DiffToolConfiguration : IEqualityComparer<DiffToolConfiguration>
	{
		/// <summary>
		/// The full path to the diff tool's executable.
		/// </summary>
		public string ExecutableFilePath { get; private set; }

		/// <summary>
		/// The format of the arguments to pass to the diff tool.
		/// </summary>
		public string ExecutableArgumentFormat { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiffToolConfiguration"/> class.
		/// </summary>
		/// <param name="executableFilePath">The executable file path.</param>
		/// <param name="executableArgumentFormat">The executable argument format.</param>
		public DiffToolConfiguration(string executableFilePath, string executableArgumentFormat)
		{
			ExecutableFilePath = executableFilePath;
			ExecutableArgumentFormat = executableArgumentFormat;
		}

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
		/// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
		public bool Equals(DiffToolConfiguration x, DiffToolConfiguration y)
		{
			return x.ExecutableFilePath.Equals(y.ExecutableFilePath) && x.ExecutableArgumentFormat.Equals(y.ExecutableArgumentFormat);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		public int GetHashCode(DiffToolConfiguration obj)
		{
			return obj.ExecutableFilePath.GetHashCode() * obj.ExecutableArgumentFormat.GetHashCode();
		}

		/// <summary>
		/// The Diff Tool Configuration used to identify the default built-in Visual Studio Diff Tool.
		/// </summary>
		public static readonly DiffToolConfiguration VsBuiltInDiffToolConfiguration = new DiffToolConfiguration("VsBuiltInDiffTool", string.Empty);
	}
}
