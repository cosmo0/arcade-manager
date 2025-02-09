namespace ArcadeManager.Actions; 

/// <summary>
/// Rom adding action
/// </summary>
public class RomsAction {

	/// <summary>
	/// Gets or sets the path to the main file
	/// </summary>
	public string Main { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to overwrite the files
	/// </summary>
	public bool Overwrite { get; set; }

	/// <summary>
	/// Gets or sets the path to the full romset
	/// </summary>
	public string Romset { get; set; }

	/// <summary>
	/// Gets or sets the path to the roms selection
	/// </summary>
	public string Selection { get; set; }
}