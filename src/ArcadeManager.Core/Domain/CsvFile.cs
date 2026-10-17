using ArcadeManager.Core.Models;
using ArcadeManager.Core.Models.DataFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

/// <summary>
/// Converter from a file to a CsvGamesList, and vice-versa
/// </summary>
/// <seealso cref="ArcadeManager.Core.Domain.ICsvFile" />
public class CsvFile : ICsvFile
{
    /// <summary>
    /// The default delimiter
    /// </summary>
    public static readonly string DefaultDelimiter = ";";

    /// <summary>
    /// The header row of a DAT conversion
    /// </summary>
    public static readonly string HeaderDatRow = "name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof";

    /// <summary>
    /// The header row of a INI conversion
    /// </summary>
    public static readonly string HeaderIniRow = "name;value;";

    /// <summary>
    /// The minimal header
    /// </summary>
    public static readonly string HeaderMinimal = $"name;";

    /// <summary>
    /// The "name" column name
    /// </summary>
    public static readonly string NameColumn = "name";

    /// <summary>
    /// The accepted delimiters
    /// </summary>
    /// <remarks>Pipe is escaped because it'll be used in a regex</remarks>
    private static readonly string[] delimiters = [";", ",", "\t", "\\|"];

    /// <summary>
    /// Reads the provided CSV file
    /// </summary>
    /// <param name="reader">The stream reader.</param>
    /// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
    /// <returns>
    /// The list of games in the CSV file
    /// </returns>
    public async Task<CsvGamesList> ReadFile(StreamReader reader, bool getOtherValues)
    {
        CsvGamesList result = new();

        // check that the first line has a header
        var firstLine = await reader.ReadLineAsync();
        var (hasHeader, delimiter) = HasHeader(firstLine);

        // back to the beginning of the file
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        // read the file
        var options = new Csv.CsvOptions
        {
            HeaderMode = hasHeader ? Csv.HeaderMode.HeaderPresent : Csv.HeaderMode.HeaderAbsent,
            Separator = delimiter[0]
        };
        var csvLines = Csv.CsvReader.ReadFromStreamAsync(reader.BaseStream, options);

        await foreach (var line in csvLines)
        {
            if (hasHeader)
            {
                Dictionary<string, string> values = [];
                if (getOtherValues)
                {
                    // get all other columns
                    values = line.Headers
                        // get the headers
                        .Where(h => h != NameColumn)
                        // get a dictionary <header, value>
                        .ToDictionary(h => h, h => line[h]);
                }

                result.Add(line["name"], values);
            }
            else
            {
                result.Add(line[0], []);
            }
        }

        return result;
    }

    /// <summary>
    /// Serializes the specified DAT file entry as a CSV line.
    /// </summary>
    /// <param name="entry">The DAT file entry to serialize.</param>
    /// <returns>
    /// The CSV line
    /// </returns>
    public string SerializeDatEntry(Entry entry)
    {
        var sb = new StringBuilder();
        sb.Append($"{entry.Name}{DefaultDelimiter}");
        sb.Append($"{entry.Description ?? "-"}{DefaultDelimiter}");
        sb.Append($"{entry.Year ?? "-"}{DefaultDelimiter}");
        sb.Append($"{entry.Manufacturer ?? "-"}{DefaultDelimiter}");

        sb.Append(string.IsNullOrEmpty(entry.Cloneof) ? "NO" : "YES").Append(DefaultDelimiter); // is_parent
        sb.Append(entry.Romof ?? "-").Append(DefaultDelimiter); // romof
        sb.Append(string.IsNullOrEmpty(entry.Cloneof) ? "YES" : "NO").Append(DefaultDelimiter); // is_clone
        sb.Append(entry.Cloneof ?? "-").Append(DefaultDelimiter); // cloneof
        sb.Append(entry.Sampleof ?? "-").Append(DefaultDelimiter); // sampleof

        return sb.ToString();
    }

    /// <summary>
    /// Serializes the specified ini entry as CSV line.
    /// </summary>
    /// <param name="entry">The ini entry.</param>
    /// <returns>The CSV line</returns>
    public string SerializeIniEntry(IniFile.IniEntry entry)
    {
        return $"{entry.Game}{DefaultDelimiter}{entry.Value}{DefaultDelimiter}";
    }

    /// <summary>
    /// Writes the file to the specified target.
    /// </summary>
    /// <param name="output">The output stream.</param>
    /// <param name="entries">The entries to write.</param>
    public async Task WriteFile(StreamWriter output, CsvGamesList entries)
    {
        // header line
        await output.WriteLineAsync(entries.GetHeaderLine(NameColumn, DefaultDelimiter));

        // entries
        foreach (var entry in entries.Games)
        {
            await output.WriteLineAsync(entry.ToCSVString(DefaultDelimiter));
        }
    }

    /// <summary>
    /// Checks if the specified line has a header field
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <returns>Whether a delimiter has been found, and the delimiter, or the default one</returns>
    private static (bool hasDelimiter, string delimiter) HasHeader(string line)
    {
        bool hasDelimiter = false;

        // remove quotes, just to simplify this check
        line = line.Replace("\"", string.Empty);
        line = line.Replace("'", string.Empty);

        foreach (var d in delimiters)
        {
            // ",name," OR "name," OR ",name"
            var hasKeyword = new Regex($"{d}{NameColumn}{d}|^{NameColumn}{d}|{d}{NameColumn}$", RegexOptions.Multiline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            // single column with just "name" and nothing else
            var hasKeywordAlone = new Regex($"^{d}$", RegexOptions.Multiline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

            if (hasKeyword.IsMatch(line))
            {
                return (true, d.Replace("\\", ""));
            }

            if (hasKeywordAlone.IsMatch(line))
            {
                return (false, d.Replace("\\", ""));
            }

            hasDelimiter |= line.Contains(d.Replace("\\", ""), StringComparison.InvariantCultureIgnoreCase);
        }

        // no header column found, and no delimiter either
        return (false, delimiters[0]);
    }
}