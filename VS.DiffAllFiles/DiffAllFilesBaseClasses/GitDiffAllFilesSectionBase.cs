using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.TeamFoundation.VersionControl.Client;
using VS_DiffAllFiles.Adapters;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles
{
	public abstract class GitDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		/// <summary>
		/// Gets if version control service is available.
		/// </summary>
		/// <returns></returns>
		protected override async Task<bool> GetIfVersionControlServiceIsAvailable()
		{
			// Git uses a local repository which should always be available, so just return true.
			return true;
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
