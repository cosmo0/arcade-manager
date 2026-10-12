using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

/// <summary>
/// Converter from a file to a CsvGamesList, and vice-versa
/// </summary>
/// <seealso cref="ArcadeManager.Core.Domain.ICsvFile" />
public class CsvFile(IFileSystem fs) : ICsvFile
{
    /// <summary>
    /// The default delimiter
    /// </summary>
    public static readonly string DefaultDelimiter = ";";

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
    /// <param name="filepath">The path to the file</param>
    /// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
    /// <returns>The list of games in the CSV file</returns>
    public async Task<CsvGamesList> ReadFile(string filepath, bool getOtherValues)
    {
        CsvGamesList result = new();

        using var reader = fs.GetReadFileStream(filepath);

        // check that the first line has a header
        var firstLine = await reader.ReadLineAsync();
        var (hasHeader, delimiter) = HasHeader(firstLine);

        // back to the beginning of the file
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        // build CSV read options
        var conf = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = hasHeader,
            IgnoreBlankLines = true
        };

        // read the file
        using var csv = new CsvReader(reader, conf);
        var entries = csv.GetRecordsAsync<dynamic>();

        if (hasHeader)
        {
            await foreach (var e in entries)
            {
                Dictionary<string, string> values = [];
                if (getOtherValues)
                {
                    // get all other columns ; see visualstudiomagazine.com/articles/2019/04/01/working-with-dynamic-objects.aspx
                    values = ((IDictionary<string, object>)e)
                        .Where(elem => !elem.Key.Equals(NameColumn, StringComparison.InvariantCultureIgnoreCase))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                }

                result.Add(e.name, values);
            }
        }
        else
        {
            // Without headers, CsvReader names the fields Field1, Field2, etc
            result.AddRange((await entries.ToListAsync()).Select(e => new string(e.Field1)));
        }

        return result;
    }

    /// <summary>
    /// Writes the file to the specified target.
    /// </summary>
    /// <param name="entries">The entries to write.</param>
    /// <param name="target">The target to write to.</param>
    public async Task WriteFile(CsvGamesList entries, string target)
    {
        using var output = fs.GetWriteFileStream(target);

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
        if (!hasDelimiter)
        {
            return (false, delimiters[0]);
        }

        throw new FormatException("Your CSV file must have a 'name' column");
    }
}