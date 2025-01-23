using System.Collections.Generic;

namespace ArcadeManager.Models;

/// <summary>
/// The application settings
/// </summary>
public class Settings {

    /// <summary>
    /// Gets or sets the ignored app versions.
    /// </summary>
    public List<string> IgnoredVersions { get; set; } = new();

    /// <summary>
    /// Gets or sets the OS (Retropie/Recalbox)
    /// </summary>
    public string Os { get; set; }
}