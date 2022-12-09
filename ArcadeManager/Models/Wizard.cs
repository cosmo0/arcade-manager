using System.Collections.Generic;

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

    /// <summary>
    /// Gets or sets the target emulator.
    /// </summary>
    public string Emulator { get; set; }

    /// <summary>
    /// Gets or sets the number of games in lists.
    /// </summary>
    public Dictionary<string, int> GameNumbers { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Gets or sets the roms lists.
    /// </summary>
    public string[] Lists { get; set; }
}