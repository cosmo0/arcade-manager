using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public interface IIniFile
{
    /// <summary>
    /// Reads an ini file and returns the entries.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <returns>
    /// The ini entries
    /// </returns>
    Task<Dictionary<string, List<IniFile.IniEntry>>> ReadIniFile(StreamReader source);
}