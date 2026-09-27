namespace ArcadeManager.Core.Actions.Overlays;

/// <summary>
/// Converts a Retroarch overlay to a MAME bezel
/// </summary>
public class RaToMameAction : BaseConvertAction
{
    /// <summary>
    /// Gets or sets the path to the output
    /// </summary>
    public string Output { get; set; }

    /// <summary>
    /// Gets or sets the path to the source Retroarch overlays configs
    /// </summary>
    public string SourceConfigs { get; set; }

    /// <summary>
    /// Gets or sets the path to the source Retroarch roms configs
    /// </summary>
    public string SourceRoms { get; set; }

    /// <summary>
    /// Gets or sets the path to the game config template
    /// </summary>
    public string Template { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the bezels will be zipped
    /// </summary>
    public bool Zip { get; set; } = false;
}