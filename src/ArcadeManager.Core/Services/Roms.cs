using System.Runtime.InteropServices;
using ArcadeManager.Actions;
using ArcadeManager.Exceptions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;

namespace ArcadeManager.Services;

/// <summary>
/// Roms management
/// </summary>
public class Roms : IRoms
{
    private readonly List<string> bioslist;
    private readonly BiosMatchList biosmatch = new();
    private readonly DeviceMatchList devicematch = new();
    private readonly ICsv csvService;
    private readonly IFileSystem fs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Roms"/> class.
    /// </summary>
    /// <param name="csvService">The CSV service.</param>
    /// <param name="fs">The filesystem infrastructure.</param>
    public Roms(ICsv csvService, IFileSystem fs)
    {
        this.csvService = csvService;
        this.fs = fs;

        this.bioslist = [.. fs.ReadAllLines(fs.GetDataPath("bioslist.txt"))];
        this.biosmatch.AddRange(
            fs.ReadAllLines(fs.GetDataPath("biosmatch.csv"))
                .Select(l => l.Split(";".ToCharArray()))
                .Select(l => new BiosMatch(l[0], l[1], l[2]))
        );

        this.devicematch.AddRange(
            fs.ReadAllLines(fs.GetDataPath("devices.csv"))
                .Select(l => l.Split(";".ToCharArray()))
                .Select(l => new DeviceMatch(l[0], [.. l.Skip(1)]))
        );
    }

