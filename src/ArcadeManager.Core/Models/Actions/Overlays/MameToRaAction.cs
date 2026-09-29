namespace ArcadeManager.Core.Models.Actions.Overlays;

/// <summary>
/// Options for conversion from MAME to Retroarch
/// </summary>
public class MameToRaAction : BaseConvertAction
{
    /// <summary>
    /// Gets or sets the path to the output overlays
    /// </summary>
    public string OutputOverlays { get; set; }

    /// <summary>
    /// Gets or sets the path to the output roms
    /// </summary>
    public string OutputRoms { get; set; }

    /// <summary>
    /// Gets or sets the path to the source MAME bezels
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the path to the MAME configs folder
    /// </summary>
    public string SourceConfigs { get; set; }

    /// <summary>
    /// Gets or sets the path to the game config template
    /// </summary>
    public string TemplateGameCfg { get; set; }

    /// <summary>
    /// Gets or sets the path to the overlay config template
    /// </summary>
    public string TemplateOverlayCfg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the first view, if multiple are found
    /// </summary>
    public bool UseFirstView { get; set; } = true;
}