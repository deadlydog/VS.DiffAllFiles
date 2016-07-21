using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;

namespace VS_DiffAllFiles.Adapters
{
	public class GitChangesService : IFileChangesService
	{
		/// <summary>
		/// Handle to the Changes Extensibility service.
		/// </summary>
		private readonly IChangesExt _changesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="GitChangesService"/> class.
		/// </summary>
		/// <param name="changesService">The changes service.</param>
		/// <exception cref="System.ArgumentNullException">changesService;The Git Changes Service provided cannot be null.</exception>
		public GitChangesService(IChangesExt changesService)
		{
			if (changesService == null)
				throw new ArgumentNullException("changesService", "The Git Changes Service provided cannot be null.");

			// Save a handle to the service and hookup the Property Changed event handler.
			_changesService = changesService;
			_changesService.PropertyChanged += _changesService_PropertyChanged;
		}

		/// <summary>
		/// Handles the PropertyChanged event of the _changesService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		void _changesService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
			get
			{
				IReadOnlyList<IChangesPendingChangeItem> includedChanges = _changesService.IncludedChanges;

				// We actually want to consider all Staged Changes as included ones, so if we can find that property use it instead, since the backing type's IncludedChanges vary depending on if files are Staged or not.
				var stagedChanges = _changesService.GetType().GetProperty("StagedChanges").GetValue(_changesService) as IReadOnlyList<IChangesPendingChangeItem>;
				if (stagedChanges != null)
					includedChanges = stagedChanges;

				return includedChanges.Select(change => new GitFileChange(change)).ToList();
			}
		}

		/// <summary>
		/// Gets the excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> ExcludedChanges
		{
			get
			{
				IReadOnlyList<IChangesPendingChangeItem> excludedChanges = _changesService.ExcludedChanges;

				// We actually want to consider all Unstaged Changes as excluded ones, so if we can find that property use it instead, since the backing type's ExcludedChanges vary depending on if files are Staged or not.
				var stagedChanges = _changesService.GetType().GetProperty("UnstagedChanges").GetValue(_changesService) as IReadOnlyList<IChangesPendingChangeItem>;
				if (stagedChanges != null)
					excludedChanges = stagedChanges;

				return excludedChanges.Select(change => new GitFileChange(change)).ToList();
			}
		}

		/// <summary>
		/// Gets the selected included changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedIncludedChanges
		{
			get
			{
				IReadOnlyList<IChangesPendingChangeItem> includedChanges = _changesService.SelectedIncludedChanges;

				// We actually want to consider all Staged Changes as included ones, so if we can find that property use it instead, since the backing type's IncludedChanges vary depending on if files are Staged or not.
				var stagedChanges = _changesService.GetType().GetProperty("SelectedStagedChanges").GetValue(_changesService) as IReadOnlyList<IChangesPendingChangeItem>;
				if (stagedChanges != null)
					includedChanges = stagedChanges;

				return includedChanges.Select(change => new GitFileChange(change)).ToList();
			}
		}

		/// <summary>
		/// Gets the selected excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedExcludedChanges
		{
			get
			{
				IReadOnlyList<IChangesPendingChangeItem> excludedChanges = _changesService.ExcludedChanges;

				// We actually want to consider all Unstaged Changes as excluded ones, so if we can find that property use it instead, since the backing type's ExcludedChanges vary depending on if files are Staged or not.
				var stagedChanges = _changesService.GetType().GetProperty("SelectedUnstagedChanges").GetValue(_changesService) as IReadOnlyList<IChangesPendingChangeItem>;
				if (stagedChanges != null)
					excludedChanges = stagedChanges;

				return excludedChanges.Select(change => new GitFileChange(change)).ToList();
			}
		}

		#endregion
	}
}
