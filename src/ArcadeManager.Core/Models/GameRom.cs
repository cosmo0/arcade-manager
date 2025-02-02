using System;
using System.Xml.Linq;

namespace ArcadeManager.Models;

/// <summary>
/// A list of game roms (zip)
/// </summary>
public class GameRomList : List<GameRom> {
    /// <summary>
    /// Gets a game by its name
    /// </summary>
    /// <param name="name">The game name</param>
    /// <returns>The game, if found</returns>
    public GameRom this[string name] {
        get {
            return this.FirstOrDefault(g => g.Name == name);
        }
    }
}

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
    /// Gets a value indicating whether this game has an error
    /// </summary>
    public bool HasError => ErrorReason != ErrorReason.None;

    /// <summary>
    /// Gets or sets the error reasons
    /// </summary>
    public ErrorReason ErrorReason { get; set; } = ErrorReason.None;

    /// <summary>
    /// Gets or sets the error details, if any
    /// </summary>
    public string ErrorDetails { get; set; }

    /// <summary>
    /// Gets the list of rom files
    /// </summary>
    public GameRomFilesList RomFiles { get; } = [];

    /// <summary>
    /// Sets an error on the game
    /// </summary>
    /// <param name="reason">The error reason</param>
    /// <param name="details">The error details</param>
    /// <param name="fileName">The related file name</param>
    public void Error(ErrorReason reason, string details, string fileName) {
        if (!string.IsNullOrEmpty(fileName)) {
            var file = this.RomFiles[fileName];
            if (file != null) {
                file.ErrorReason = reason;
                file.ErrorDetails = details;
                return;
            }
        }

        // if no file name or not found
        this.ErrorReason = reason;
        this.ErrorDetails = details;
    }

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

        // add the files
        foreach (var romXml in gameXml.Descendants("rom")) {
            game.RomFiles.Add(GameRomFile.FromXml(game.Name, romXml));
        }

        return game;
    }
}

/// <summary>
/// A list of rom files (inside a zip)
/// </summary>
public class GameRomFilesList : List<GameRomFile> {
    /// <summary>
    /// Gets the rom file details using the specified file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The rom file details, if any</returns>
    public GameRomFile this[string fileName] {
        get {
            return this.FirstOrDefault(g => g.Name == fileName);
        }
    }
}

/// <summary>
/// A game rom file (rom files inside the zip)
/// </summary>
public class GameRomFile {
    /// <summary>
    /// Gets or sets the zip file name
    /// </summary>
    public string ZipFileName { get; set; }

    /// <summary>
    /// Gets or sets the file name (including extension)
    /// </summary>
    public string Name { get; set; }

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
    /// Gets or sets the file status, if any
    /// </summary>
    public string Status { get; set; }

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
    /// Creates a new rom file info from a XML node from the DAT
    /// </summary>
    /// <param name="game">The game name</param>
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
            ZipFileName = $"{name}.zip",
            Name = name,
            Size = long.Parse(romXml.Attribute("size")?.Value ?? throw new ArgumentNullException($"No size for file {name} in game {game}")),
            Crc = crc,
            Status = status,
            Sha1 = romXml.Attribute("sha1")?.Value,
        };
    }
}

/// <summary>
/// A reason of an error on a game or file
/// </summary>
public enum ErrorReason {
    None,
    MissingFile,
    BadHash
}
