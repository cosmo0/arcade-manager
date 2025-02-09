namespace ArcadeManager.Actions; 

/// <summary>
/// Represents a file action
/// </summary>
public class FileAction {
    /// <summary>
    /// Gets or sets the source path
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the target path
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite the target path
    /// </summary>
    public bool Overwrite { get; set; }
}