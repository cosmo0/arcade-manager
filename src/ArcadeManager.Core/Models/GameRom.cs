using System;
using System.Xml.Linq;

namespace ArcadeManager.Models;

/// <summary>
/// A game rom (zip)
/// </summary>
public class GameRom {
    /// <summary>
    /// Gets or sets the game name (without extension)
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the parent
    /// </summary>
    public string CloneOf { get; set; }

    /// <summary>
    /// Gets or sets the bios, or the parent
    /// </summary>
    public string RomOf { get; set; }

    /// <summary>
    /// Gets the list of rom files from the DAT
    /// </summary>
    public List<GameRomFile> RomFilesFromDat { get; } = [];

    /// <summary>
    /// Gets the list of rom files found on the disk
    /// </summary>
    public List<GameRomFile> RomFilesOnDisk { get; } = [];

    /// <summary>
    /// Creates a new game rom info from an XML node from the DAT
    /// </summary>
    /// <param name="gameXml">The game XML infos from the DAT</param>
    /// <returns>The game rom infos</returns>
    public static GameRom FromXml(XElement gameXml) {
        var game = new GameRom {
            Name = gameXml.Attribute("name").Value,
            CloneOf = gameXml.Attribute("cloneof")?.Value,
            RomOf = gameXml.Attribute("romof")?.Value
        };

        foreach (var romXml in gameXml.Descendants("rom")) {
            game.RomFilesFromDat.Add(GameRomFile.FromXml(game.Name, romXml));
        }

        return game;
    }
}

/// <summary>
/// A game rom file (rom files inside the zip)
/// </summary>
public class GameRomFile {
    /// <summary>
    /// Gets or sets the file name (including extension)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the file size
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the file CRC
    /// </summary>
    public string Crc { get; set; }

    /// <summary>
    /// Gets or sets the file SHA1 hash
    /// </summary>
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the file status, if any
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Creates a new rom file info from a XML node from the DAT
    /// </summary>
    /// <param name="romXml">The XML node from the DAT</param>
    /// <returns>The file infos</returns>
    public static GameRomFile FromXml(string game, XElement romXml) {
        var name = romXml.Attribute("name").Value;

        var crc = romXml.Attribute("crc")?.Value;
        var status = romXml.Attribute("status")?.Value;
        if (crc == null && status != "nodump") {
            throw new ArgumentNullException($"No crc for file {name} in game {game}");
        }

        return new GameRomFile() {
            Name = name,
            Size = int.Parse(romXml.Attribute("size")?.Value ?? throw new ArgumentNullException($"No size for file {name} in game {game}")),
            Crc = crc,
            Status = status,
            Sha1 = romXml.Attribute("sha1")?.Value,
        };
    }
}

/// <summary>
/// A game error details
/// </summary>
public class GameError {
    /// <summary>
    /// Gets or sets the game name
    /// </summary>
    public string Game { get; set; }

    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string File { get; set; }

    /// <summary>
    /// Gets or sets the error reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Gets or sets the error details, if any
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// Creates a new "missing game" error
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="file">The file name</param>
    /// <returns>The error details</returns>
    public static GameError Missing(string game, string file)
    {
        return new GameError {
            Game = game,
            File = file,
            Reason = "missing",
            Details = "File missing"
        };
    }

    /// <summary>
    /// Creates a new "bad hash" error
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="file">The file name</param>
    /// <param name="details">The error details, if any</param>
    /// <returns>The error details</returns>
    public static GameError BadHash(string game, string file, string details = null)
    {
        return new GameError {
            Game = game,
            File = file,
            Reason = "badhash",
            Details = details
        };
    }
}