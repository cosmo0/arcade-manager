namespace ArcadeManager.Core.Actions;

/// <summary>
/// Represents a CSV action message
/// </summary>
public class CsvAction
{
    /// <summary>
    /// Gets or sets the path to the main file.
    /// </summary>
    public string Main { get; set; }

    /// <summary>
    /// Gets or sets the path to the secondary file.
    /// </summary>
    public string Secondary { get; set; }

    /// <summary>
    /// Gets or sets the path to the target file.
    /// </summary>
    public string Target { get; set; }
}