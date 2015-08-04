using System;
using System.Collections.Generic;
using System.Linq;
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
			get { return _changesService.IncludedChanges.Select(change => new GitFileChange(change)).ToList(); }
		}

		/// <summary>
		/// Gets the excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> ExcludedChanges
		{
			get { return _changesService.ExcludedChanges.Select(change => new GitFileChange(change)).ToList(); }
		}

		/// <summary>
		/// Gets the selected included changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedIncludedChanges
		{
			get { return _changesService.SelectedIncludedChanges.Select(change => new GitFileChange(change)).ToList(); }
		}

		/// <summary>
		/// Gets the selected excluded changes.
		/// </summary>
		public IReadOnlyList<IFileChange> SelectedExcludedChanges
		{
			get { return _changesService.SelectedExcludedChanges.Select(change => new GitFileChange(change)).ToList(); }
		}

		#endregion
	}
}
