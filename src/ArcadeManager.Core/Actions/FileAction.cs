namespace ArcadeManager.Actions; 

/// <summary>
/// Represents a file action
/// </summary>
public class FileAction {
    /// <summary>
    /// Gets or sets the source path
    /// </summary>
    public string source { get; set; }

    /// <summary>
    /// Gets or sets the target path
    /// </summary>
    public string target { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite the target path
    /// </summary>
    public bool overwrite { get; set; }
}