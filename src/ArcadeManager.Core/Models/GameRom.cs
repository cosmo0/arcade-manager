using System;
using System.Text.Json.Serialization;
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
    private ErrorReason _ownError = ErrorReason.None;

    /// <summary>
    /// Gets or sets the game name (without extension)
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the parent name
    /// </summary>
    public string ParentName { get; set; }

    /// <summary>
    /// Gets or sets the parent data
    /// </summary>
    [JsonIgnore]
    public GameRom Parent { get; set; }

    /// <summary>
    /// Gets or sets the bios
    /// </summary>
    public string BiosName { get; set; }

    /// <summary>
    /// Gets or sets the bios data
    /// </summary>
    [JsonIgnore]
    public GameRom Bios { get; set; }

    /// <summary>
    /// Gets the list of clones
    /// </summary>
    public GameRomList Clones { get; } = [];

    /// <summary>
    /// Gets a value indicating whether this game or any associated file has an error
    /// </summary>
    public bool HasError {
        get {
            if (_ownError != ErrorReason.None) {
                return true;
            }

            if (this.Bios != null && this.Bios.HasError) {
                return true;
            }

            if (this.RomFiles.Any(f => f.HasError)) {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the error reason
    /// </summary>
    public ErrorReason ErrorReason => _ownError;

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
        if (string.IsNullOrEmpty(fileName))
        {
            // if no file name
            this._ownError = reason;
            this.ErrorDetails = details;
            return;
        }
        
        // missing whole file
        if (fileName == $"{this.Name}.zip")
        {
            this._ownError = reason;
            this.ErrorDetails = details;
            return;
        }

        // missing bios
        if (fileName == $"{this.BiosName}.zip")
        {
            this.Bios?.Error(reason, details, fileName);
            this.ErrorDetails = details;
            return;
        }

        // missing file inside
        var file = this.RomFiles[fileName];
        if (file != null)
        {
            file.ErrorReason = reason;
            file.ErrorDetails = details;
            return;
        }

        // default: assign error to the game
        this._ownError = reason;
        this.ErrorDetails = details;
    }

    /// <summary>
    /// Creates a new game rom info from an XML node from the DAT
    /// </summary>
    /// <param name="gameXml">The game XML infos from the DAT</param>
    /// <param name="folder">The game folder</param>
    /// <returns>The game rom infos</returns>
    public static GameRom FromXml(XElement gameXml, string folder) {
        var game = new GameRom {
            Name = gameXml.Attribute("name").Value,
            ParentName = gameXml.Attribute("cloneof")?.Value,
            BiosName = gameXml.Attribute("romof")?.Value
        };

        if (game.ParentName == game.BiosName) {
            game.BiosName = null;
        }

        // add the files
        foreach (var romXml in gameXml.Descendants("rom").Where(r => r.Attribute("status")?.Value != "nodump")) {
            game.RomFiles.Add(GameRomFile.FromXml(game.Name, romXml, folder));
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

    /// <summary>
    /// Gets a value indicating whether any rom file has an error
    /// </summary>
    public bool HasError => this.Any(rf => rf.HasError);

    /// <summary>
    /// Removes a file from the list
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="filePath">The file path, if any</param>
    public void RemoveFile(string fileName, string filePath = null) {
        this.RemoveAll(f => f.Name == fileName && ((string.IsNullOrEmpty(f.Path) && string.IsNullOrEmpty(filePath)) || f.Path == filePath));
    }

    /// <summary>
    /// Replaces a file in this collection with the specified one
    /// </summary>
    /// <param name="file">The file to replace</param>
    /// <param name="game">The related game infos</param>
    public void ReplaceFile(GameRomFile file, GameRom game) {
        this.RemoveFile(file.Name, file.Path);
        this.Add(file.CloneFor(game));
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
    /// Gets or sets the zip file folder
    /// </summary>
    public string ZipFileFolder { get; set; }

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
    /// <param name="game">The game to use data of</param>
    /// <returns>The cloned GameRomFile</returns>
    public GameRomFile CloneFor(GameRom game) {
        return new GameRomFile {
            ZipFileName = $"{game.Name}.zip",
            ZipFileFolder = game.RomFiles.FirstOrDefault()?.ZipFileFolder,
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
    public static GameRomFile FromXml(string game, XElement romXml, string folder) {
        var name = romXml.Attribute("name").Value;

        var crc = romXml.Attribute("crc")?.Value;
        var status = romXml.Attribute("status")?.Value;
        if (crc == null && status != "nodump") {
            throw new ArgumentNullException($"No crc for file {name} in game {game}");
        }

        return new GameRomFile() {
            ZipFileName = $"{game}.zip",
            ZipFileFolder = folder,
            Name = name,
            Size = long.Parse(romXml.Attribute("size")?.Value ?? throw new ArgumentNullException($"No size for file {name} in game {game}")),
            Crc = crc,
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
