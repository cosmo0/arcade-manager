using ArcadeManager.Core.Models.DataFile;
using ArcadeManager.Core.Models.Roms;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

/// <summary>
/// Interface for the DAT file processing
/// </summary>
public interface IDatFile
{
    /// <summary>
    /// Gets a roms list from a DAT file
    /// </summary>
    /// <param name="datStream">The DAT file stream.</param>
    /// <param name="folder">The folder of the rom files</param>
    /// <returns>
    /// The roms list
    /// </returns>
    Task<GameRomList> GetRoms(StreamReader datStream, string folder);

    /// <summary>
    /// Reads the specified dat file content.
    /// </summary>
    /// <param name="reader">The input stream reader.</param>
    /// <returns>
    /// The file content, deserialized
    /// </returns>
    Task<Datafile> ReadDatFile(StreamReader reader);
}