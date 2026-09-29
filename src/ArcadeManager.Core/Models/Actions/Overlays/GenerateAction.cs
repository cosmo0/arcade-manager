namespace ArcadeManager.Core.Models.Actions.Overlays;

/// <summary>
/// Generates overlay files from images
/// </summary>
public class GenerateAction : BaseAction
{
    /// <summary>
    /// Gets or sets the path to the overlays configuration folder.
    /// </summary>
    public string ImagesFolder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether existing files will be overwritten
    /// </summary>
    public bool Overwrite { get; set; } = false;

    /// <summary>
    /// Gets or sets the path to the roms folder.
    /// </summary>
    public string RomsFolder { get; set; }

    /// <summary>
    /// Gets or sets the path to the overlay template.
    /// </summary>
    public string TemplateOverlay { get; set; }

    /// <summary>
    /// Gets or sets the path to the rom template.
    /// </summary>
    public string TemplateRom { get; set; }
}