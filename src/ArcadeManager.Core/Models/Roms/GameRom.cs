using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace ArcadeManager.Core.Models.Roms;

/// <summary>
/// A game rom (zip)
/// </summary>
[DebuggerDisplay("{Name}")]
public class GameRom
{
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
    public bool HasError
    {
        get
        {
            if (_ownError != ErrorReason.None)
            {
                return true;
            }

            if (Bios != null && Bios.HasError)
            {
                return true;
            }

            if (RomFiles.Any(f => f.HasError))
            {
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
    /// Gets a value indicating whether the game itself has an error (ex the whole file is missing)
    /// </summary>
    public bool HasOwnError => _ownError != ErrorReason.None;

    /// <summary>
    /// Sets an error on the game
    /// </summary>
    /// <param name="reason">The error reason</param>
    /// <param name="details">The error details</param>
    /// <param name="fileName">The related file name</param>
    public void Error(ErrorReason reason, string details, string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            // if no file name
            _ownError = reason;
            ErrorDetails = details;
            return;
        }

        // missing whole file
        if (fileName == $"{Name}.zip")
        {
            _ownError = reason;
            ErrorDetails = details;
            return;
        }

        // missing bios
        if (fileName == $"{BiosName}.zip")
        {
            Bios?.Error(reason, details, fileName);
            ErrorDetails = details;
            return;
        }

        // missing file inside
        var file = RomFiles[fileName];
        if (file != null)
        {
            file.ErrorReason = reason;
            file.ErrorDetails = details;
            return;
        }

        // default: assign error to the game
        _ownError = reason;
        ErrorDetails = details;
    }

    /// <summary>
    /// Creates a new game rom info from an XML node from the DAT
    /// </summary>
    /// <param name="gameXml">The game XML infos from the DAT</param>
    /// <param name="folder">The game folder</param>
    /// <returns>The game rom infos</returns>
    public static GameRom FromXml(XElement gameXml, string folder)
    {
        var game = new GameRom
        {
            Name = gameXml.Attribute("name").Value,
            ParentName = gameXml.Attribute("cloneof")?.Value,
            BiosName = gameXml.Attribute("romof")?.Value
        };

        if (game.ParentName == game.BiosName)
        {
            game.BiosName = null;
        }

        // add the files
        foreach (var romXml in gameXml.Descendants("rom").Where(r => r.Attribute("status")?.Value != "nodump"))
        {
            game.RomFiles.Add(GameRomFile.FromXml(game, romXml));
        }

        return game;
    }
}
