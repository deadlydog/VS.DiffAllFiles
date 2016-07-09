using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;

namespace VS_DiffAllFiles.Adapters
{
	public class GitCommitDetailsService : IFileChangesService
	{
		/// <summary>
		/// Handle to the Commit Details Extensibility service.
		/// </summary>
		private readonly ICommitDetailsExt _commitDetailsService = null;

		public GitCommitDetailsService(ICommitDetailsExt commitDetailsService)
		{
			if (commitDetailsService == null)
				throw new ArgumentNullException("commitDetailsService", "The Git Commit Details Service provided cannot be null.");

			// Save a handle to the service and hookup the Property Changed event handler.
			_commitDetailsService = commitDetailsService;
			commitDetailsService.PropertyChanged += commitDetailsService_PropertyChanged;
		}

		void commitDetailsService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			PropertyChanged(sender, e);
		}

		#region IFileChangesService Implementation.

		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = delegate { };

		/// <summary>
		/// Gets the included changes.
		/// </summary>
		public IReadOnlyList<IFileChange> IncludedChanges
		{
			get { return _commitDetailsService.Changes.Select(change => new GitCommitFileChange(change, _commitDetailsService.Sha)).ToList(); }
		}

		/// <summary>
		/// Gets the excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> ExcludedChanges
		{
			get { return new List<IFileChange>(); }
		}

		/// <summary>
		/// Gets the selected included changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedIncludedChanges
		{
			get { return _commitDetailsService.SelectedChanges.Select(change => new GitCommitFileChange(change, _commitDetailsService.Sha)).ToList(); }
		}

		/// <summary>
		/// Gets the selected excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedExcludedChanges
		{
			get { return new List<IFileChange>(); }
		}

		#endregion
	}
}
