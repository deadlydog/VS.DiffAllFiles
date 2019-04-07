using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;

namespace VS_DiffAllFiles.Adapters
{
	public class TfsPendingChangesService : ITfsPendingChangesService
	{
		/// <summary>
		/// Handle to the Pending Changes Extensibility service.
		/// </summary>
		protected IPendingChangesExt _pendingChangesService = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="TfsPendingChangesService"/> class.
		/// </summary>
		/// <param name="pendingChangesService">The pending changes service.</param>
		/// <exception cref="System.ArgumentNullException">pendingChangesService;The Tfs Pending Changes Service provided cannot be null.</exception>
		public TfsPendingChangesService(IPendingChangesExt pendingChangesService)
		{
			if (pendingChangesService == null)
				throw new ArgumentNullException("pendingChangesService", "The TFS Pending Changes Service provided cannot be null.");

			// Save a handle to the service and hookup the Property Changed event handler.
			_pendingChangesService = pendingChangesService;
			pendingChangesService.PropertyChanged += pendingChangesService_PropertyChanged;
		}

		/// <summary>
		/// Handles the PropertyChanged event of the pendingChangesService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		void pendingChangesService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			PropertyChanged(sender, e);
		}

		#region ITfsPendingChangesService Implementation.

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
#if (!VS2019)
				return _pendingChangesService.FilteredIncludedChanges.Length > 0
					  ? _pendingChangesService.FilteredIncludedChanges.Select(pendingChange => new TfsFileChange(pendingChange)).ToList()
					  : _pendingChangesService.IncludedChanges.Select(pendingChange => new TfsFileChange(pendingChange)).ToList();
#else
				return _pendingChangesService.SelectedIncludedItems.Length > 0
					  ? _pendingChangesService.SelectedIncludedItems.Select(pendingChange => new TfsFileChange(pendingChange)).ToList()
					  : _pendingChangesService.SelectedIncludedItems.Select(pendingChange => new TfsFileChange(pendingChange)).ToList();
#endif
			}
		}

		/// <summary>
		/// Gets the excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> ExcludedChanges
		{
			get
			{
#if (!VS2019)
				return _pendingChangesService.FilteredExcludedChanges.Length > 0
					  ? _pendingChangesService.FilteredExcludedChanges.Select(pendingChange => new TfsFileChange(pendingChange)).ToList()
					  : _pendingChangesService.ExcludedChanges.Select(pendingChange => new TfsFileChange(pendingChange)).ToList();
#else
				return _pendingChangesService.SelectedExcludedItems.Length > 0
					  ? _pendingChangesService.SelectedExcludedItems.Select(pendingChange => new TfsFileChange(pendingChange)).ToList()
					  : _pendingChangesService.ExcludedChanges.Select(pendingChange => new TfsFileChange(pendingChange)).ToList();
#endif
			}
		}

		/// <summary>
		/// Gets the selected included changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedIncludedChanges
		{
			get { return _pendingChangesService.SelectedIncludedItems.Where(i => i.IsPendingChange).Select(i => new TfsFileChange(i.PendingChange)).ToList(); }
		}

		/// <summary>
		/// Gets the selected excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedExcludedChanges
		{
			get { return _pendingChangesService.SelectedExcludedItems.Where(i => i.IsPendingChange).Select(i => new TfsFileChange(i.PendingChange)).ToList(); }
		}

		/// <summary>
		/// Gets the workspace.
		/// </summary>
		public Microsoft.TeamFoundation.VersionControl.Client.Workspace Workspace
		{
			get { return _pendingChangesService.Workspace; }
		}

#endregion
	}
}
