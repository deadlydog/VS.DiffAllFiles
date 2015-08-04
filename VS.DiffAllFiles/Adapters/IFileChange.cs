namespace VS_DiffAllFiles.Adapters
{
	public interface IFileChange
	{
		/// <summary>
		/// Gets the local file path.
		/// </summary>
		string LocalFilePath { get; }

		/// <summary>
		/// Gets the server file path.
		/// </summary>
		string ServerFilePath { get; }

		/// <summary>
		/// Gets the local file path if it is not empty, otherwise it gets the server file path.
		/// </summary>
		string LocalOrServerFilePath { get; }

		/// <summary>
		/// Gets the server file path if it is not empty, otherwise it gets the local file path.
		/// </summary>
		string ServerOrLocalFilePath { get; }

		/// <summary>
		/// Get if this file is being added to source control or not.
		/// </summary>
		bool IsAdd { get; }

		/// <summary>
		/// Get if this file is being deleted from source control or not.
		/// </summary>
		bool IsDelete { get; }
	}
}