    /// <summary>
    /// Copies roms
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
    /// <exception cref="DirectoryNotFoundException">Unable to find romset folder {args.romset}</exception>
    public async Task Add(Actions.RomsAction args, IMessageHandler messageHandler)
    {
        messageHandler.Init("Copying roms");

        try
        {
            // check files and folders
            if (!fs.FileExists(args.Main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.Main}"); }
            if (!fs.DirectoryExists(args.Romset)) { throw new PathNotFoundException($"Unable to find romset folder {args.Romset}"); }
            if (!fs.DirectoryExists(args.Selection)) { fs.CreateDirectory(args.Selection); }

            // read CSV file
            var content = await csvService.ReadFile(args.Main, false);

            // copy the games
            var copied = CopyGamesList(args, content, messageHandler);

            messageHandler.Done($"Copied {copied} file(s)", args.Selection);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Copies roms from a folder to another, from the wizard
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task AddFromWizard(RomsAction args, IMessageHandler messageHandler)
    {
        // the csv is just a list name, we need to match it to the selected path
        string[] emulatorFiles = args.Main.Split("/", StringSplitOptions.RemoveEmptyEntries);
        string emulator = emulatorFiles[0];
        string[] files = emulatorFiles[1].Split(",", StringSplitOptions.RemoveEmptyEntries);

        messageHandler.Init("Copying roms");

        try
        {
            // check files and folders
            if (!fs.DirectoryExists(args.Romset)) { throw new PathNotFoundException($"Unable to find romset folder {args.Romset}"); }
            if (!fs.DirectoryExists(args.Selection)) { fs.CreateDirectory(args.Selection); }

            // read CSV files
            Models.CsvGamesList content = new();
            foreach (var file in files)
            {
                string filepath = fs.GetDataPath("csv", emulator, $"{file}.csv");
                content.AddRange(await csvService.ReadFile(filepath, false));
            }

            content.DeDuplicate();

            // copy the games
            var copied = CopyGamesList(args, content, messageHandler);

            messageHandler.Done($"Copied {copied} file(s)", args.Selection);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Checks a list of roms
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler</param>
    /// <returns>The missing files list</returns>
    public async Task<string[]> Check(RomsAction args, IMessageHandler messageHandler)
    {
        try
        {
            // check files and folders
            if (!fs.FileExists(args.Main)) { throw new PathNotFoundException($"Unable to find CSV file {args.Main}"); }
            if (!fs.DirectoryExists(args.Romset)) { throw new PathNotFoundException($"Unable to find folder {args.Romset}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.Main, false);

            // check that all files in the CSV are listed in the folder
            var result = new List<string>();
            foreach (var gameName in content.Games.Select(g => g.Name))
            {
                var gamePath = fs.PathJoin(args.Romset, $"{gameName}.zip");
                if (!fs.FileExists(gamePath))
                {
                    result.Add(gameName);
                }
            }

            return [.. result];
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }

        return [];
    }

    /// <summary>
    /// Deletes roms from a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
    /// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
    public async Task Delete(Actions.RomsAction args, IMessageHandler messageHandler)
    {
        messageHandler.Init("Deleting roms");

        try
        {
            // check files and folders
            if (!fs.FileExists(args.Main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.Main}"); }
            if (!fs.DirectoryExists(args.Selection)) { throw new PathNotFoundException($"Unable to find selection folder {args.Selection}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.Main, false);

            var total = content.Games.Count;
            var i = 0;
            var deleted = 0;

            foreach (var game in content.Games.Select(g => g.Name))
            {
                if (messageHandler.MustCancel) { break; }
                i++;

                // build vars
                var zip = $"{game}.zip";
                var filePath = fs.PathJoin(args.Selection, zip);

                messageHandler.Progress(game, total, i);

                // check that source rom exists
                if (!fs.FileExists(filePath))
                {
                    zip = $"{game}.7z";
                    filePath = fs.PathJoin(args.Romset, zip);
                }

                // still not found: next
                if (!fs.FileExists(filePath))
                {
                    continue;
                }

                fs.FileDelete(filePath);
                deleted++;
            }

            messageHandler.Done($"Deleted {deleted} file(s)", args.Selection);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Keeps only listed roms in a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <exception cref="FileNotFoundException">Unable to find main CSV file</exception>
    /// <exception cref="DirectoryNotFoundException">Unable to find selection folder {args.selection}</exception>
    public async Task Keep(Actions.RomsAction args, IMessageHandler messageHandler)
    {
        messageHandler.Init("Filtering roms");

        try
        {
            // check files and folders
            if (!fs.FileExists(args.Main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.Main}"); }
            if (!fs.DirectoryExists(args.Selection)) { throw new PathNotFoundException($"Unable to find selection folder {args.Selection}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.Main, false);

            // get list of files
            var files = fs.GetFiles(args.Selection, "*.zip");
            files.AddRange(fs.GetFiles(args.Selection, "*.7z"));

            var total = content.Games.Count;
            var i = 0;
            var deleted = 0;

            // check if files exist in games list
            foreach (var f in files)
            {
                if (messageHandler.MustCancel) { break; }
                i++;

                var nameNoExt = fs.FileNameWithoutExtension(f);

                messageHandler.Progress(nameNoExt, total, i);

                // don't auto-delete bios files
                if (this.bioslist.Contains(nameNoExt, StringComparer.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                // delete if it's not found in the provided list
                if (!content.Games.Any(c => c.Name.Equals(nameNoExt, StringComparison.InvariantCultureIgnoreCase)))
                {
                    fs.FileDelete(f);
                    deleted++;
                }
            }

            messageHandler.Done($"Deleted {deleted} files", args.Selection);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    private int CopyGamesList(Actions.RomsAction args, Models.CsvGamesList content, IMessageHandler messageHandler)
    {
        var total = content.Games.Count;
        var i = 0;
        var copied = 0;

        // copy each file found in CSV
        foreach (var game in content.Games.Select(g => g.Name))
        {
            if (messageHandler.MustCancel) { break; }

            i++;

            // build vars
            var zip = $"{game}.zip";
            var sourceRom = fs.PathJoin(args.Romset, zip);
            var ext = "zip";

            // always display progress
            messageHandler.Progress(game, total, i);

            // check that source rom exists
            if (!fs.FileExists(sourceRom))
            {
                zip = $"{game}.7z";
                sourceRom = fs.PathJoin(args.Romset, zip);
                ext = "7z";
            }

            // still not found: next
            if (!fs.FileExists(sourceRom))
            {
                continue;
            }

            var destRom = fs.PathJoin(args.Selection, zip);
            var fileSize = fs.FileSize(sourceRom);

            // replace progress with file size (so the user knows when a file is large)
            messageHandler.Progress($"{game} ({fs.HumanSize(fileSize)})", total, i);

            // copy rom
            if (!fs.FileExists(destRom) || args.Overwrite)
            {
                fs.FileCopy(sourceRom, destRom, true);
                copied++;
            }

            // try to copy additional files for games if it's used and can be found
            List<string> additionalFiles = [.. this.biosmatch.GetBiosesForGame(game), .. this.devicematch.GetDevicesForGame(game)];
            foreach (var af in additionalFiles)
            {
                var sourceaf = fs.PathJoin(args.Romset, $"{af}.{ext}");
                var destaf = fs.PathJoin(args.Selection, $"{af}.{ext}");
                // never overwrite
                if (fs.FileExists(sourceaf) && !fs.FileExists(destaf))
                {
                    fs.FileCopy(sourceaf, destaf, false);
                    copied++;
                }
            }

            // try to copy chd if it can be found
            var sourceChd = fs.PathJoin(args.Romset, game);
            var targetChd = fs.PathJoin(args.Selection, game);
            if (fs.DirectoryExists(sourceChd))
            {
                if (messageHandler.MustCancel) { break; }

                messageHandler.Progress($"Copying {game} CHD ({fs.HumanSize(fs.DirectorySize(sourceChd))})", total, i);

                copied += fs.DirectoryCopy(sourceChd, targetChd, args.Overwrite, false);
            }
        }

        return copied;
    }

    /// <summary>
    /// A game/bios match
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BiosMatch"/> class.
    /// </remarks>
    /// <param name="game">The game.</param>
    /// <param name="bios">The bios.</param>
    /// <param name="clone">The clone.</param>
    private sealed class BiosMatch(string game, string bios, string clone)
    {
        /// <summary>
        /// Gets or sets the bios name.
        /// </summary>
        public string Bios { get; set; } = bios;

        /// <summary>
        /// Gets or sets the clone.
        /// </summary>
        public string Clone { get; set; } = clone;

        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Game { get; set; } = game;
    }

    /// <summary>
    /// A game/bios matches list
    /// </summary>
    private sealed class BiosMatchList : List<BiosMatch>
    {
        /// <summary>
        /// Gets the bioses for a game (it might change between MAME/FBNeo versions).
        /// </summary>
        /// <param name="game">The game name.</param>
        /// <returns>The potential bioses names</returns>
        public IEnumerable<string> GetBiosesForGame(string game)
        {
            var result = new List<string>();
            foreach (var item in this.Where(bm => bm.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase)))
            {
                result.Add(item.Bios);
            }

            return result;
        }
    }

    /// <summary>
    /// A game/devices match
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="devices">The devices names</param>
    private sealed class DeviceMatch(string game, string[] devices)
    {
        public string Game { get; set; } = game;
        public string[] Devices { get; set; } = devices;
    }

    /// <summary>
    /// A game/device matches list
    /// </summary>
    private sealed class DeviceMatchList : List<DeviceMatch>
    {
        public IEnumerable<string> GetDevicesForGame(string game)
        {
            var match = this.FirstOrDefault(dm => dm.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase));
            if (match != null)
            {
                return match.Devices;
            }

            return [];
        }
    }
}