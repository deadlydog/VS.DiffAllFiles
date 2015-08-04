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
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using VS_DiffAllFiles.Adapters;
using VS_DiffAllFiles.Sections;
using VS_DiffAllFiles.Settings;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.DiffAllFilesBaseClasses
{
	public abstract class TfsDiffAllFilesSectionBase : DiffAllFilesSectionBase
	{
		/// <summary>
		/// Initialize override.
		/// </summary>
		public override void Initialize(object sender, SectionInitializeEventArgs e)
		{
			base.Initialize(sender, e);

			// Find the Pending Changes extensibility service and save a handle to it.
			FileChangesService = new TfsPendingChangesService(this.GetService<IPendingChangesExt>());

			// Register for property change notifications on the Pending Changes window.
			if (FileChangesService != null)
				FileChangesService.PropertyChanged += _pendingChangesService_PropertyChanged;

			// Make sure the Version Control is available on load.
			Refresh();
		}

		/// <summary>
		/// Dispose override.
		/// </summary>
		public override void Dispose()
		{
			if (FileChangesService != null)
				FileChangesService.PropertyChanged -= _pendingChangesService_PropertyChanged;
			FileChangesService = null;

			base.Dispose();
		}

		/// <summary>
		/// Pending Changes Extensibility PropertyChanged event handler.
		/// </summary>
		protected virtual void _pendingChangesService_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IncludedChanges":
				case "FilteredIncludedChanges":
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "ExcludedChanges":
				case "FilteredExcludedChanges":
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;

				case "SelectedIncludedItems":
				case "SelectedExcludedItems":
					NotifyPropertyChanged("IsCompareSelectedFilesEnabled");
					NotifyPropertyChanged("IsCompareAllFilesEnabled");
					break;
			}
		}

		/// <summary>
		/// Gets if the version control service is available or not.
		/// </summary>
		/// <returns></returns>
		protected override async Task<bool> GetIfVersionControlServiceIsAvailable()
		{
			// Make sure we have a connection to Team Foundation.
			ITeamFoundationContext context = this.CurrentContext;
			if (context == null || !context.HasCollection)
				return false;

			// Make sure we can access the Version Control Server.
			VersionControlServer versionControlService = null;
			await Task.Run(() => versionControlService = context.TeamProjectCollection.GetService<VersionControlServer>());

			// Return if we could connect to the TFS Version Control or not.
			return versionControlService != null;
		}
	}
}
