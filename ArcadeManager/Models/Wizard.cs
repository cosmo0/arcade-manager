namespace ArcadeManager.Models;

/// <summary>
/// Model for the wizard
/// </summary>
public class Wizard {

    /// <summary>
    /// Gets or sets a value indicating whether to do the overlays install.
    /// </summary>
    public bool DoOverlays { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to do the roms selection.
    /// </summary>
    public bool DoRoms { get; set; }
}