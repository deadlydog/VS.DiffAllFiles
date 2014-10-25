using System;
using System.Linq;

namespace VS_DiffAllFiles.StructuresAndEnums
{
	/// <summary>
	/// Enum of possible file versions to compare local changes against.
	/// </summary>
	public sealed class CompareVersion
	{
		#region Code that should never have to change.
		private readonly string _name;
		public readonly Values Value;

		private CompareVersion(Values value, String name)
		{
			this._name = name;
			this.Value = value;
		}

		public override String ToString() { return _name; }
		#endregion

		public enum Values
		{
			/// <summary>
			/// TFS - The Shelve File will compare the files with any pending changes to the corresponding files prior to the shelve being created.
			/// Git - Compares your code to the Head of your Git branch.
			/// </summary>
			UnmodifiedVersion = 1,

			/// <summary>
			/// Compares the changeset file's code with the given file's previous changeset code.
			/// </summary>
			PreviousVersion = 2,

			/// <summary>
			/// Typically this will compare your current code running on your machine to the code at the time that you checked out the file.
			/// Basically allowing you to see what changes you have made in the current checkout.
			/// </summary>
			WorkspaceVersion = 3,

			/// <summary>
			/// Compares your code to the most recent code that has been checked into TFS.
			/// </summary>
			LatestVersion = 4
		}

		public static readonly CompareVersion UnmodifiedVersion = new CompareVersion(Values.UnmodifiedVersion, "Unmodified Version");
		public static readonly CompareVersion PreviousVersion = new CompareVersion(Values.PreviousVersion, "Previous Version");
		public static readonly CompareVersion WorkspaceVersion = new CompareVersion(Values.WorkspaceVersion, "Workspace Version");
		public static readonly CompareVersion LatestVersion = new CompareVersion(Values.LatestVersion, "Latest Version");

		/// <summary>
		/// Gets the CompareVersion class that corresponds to the given Value.
		/// </summary>
		/// <param name="value">The Value corresponding to the CompareVersion class to return..</param>
		public static CompareVersion GetCompareVersionFromValue(CompareVersion.Values value)
		{
			CompareVersion version = null;
			switch (value)
			{
				case CompareVersion.Values.UnmodifiedVersion:
					version = CompareVersion.UnmodifiedVersion;
					break;

				case CompareVersion.Values.PreviousVersion:
					version = CompareVersion.PreviousVersion;
					break;

				case CompareVersion.Values.WorkspaceVersion:
					version = CompareVersion.WorkspaceVersion;
					break;

				case CompareVersion.Values.LatestVersion:
					version = CompareVersion.LatestVersion;
					break;
			}
			return version;
		}
	}
}