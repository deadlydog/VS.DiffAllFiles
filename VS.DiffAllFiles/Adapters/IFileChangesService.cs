using System.Collections.Generic;
using System.ComponentModel;

namespace VS_DiffAllFiles.Adapters
{
	public interface IFileChangesService
	{
		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets the included changes.
		/// </summary>
		IReadOnlyList<IFileChange> IncludedChanges { get; }

		/// <summary>
		/// Gets the excluded changes.
		/// </summary>
		IReadOnlyList<IFileChange> ExcludedChanges { get; }

		/// <summary>
		/// Gets the selected included changes.
		/// </summary>
		IReadOnlyList<IFileChange> SelectedIncludedChanges { get; }

		/// <summary>
		/// Gets the selected excluded changes.
		/// </summary>
		IReadOnlyList<IFileChange> SelectedExcludedChanges { get; }
	}
}
