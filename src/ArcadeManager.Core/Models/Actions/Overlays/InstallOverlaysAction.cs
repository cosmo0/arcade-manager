namespace ArcadeManager.Core.Models.Actions.Overlays;

/// <summary>
/// An overlay action
/// </summary>
public class InstallOverlaysAction
{
    /// <summary>
    /// Gets or sets the path to the configs folder
    /// </summary>
    public string ConfigFolder { get; set; }

    /// <summary>
    /// Gets or sets the target height.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the files should be overwritten
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets or sets the selected pack name
    /// </summary>
    public string Pack { get; set; }

    /// <summary>
    /// Gets or sets the path to the roms folders
    /// </summary>
    public string[] RomFolders { get; set; }

    /// <summary>
    /// Gets or sets the target width
    /// </summary>
    public int Width { get; set; }
}