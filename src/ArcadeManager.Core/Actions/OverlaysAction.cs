namespace ArcadeManager.Core.Actions; 

/// <summary>
/// An overlay action
/// </summary>
public class OverlaysAction {

	/// <summary>
	/// Gets or sets the path to the configs folder
	/// </summary>
	public string ConfigFolder { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the files should be overwritten
	/// </summary>
	public bool Overwrite { get; set; }

	/// <summary>
	/// Gets or sets the selected pack name
	/// </summary>
	public string Pack { get; set; }

	/// <summary>
	/// Gets or sets the target size ratio
	/// </summary>
	public float Ratio { get; set; }

	/// <summary>
	/// Gets or sets the path to the roms folders
	/// </summary>
	public string[] RomFolders { get; set; }
}