using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public class IniFile : IIniFile
{
    /// <summary>
    /// Represents an INI file entry
    /// </summary>
    public record IniEntry(string Game, string Value);

    /// <summary>
    /// Reads an ini file and returns the entries.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <returns>
    /// The ini entries
    /// </returns>
    public async Task<Dictionary<string, List<IniEntry>>> ReadIniFile(StreamReader source)
    {
        var isFolderSetting = false;
        var currentSection = "";
        string line;

        Dictionary<string, List<IniEntry>> data = [];

        while ((line = await source.ReadLineAsync()) is not null)
        {
            line = line.Trim();

            // ignore empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';'))
            {
                continue;
            }

            // ignore folder settings
            if (line == "[FOLDER_SETTINGS]")
            {
                isFolderSetting = true;
                continue;
            }

            if (isFolderSetting && !line.StartsWith('['))
            {
                continue;
            }

            // we're out of folder settings
            isFolderSetting = false;

            // found a section
            if (line.StartsWith('['))
            {
                currentSection = ReadSection(line, data);

                continue;
            }

            // we're in a section data
            ReadLine(line, data, currentSection);
        }

        return data;
    }

    private static void ReadLine(string line, Dictionary<string, List<IniEntry>> data, string currentSection)
    {
        if (line.Contains('=', StringComparison.InvariantCultureIgnoreCase))
        {
            // game=value
            var split = line.Split("=", StringSplitOptions.TrimEntries);
            data[currentSection].Add(new IniEntry(split[0], split[1]));
        }
        else
        {
            // simple games list
            data[currentSection].Add(new IniEntry(line, null));
        }
    }

    private static string ReadSection(string line, Dictionary<string, List<IniEntry>> data)
    {
        line = line.Trim('[', ']');

        // add to data
        if (!data.ContainsKey(line))
        {
            data.Add(line, []);
        }

        return line;
    }
}