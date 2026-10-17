using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Exceptions;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models;
using ArcadeManager.Core.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Services;

/// <summary>
/// CSV files management
/// </summary>
/// <seealso cref="ArcadeManager.Core.Services.Interfaces.ICsv" />
/// <remarks>
/// Initializes a new instance of the <see cref="Csv" /> class.
/// </remarks>
public class Csv(IFileSystem fs, ICsvFile csvFile, IIniFile iniFile, IDatFile datFile) : ICsv
{
    /// <summary>
    /// Converts a DAT file.
    /// </summary>
    /// <param name="main">The main file.</param>
    /// <param name="target">The target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertDat(string main, string target, IMessageHandler messageHandler)
    {
        messageHandler.ProgressInit("DAT conversion");

        try
        {
            // input
            using var streamReader = fs.GetReadFileStream(main);
            var dat = await datFile.ReadDatFile(streamReader);

            // output
            using var outStreamWriter = fs.GetWriteFileStream(target);
            await outStreamWriter.WriteLineAsync(Domain.CsvFile.HeaderDatRow);

            int total = dat.Entries.Count;
            int i = 0;
            foreach (var e in dat.Entries)
            {
                i++;
                messageHandler.Progress($"Converting {e.Name}", total, i);

                await outStreamWriter.WriteLineAsync(csvFile.SerializeDatEntry(e));
            }

            messageHandler.ProgressDone("DAT file converted", target);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Converts a INI file to CSV
    /// </summary>
    /// <param name="main">The main file</param>
    /// <param name="target">The target folder to create files into</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertIni(string main, string target, IMessageHandler messageHandler)
    {
        messageHandler.ProgressInit("INI conversion");

        try
        {
            var fileName = fs.FileName(main);

            using var stream = fs.GetReadFileStream(main);
            var data = await iniFile.ReadIniFile(stream);

            // create a file for each non-empty section
            var i = 0;
            foreach (var entry in data.Where(d => d.Value.Count != 0))
            {
                i++;

                // file name = sanitized section name, or source name if there's only one section
                var name = data.Count > 1 ? fs.Sanitize(entry.Key.Trim("[]".ToCharArray())) : fileName.Replace(".ini", "");
                name += ".csv";

                var path = fs.PathJoin(target, name);

                // create increments to not overwrite existing conversions
                var j = 0;
                var finalPath = path;
                while (fs.FileExists(finalPath)) { finalPath = path.Replace(".csv", $" ({++j}).csv"); }
                path = finalPath;

                // progress from 50%
                messageHandler.Progress($"Creating file {name}", 100, 50 + i / data.Count * 50);

                // write into file
                using var output = fs.GetWriteFileStream(path);

                await output.WriteLineAsync(Domain.CsvFile.HeaderIniRow);

                foreach (var iniEntry in entry.Value)
                {
                    await output.WriteLineAsync(csvFile.SerializeIniEntry(iniEntry));
                }
            }

            messageHandler.ProgressDone("INI file converted", target);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Keeps files that are listed in both files
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task Keep(string main, string secondary, string target, IMessageHandler messageHandler)
    {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "Filter entries in a CSV files", (main, sec) =>
        {
            var result = new CsvGamesList();

            // keep entries from the main file that also exist in the secondary
            foreach (var me in main.Games.Where(me => sec.Games.Any(se => se.Name == me.Name)))
            {
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
    public async Task ListFiles(string main, string target, IMessageHandler messageHandler)
    {
        messageHandler.ProgressInit("List files to a CSV");

        try
        {
            if (!fs.DirectoryExists(main)) { throw new PathNotFoundException($"Unable to find the folder {main}"); }

            using var output = fs.GetWriteFileStream(target);

            await output.WriteLineAsync(Domain.CsvFile.HeaderMinimal);

            var files = fs.FilesGetList(main, "*.zip").ToList();
            files.AddRange(fs.FilesGetList(main, "*.7z"));
            var total = files.Count;
            var i = 0;

            foreach (var n in files.Select(f => fs.FileName(f)))
            {
                i++;
                messageHandler.Progress($"Listing file {n}", total, i);

                var fileName = n.Replace(".zip", "", StringComparison.InvariantCultureIgnoreCase).Replace(".7z", "", StringComparison.InvariantCultureIgnoreCase);
                await output.WriteLineAsync(fileName);
            }

            messageHandler.ProgressDone("Files listed", target);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Merges the specified main and secondary files to the target file.
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task Merge(string main, string secondary, string target, IMessageHandler messageHandler)
    {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "Merge two CSV files", (main, sec) =>
        {
            var result = new CsvGamesList(main.Games);

            foreach (var secg in sec.Games)
            {
                var maing = main.Games.FirstOrDefault(me => me.Name == secg.Name);
                if (maing == null)
                {
                    // entry is in secondary but not main file: copy to result
                    result.Add(secg);
                }
                else
                {
                    // entry is already in main: copy additional data
                    result.CopyEntry(secg);
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Removes entries in the main file that are in the secondary
    /// </summary>
    /// <param name="main">The path to the main file.</param>
    /// <param name="secondary">The path to the secondary file.</param>
    /// <param name="target">The path to the target file.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <returns></returns>
    public async Task Remove(string main, string secondary, string target, IMessageHandler messageHandler)
    {
        await WorkOnTwoFiles(main, secondary, target, messageHandler, "", (main, sec) =>
        {
            var result = new CsvGamesList();

            foreach (var me in main.Games.Where(g => !sec.Games.Any(se => se.Name.Equals(g.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                result.Add(me);
            }

            return result;
        });
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
    private async Task WorkOnTwoFiles(string main, string secondary, string target, IMessageHandler messageHandler, string init, Func<CsvGamesList, CsvGamesList, CsvGamesList> action)
    {
        messageHandler.ProgressInit(init);

        try
        {
            var steps = 5;
            var current = 0;

            var fiMain = fs.FileName(main);
            var fiSecondary = fs.FileName(secondary);
            var fiTarget = fs.FileName(target);

            messageHandler.Progress($"reading file {fiMain}", steps, ++current);
            using var mainStream = fs.GetReadFileStream(main);
            var mainEntries = await csvFile.ReadFile(mainStream, true);

            messageHandler.Progress($"reading file {fiSecondary}", steps, ++current);
            using var secondStream = fs.GetReadFileStream(secondary);
            var secondaryEntries = await csvFile.ReadFile(secondStream, true);

            messageHandler.Progress($"action", steps, ++current);
            var result = action(mainEntries, secondaryEntries);

            messageHandler.Progress($"save to file {fiTarget}", steps, ++current);
            using var outStream = fs.GetWriteFileStream(target);
            await csvFile.WriteFile(outStream, result);

            messageHandler.ProgressDone($"{current}/{steps} - Done! Result has {result.Games.Count} entries", target);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }
}