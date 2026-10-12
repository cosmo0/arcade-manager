using ArcadeManager.Core.Models;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public interface ICsvFile
{
    /// <summary>
    /// Reads the provided CSV file
    /// </summary>
    /// <param name="filepath">The path to the file</param>
    /// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
    /// <returns>The list of games in the CSV file</returns>
    Task<CsvGamesList> ReadFile(string filepath, bool getOtherValues);

    /// <summary>
    /// Writes the file to the specified target.
    /// </summary>
    /// <param name="entries">The entries to write.</param>
    /// <param name="target">The target to write to.</param>*
    Task WriteFile(CsvGamesList entries, string target);
}