using ArcadeManager.Core.Models;
using ArcadeManager.Core.Models.DataFile;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public interface ICsvFile
{
    /// <summary>
    /// Reads the provided CSV file
    /// </summary>
    /// <param name="reader">The file reader.</param>
    /// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
    /// <returns>
    /// The list of games in the CSV file
    /// </returns>
    Task<CsvGamesList> ReadFile(StreamReader reader, bool getOtherValues);

    /// <summary>
    /// Serializes the specified DAT file entry as a CSV line.
    /// </summary>
    /// <param name="entry">The DAT file entry to serialize.</param>
    /// <returns>The serialized entry</returns>
    string SerializeDatEntry(Entry entry);

    /// <summary>
    /// Serializes the specified ini entry as CSV line.
    /// </summary>
    /// <param name="entry">The ini entry.</param>
    /// <returns>The CSV line</returns>
    string SerializeIniEntry(IniFile.IniEntry entry);

    /// <summary>
    /// Writes the file to the specified target.
    /// </summary>
    /// <param name="output">The output file stream.</param>
    /// <param name="entries">The entries to write.</param>
    /// <returns></returns>
    /// *
    Task WriteFile(StreamWriter output, CsvGamesList entries);
}