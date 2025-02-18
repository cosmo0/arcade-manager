using ArcadeManager.Exceptions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ArcadeManager.Services;

/// <summary>
/// CSV files management
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Csv"/> class.
/// </remarks>
/// <param name="fs">The file system.</param>
public class Csv(IFileSystem fs) : ICsv {

    /// <summary>
    /// The default delimiter
    /// </summary>
    private static readonly string defaultDelimiter = ";";

    /// <summary>
    /// The accepted delimiters
    /// </summary>
    /// <remarks>Pipe is escaped because it'll be used in a regex</remarks>
    private static readonly string[] delimiters = { ";", ",", "\t", "\\|" };

    /// <summary>
    /// The header row of a DAT conversion
    /// </summary>
    private static readonly string headerDatRow = "name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof";

    /// <summary>
    /// The header row of a INI conversion
    /// </summary>
    private static readonly string headerIniRow = "name;value;";

    /// <summary>
    /// The "name" column name
    /// </summary>
    private static readonly string nameColumn = "name";

    /// <summary>
    /// Converts a DAT file.
    /// </summary>
    /// <param name="main">The main file.</param>
    /// <param name="target">The target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertDat(string main, string target, IMessageHandler messageHandler) {
        messageHandler.Init("DAT conversion");

        try {
            XmlReaderSettings settings = new() {
                DtdProcessing = DtdProcessing.Ignore
            };

            // input file
            await fs.ReadFileStream(main, async streamReader => {
                using var xmlReader = XmlReader.Create(streamReader, settings);

                // output file
                await fs.WriteFileStream(target, async outStreamWriter => {
                    // deserialize DAT file
                    var datFile = Serializer.DeserializeXml<Models.DatFile.Datafile>(xmlReader);

                    // output stream
                    await outStreamWriter.WriteLineAsync(headerDatRow);

                    int total = datFile.Entries.Count;
                    int i = 0;
                    foreach (var e in datFile.Entries) {
                        i++;
                        messageHandler.Progress($"Converting {e.Name}", total, i);

                        var sb = new StringBuilder();
                        sb.Append($"{e.Name}{defaultDelimiter}");
                        sb.Append($"{e.Description ?? "-"}{defaultDelimiter}");
                        sb.Append($"{e.Year ?? "-"}{defaultDelimiter}");
                        sb.Append($"{e.Manufacturer ?? "-"}{defaultDelimiter}");

                        sb.Append(string.IsNullOrEmpty(e.Cloneof) ? "NO" : "YES").Append(defaultDelimiter); // is_parent
                        sb.Append(e.Romof ?? "-").Append(defaultDelimiter); // romof
                        sb.Append(string.IsNullOrEmpty(e.Cloneof) ? "YES" : "NO").Append(defaultDelimiter); // is_clone
                        sb.Append(e.Cloneof ?? "-").Append(defaultDelimiter); // cloneof
                        sb.Append(e.Sampleof ?? "-").Append(defaultDelimiter); // sampleof

                        await outStreamWriter.WriteLineAsync(sb.ToString());
                    }

                    messageHandler.Done("DAT file converted", target);
                });
            });
        }
        catch (Exception ex) {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Converts a INI file to CSV
    /// </summary>
    /// <param name="main">The main file</param>
    /// <param name="target">The target folder to create files into</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertIni(string main, string target, IMessageHandler messageHandler) {
        messageHandler.Init("INI conversion");

        try {
            var data = new Dictionary<string, List<IniEntry>>();

            var fileSize = fs.FileSize(main);
            var fileName = fs.FileName(main);

            await fs.ReadFileStream(main, async source => {
                await ReadIniFile(source, data, fileSize, messageHandler);
            });

            // create a file for each non-empty section
            var i = 0;
            foreach (var entry in data.Where(d => d.Value.Count != 0)) {
                i++;

                // file name = sanitized section name, or source name if there's only one section
                var name = data.Count > 1 ? Sanitize(entry.Key.Trim("[]".ToCharArray())) : fileName.Replace(".ini", "");
                name += ".csv";

                var path = fs.PathJoin(target, name);

                // create increments to not overwrite existing conversions
                var j = 0;
                var finalPath = path;
                while (fs.FileExists(finalPath)) { finalPath = path.Replace(".csv", $" ({++j}).csv"); }
                path = finalPath;

                // progress from 50%
                messageHandler.Progress($"Creating file {name}", 100, 50 + (i / data.Count * 50));

                // write into file
                await fs.WriteFileStream(path, async output => {
                    await output.WriteLineAsync(headerIniRow);

                    foreach (var iniEntry in entry.Value) {
                        await output.WriteLineAsync($"{iniEntry.game}{defaultDelimiter}{iniEntry.value}{defaultDelimiter}");
                    }
                });
            }

            messageHandler.Done("INI file converted", target);
        }
        catch (Exception ex) {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Keeps files that are listed in both files
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task Keep(string main, string secondary, string target, IMessageHandler messageHandler) {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "Filter entries in a CSV files", (main, sec) => {
            var result = new CsvGamesList();

            // keep entries from the main file that also exist in the secondary
            foreach (var me in main.Games.Where(me => sec.Games.Any(se => se.Name == me.Name))) {
                result.Add(me);
            }

            return result;
        });
    }

    /// <summary>
    /// Lists the files in a folder to a CSV file.
    /// </summary>
    /// <param name="main">The main folder.</param>
    /// <param name="target">The target CSV file.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <exception cref="DirectoryNotFoundException">Unable to find the folder {main}</exception>
    public async Task ListFiles(string main, string target, IMessageHandler messageHandler) {
        messageHandler.Init("List files to a CSV");

        try {
            if (!fs.DirectoryExists(main)) { throw new PathNotFoundException($"Unable to find the folder {main}"); }

            await fs.WriteFileStream(target, async output => {
                await output.WriteLineAsync($"{nameColumn}{defaultDelimiter}");

                var files = fs.FilesGetList(main, "*.zip");
                files.AddRange(fs.FilesGetList(main, "*.7z"));
                var total = files.Count;
                var i = 0;

                foreach (var n in files.Select(f => fs.FileName(f))) {
                    i++;
                    messageHandler.Progress($"Listing file {n}", total, i);

                    var fileName = n.Replace(".zip", "", StringComparison.InvariantCultureIgnoreCase).Replace(".7z", "", StringComparison.InvariantCultureIgnoreCase);
                    await output.WriteLineAsync(fileName + defaultDelimiter);
                }
            });

            messageHandler.Done("Files listed", target);
        }
        catch (Exception ex) {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Merges the specified main and secondary files to the target file.
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task Merge(string main, string secondary, string target, IMessageHandler messageHandler) {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "Merge two CSV files", (main, sec) => {
            var result = new CsvGamesList(main.Games);

            foreach (var secg in sec.Games) {
                var maing = main.Games.FirstOrDefault(me => me.Name == secg.Name);
                if (maing == null) {
                    // entry is in secondary but not main file: copy to result
                    result.Add(secg);
                }
                else {
                    // entry is already in main: copy additional data
                    result.CopyEntry(secg);
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Reads the provided CSV file
    /// </summary>
    /// <param name="filepath">The path to the file</param>
    /// <param name="getOtherValues">if set to <c>true</c> get the values other than the name.</param>
    /// <returns>The list of games in the CSV file</returns>
    public async Task<CsvGamesList> ReadFile(string filepath, bool getOtherValues) {
        CsvGamesList result = new();

        await fs.ReadFileStream(filepath, async reader => {
            // check that the first line has a header
            var firstLine = await reader.ReadLineAsync();
            var (hasHeader, delimiter) = HasHeader(firstLine);

            // back to the beginning of the file
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            // build CSV read options
            var conf = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = delimiter,
                HasHeaderRecord = hasHeader,
                IgnoreBlankLines = true
            };

            // read the file
            using (var csv = new CsvReader(reader, conf)) {
                var entries = csv.GetRecordsAsync<dynamic>();

                if (hasHeader) {
                    await foreach (var e in entries) {
                        Dictionary<string, string> values = new();
                        if (getOtherValues) {
                            // get all other columns ; see visualstudiomagazine.com/articles/2019/04/01/working-with-dynamic-objects.aspx
                            values = ((IDictionary<string, object>)e)
                                .Where(elem => !elem.Key.Equals(nameColumn, StringComparison.InvariantCultureIgnoreCase))
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                        }

                        result.Add(e.name, values);
                    }
                }
                else {
                    // Without headers, CsvReader names the fields Field1, Field2, etc
                    result.AddRange((await entries.ToListAsync()).Select(e => new string(e.Field1)));
                }
            }
        });

        return result;
    }

    /// <summary>
    /// Removes entries in the main file that are in the secondary
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <returns></returns>
    public async Task Remove(string main, string secondary, string target, IMessageHandler messageHandler) {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "", (main, sec) => {
            var result = new CsvGamesList();

            foreach (var me in main.Games) {
                if (!sec.Games.Any(se => se.Name.Equals(me.Name, StringComparison.InvariantCultureIgnoreCase))) {
                    result.Add(me);
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Checks if the specified line has a header field
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <returns>Whether a delimiter has been found, and the delimiter, or the default one</returns>
    private static (bool hasDelimiter, string delimiter) HasHeader(string line) {
        bool hasDelimiter = false;

        // remove quotes, just to simplify this check
        line = line.Replace("\"", string.Empty);
        line = line.Replace("'", string.Empty);

        foreach (var d in delimiters) {
            // ",name," OR "name," OR ",name"
            var hasKeyword = new Regex($"{d}{nameColumn}{d}|^{nameColumn}{d}|{d}{nameColumn}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            // single column with just "name" and nothing else
            var hasKeywordAlone = new Regex($"^{d}$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (hasKeyword.IsMatch(line)) {
                return (true, d.Replace("\\", ""));
            }

            if (hasKeywordAlone.IsMatch(line)) {
                return (false, d.Replace("\\", ""));
            }

            hasDelimiter |= line.Contains(d.Replace("\\", ""), StringComparison.InvariantCultureIgnoreCase);
        }

        // no header column found, and no delimiter either
        if (!hasDelimiter) {
            return (false, delimiters[0]);
        }

        throw new FormatException("Your CSV file must have a 'name' column");
    }

    /// <summary>
    /// Sanitizes a file name
    /// </summary>
    /// <param name="name">The file name</param>
    /// <returns>The sanitized file name</returns>
    /// <remarks>Copied from stackoverflow.com/a/847251/6776</remarks>
    private string Sanitize(string name) {
        string invalidChars = Regex.Escape(new string(fs.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(name, invalidRegStr, "_");
    }

    /// <summary>
    /// Processes work on two file
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="init">The initialization label.</param>
    /// <param name="action">The action to process.</param>
    /// <returns></returns>
    private async Task WorkOnTwoFiles(string main, string secondary, string target, IMessageHandler messageHandler, string init, Func<CsvGamesList, CsvGamesList, CsvGamesList> action) {
        messageHandler.Init(init);

        try {
            var steps = 5;
            var current = 0;

            var fiMain = fs.FileName(main);
            var fiSecondary = fs.FileName(secondary);
            var fiTarget = fs.FileName(target);

            messageHandler.Progress($"reading file {fiMain}", steps, ++current);
            var mainEntries = await ReadFile(main, true);

            messageHandler.Progress($"reading file {fiSecondary}", steps, ++current);
            var secondaryEntries = await ReadFile(secondary, true);

            messageHandler.Progress($"action", steps, ++current);
            var result = action(mainEntries, secondaryEntries);

            messageHandler.Progress($"save to file {fiTarget}", steps, ++current);
            await WriteFile(result, target);

            messageHandler.Done($"{current}/{steps} - Done! Result has {result.Games.Count} entries", target);
        }
        catch (Exception ex) {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Writes the file to the specified target.
    /// </summary>
    /// <param name="entries">The entries to write.</param>
    /// <param name="target">The target to write to.</param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task WriteFile(CsvGamesList entries, string target) {
        await fs.WriteFileStream(target, async output => {
            // header line
            await output.WriteLineAsync(entries.GetHeaderLine(nameColumn, defaultDelimiter));

            // entries
            foreach (var entry in entries.Games) {
                await output.WriteLineAsync(entry.ToCSVString(defaultDelimiter));
            }
        });
    }

    private static async Task ReadIniFile(StreamReader source, Dictionary<string, List<IniEntry>> data, long fileSize, IMessageHandler messageHandler) {
        var isFolderSetting = false;
        var currentSection = "";

        while (!source.EndOfStream) {
            var line = (await source.ReadLineAsync()).Trim();

            // progress up to 50%
            messageHandler.Progress("Reading source file", 100, (int)(source.BaseStream.Position / fileSize * 50));

            // ignore empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) {
                continue;
            }

            // ignore folder settings
            if (line == "[FOLDER_SETTINGS]") {
                isFolderSetting = true;
                continue;
            }

            if (isFolderSetting && !line.StartsWith('[')) {
                continue;
            }

            // we're out of folder settings
            isFolderSetting = false;

            // found a section
            if (line.StartsWith('[')) {
                currentSection = ReadSection(line, data);

                continue;
            }

            // we're in a section data
            ReadLine(line, data, currentSection);
        }
    }

    private static string ReadSection(string line, Dictionary<string, List<IniEntry>> data) {
        // add to data
        if (!data.ContainsKey(line)) {
            data.Add(line, []);
        }

        return line;
    }

    private static void ReadLine(string line, Dictionary<string, List<IniEntry>> data, string currentSection) {
        if (line.Contains('=', StringComparison.InvariantCultureIgnoreCase)) {
            // game=value
            var split = line.Split("=", StringSplitOptions.TrimEntries);
            data[currentSection].Add(new IniEntry { game = split[0], value = split[1] });
        }
        else {
            // simple games list
            data[currentSection].Add(new IniEntry { game = line });
        }
    }

    /// <summary>
    /// Represents an INI file entry
    /// </summary>
    private struct IniEntry {

        /// <summary>
        /// The game name
        /// </summary>
        public string game;

        /// <summary>
        /// The additional value, if any
        /// </summary>
        public string value;
    }
}