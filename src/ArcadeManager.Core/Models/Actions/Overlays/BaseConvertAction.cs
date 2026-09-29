namespace ArcadeManager.Core.Models.Actions.Overlays;

/// <summary>
/// Base options for conversion
/// </summary>
/// <seealso cref="BaseAction" />
public abstract class BaseConvertAction : BaseAction
{
    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing files
    /// </summary>
    public bool Overwrite { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to scan the bezel for screen position or just convert LAY file.
    /// </summary>
    public bool ScanBezelForScreenCoordinates { get; set; } = false;
}
