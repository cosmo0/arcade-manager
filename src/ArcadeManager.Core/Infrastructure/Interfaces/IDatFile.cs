using System;
using ArcadeManager.Models;
using ArcadeManager.Models.Roms;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// Interface for the DAT file processing
/// </summary>
public interface IDatFile
{
    /// <summary>
    /// Gets a roms list from a DAT file
    /// </summary>
    /// <param name="datFilePath">The DAT file path</param>
    /// <param name="folder">The folder of the rom files</param>
    /// <returns>The roms list</returns>
    Task<GameRomList> GetRoms(string datFilePath, string folder);
}
