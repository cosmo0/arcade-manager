namespace ArcadeManager.Core.Actions.Overlays;

/// <summary>
/// Checks overlay files integrity
/// </summary>
public class CheckAction : BaseAction
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically fix overlays.
    /// </summary>
    public bool AutoFix { get; set; } = false;

    /// <summary>
    /// Gets or sets the error margin for screen position scan
    /// </summary>
    public int ErrorMargin { get; set; } = 0;

    /// <summary>
    /// Gets or sets the path to the overlay configuration in rom configuration (input_overlay).
    /// </summary>
    public string InputOverlayConfigPathInRomConfig { get; set; }

    /// <summary>
    /// Gets or sets the path to the overlays configuration folder.
    /// </summary>
    public string OverlaysConfigFolder { get; set; }

    /// <summary>
    /// Gets or sets the path to the roms configuration folder.
    /// </summary>
    public string RomsConfigFolder { get; set; }

    /// <summary>
    /// Gets or sets the path to the overlay template.
    /// </summary>
    public string TemplateOverlay { get; set; }

    /// <summary>
    /// Gets or sets the rom template.
    /// </summary>
    public string TemplateRom { get; set; }
}