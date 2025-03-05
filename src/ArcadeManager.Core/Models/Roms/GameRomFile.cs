using System.Diagnostics;
using System.Xml.Linq;

namespace ArcadeManager.Models.Roms;

/// <summary>
/// A game rom file (rom files inside the zip)
/// </summary>
[DebuggerDisplay("{Name} ({Crc})")]
public class GameRomFile(string zipFileName, string zipFileFolder) : IGameRomFile
{
    /// <summary>
    /// Gets or sets the zip file name
    /// </summary>
    public string ZipFileName => zipFileName;

    /// <summary>
    /// Gets or sets the zip file folder
    /// </summary>
    public string ZipFileFolder => zipFileFolder;

    /// <summary>
    /// Gets the full zip file path
    /// </summary>
    public string ZipFilePath => System.IO.Path.Join(ZipFileFolder, ZipFileName);

    /// <summary>
    /// Gets or sets the file name (including extension)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the path in the zip
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the file size
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the file CRC
    /// </summary>
    public string Crc { get; set; }

    /// <summary>
    /// Gets or sets the file SHA1 hash
    /// </summary>
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets a value indicating whether this rom file has an error
    /// </summary>
    public bool HasError => ErrorReason != ErrorReason.None;

    /// <summary>
    /// Gets or sets the error reason
    /// </summary>
    public ErrorReason ErrorReason { get; set; } = ErrorReason.None;

    /// <summary>
    /// Gets or sets the error details
    /// </summary>
    public string ErrorDetails { get; set; }

    /// <summary>
    /// Clones a rom file object by replacing some data for the specified game
    /// </summary>
    /// <param name="zipFilePath">The path to the zip file we're processing</param>
    /// <returns>The cloned GameRomFile</returns>
    public GameRomFile CloneFor(string zipFilePath)
    {
        return new GameRomFile(System.IO.Path.GetFileName(zipFilePath), System.IO.Path.GetDirectoryName(zipFilePath))
        {
            Name = this.Name,
            Size = this.Size,
            Crc = this.Crc,
            Sha1 = this.Sha1,
            Path = this.Path,
            ErrorDetails = this.ErrorDetails,
            ErrorReason = this.ErrorReason
        };
    }

    /// <summary>
    /// Creates a new rom file info from a XML node from the DAT
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="romXml">The XML node from the DAT</param>
    /// <param name="folder">The rom file folder</param>
    /// <returns>The file infos</returns>
    public static GameRomFile FromXml(string game, XElement romXml, string folder)
    {
        var name = romXml.Attribute("name").Value;

        var crc = romXml.Attribute("crc")?.Value;
        var status = romXml.Attribute("status")?.Value;
        if (crc == null && status != "nodump")
        {
            throw new ArgumentNullException($"No crc for file {name} in game {game}");
        }

        return new GameRomFile($"{game}.zip", folder)
        {
            Name = name,
            Size = long.Parse(romXml.Attribute("size")?.Value ?? throw new ArgumentNullException($"No size for file {name} in game {game}")),
            Crc = crc,
            Sha1 = romXml.Attribute("sha1")?.Value,
        };
    }
}
