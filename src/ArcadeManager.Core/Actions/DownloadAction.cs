namespace ArcadeManager.Actions; 

/// <summary>
/// A download action request
/// </summary>
public class DownloadAction {

	/// <summary>
	/// Gets or sets the details.
	/// </summary>
	public string Details { get; set; }

	/// <summary>
	/// Gets or sets the folder.
	/// </summary>
	public string Folder { get; set; }

	/// <summary>
	/// Gets or sets the local file path.
	/// </summary>
	public string LocalFile { get; set; }

	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the repository (ex: cosmo0/arcade-manager-data)
	/// </summary>
	public string Repository { get; set; }
}