using System;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
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
			PreviousVersion = 1,
			WorkspaceVersion = 2,
			LatestVersion = 3
		}

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