using System.Threading.Tasks;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;

namespace VS_DiffAllFiles
{
	public abstract class GitDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		/// <summary>
		/// Gets if version control service is available.
		/// </summary>
		/// <returns></returns>
		protected override Task<bool> GetIfVersionControlServiceIsAvailableAsync()
		{
			// Git uses a local repository which should always be available, so just return true.
			return Task.FromResult(true);
		}

		/// <summary>
		/// Gets if the Compare All Files command should be enabled.
		/// </summary>
		public override bool IsCompareAllFilesEnabled
		{
			get { return !IsRunningCompareFilesCommand && IsVersionControlServiceAvailable; }
		}
	}
}
