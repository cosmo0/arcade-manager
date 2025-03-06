using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace ArcadeManager.Core.Models.Roms;

/// <summary>
/// A game rom file (rom files inside the zip)
/// </summary>
[DebuggerDisplay("{Path} \\ {Name} ({Crc})")]
public class GameRomFile : IGameRomFile
{
    [JsonIgnore]
    public GameRom Rom { get; set; }

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

    public ReadOnlyGameRomFile ToReadOnly(string filePath) {
        var fileName = System.IO.Path.GetFileName(filePath);
        var folderName = System.IO.Path.GetDirectoryName(filePath);

        return new ReadOnlyGameRomFile(fileName, folderName, Name, Path, Size, Crc, Sha1);
    }

    /// <summary>
    /// Creates a new rom file info from a XML node from the DAT
    /// </summary>
    /// <param name="game">The game</param>
    /// <param name="romXml">The XML node from the DAT</param>
    /// <returns>The file infos</returns>
    public static GameRomFile FromXml(GameRom game, XElement romXml)
    {
        var name = romXml.Attribute("name").Value;

        var crc = romXml.Attribute("crc")?.Value;
        var status = romXml.Attribute("status")?.Value;
        if (crc == null && status != "nodump")
        {
            throw new ArgumentNullException($"No crc for file {name} in game {game.Name}");
        }

        return new GameRomFile()
        {
            Name = name,
            Size = long.Parse(romXml.Attribute("size")?.Value ?? throw new ArgumentNullException($"No size for file {name} in game {game.Name}")),
            Crc = crc,
            Sha1 = romXml.Attribute("sha1")?.Value,
            Rom = game
        };
    }
}
