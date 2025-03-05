namespace ArcadeManager.Models.Roms;

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
        this.RemoveAll(f => f.Name == fileName && ((string.IsNullOrEmpty(f.Path) && string.IsNullOrEmpty(filePath)) || f.Path == filePath));
    }

    /// <summary>
    /// Replaces a file in this collection with the specified one
    /// </summary>
    /// <param name="file">The file to replace</param>
    /// <param name="zipFilePath">The path to the zip file we're processing</param>
    public void ReplaceFile(GameRomFile file, string zipFilePath)
    {
        this.RemoveFile(file.Name, file.Path);
        this.Add(file.CloneFor(zipFilePath));
    }
}
