
namespace ArcadeManager.Core.Models.Roms;

/// <summary>
/// A list of rom files (inside a zip)
/// </summary>
public class GameRomFilesList : List<GameRomFile>
{
    /// <summary>
    /// Gets the rom file details using the specified file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The rom file details, if any</returns>
    public GameRomFile this[string fileName]
    {
        get
        {
            return this.FirstOrDefault(g => g.Name == fileName);
        }
    }

    /// <summary>
    /// Gets a value indicating whether any rom file has an error
    /// </summary>
    public bool HasError => this.Any(rf => rf.HasError);

    /// <summary>
    /// Removes a file from the list
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="filePath">The file path, if any</param>
    public void RemoveFile(string fileName, string filePath = null)
    {
        RemoveAll(f => f.Name == fileName && (string.IsNullOrEmpty(f.Path) && string.IsNullOrEmpty(filePath) || f.Path == filePath));
    }

    /// <summary>
    /// Marks a rom file as fixed
    /// </summary>
    /// <param name="romFileName">The name of the rom file to mark as fixed</param>
    public void Fixed(string romFileName)
    {
        var file = this[romFileName];
        if (file != null) {
            file.ErrorDetails = null;
            file.ErrorReason = ErrorReason.None;
        }
    }
}
