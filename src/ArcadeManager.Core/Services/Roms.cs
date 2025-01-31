using ArcadeManager.Actions;
using ArcadeManager.Exceptions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            if (!fs.FileExists(args.main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.main}"); }
            if (!fs.DirectoryExists(args.romset)) { throw new PathNotFoundException($"Unable to find romset folder {args.romset}"); }
            if (!fs.DirectoryExists(args.selection)) { fs.CreateDirectory(args.selection); }

            // read CSV file
            var content = await csvService.ReadFile(args.main, false);

            // copy the games
            var copied = CopyGamesList(args, content, messageHandler);

            messageHandler.Done($"Copied {copied} file(s)", args.selection);
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
        string[] emulatorFiles = args.main.Split("/", StringSplitOptions.RemoveEmptyEntries);
        string emulator = emulatorFiles[0];
        string[] files = emulatorFiles[1].Split(",", StringSplitOptions.RemoveEmptyEntries);

        messageHandler.Init("Copying roms");

        try
        {
            // check files and folders
            if (!fs.DirectoryExists(args.romset)) { throw new PathNotFoundException($"Unable to find romset folder {args.romset}"); }
            if (!fs.DirectoryExists(args.selection)) { fs.CreateDirectory(args.selection); }

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

            messageHandler.Done($"Copied {copied} file(s)", args.selection);
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
            if (!fs.FileExists(args.main)) { throw new PathNotFoundException($"Unable to find CSV file {args.main}"); }
            if (!fs.DirectoryExists(args.romset)) { throw new PathNotFoundException($"Unable to find folder {args.romset}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.main, false);

            // check that all files in the CSV are listed in the folder
            var result = new List<string>();
            foreach (var game in content.Games)
            {
                var gamePath = fs.PathJoin(args.romset, $"{game.Name}.zip");
                if (!fs.FileExists(gamePath))
                {
                    result.Add(game.Name);
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
            if (!fs.FileExists(args.main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.main}"); }
            if (!fs.DirectoryExists(args.selection)) { throw new PathNotFoundException($"Unable to find selection folder {args.selection}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.main, false);

            var total = content.Games.Count;
            var i = 0;
            var deleted = 0;

            foreach (var game in content.Games.Select(g => g.Name))
            {
                if (messageHandler.MustCancel) { break; }
                i++;

                // build vars
                var zip = $"{game}.zip";
                var filePath = fs.PathJoin(args.selection, zip);

                messageHandler.Progress(game, total, i);

                // check that source rom exists
                if (!fs.FileExists(filePath))
                {
                    zip = $"{game}.7z";
                    filePath = fs.PathJoin(args.romset, zip);
                }

                // still not found: next
                if (!fs.FileExists(filePath))
                {
                    continue;
                }

                fs.FileDelete(filePath);
                deleted++;
            }

            messageHandler.Done($"Deleted {deleted} file(s)", args.selection);
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
            if (!fs.FileExists(args.main)) { throw new PathNotFoundException($"Unable to find main CSV file {args.main}"); }
            if (!fs.DirectoryExists(args.selection)) { throw new PathNotFoundException($"Unable to find selection folder {args.selection}"); }

            // read CSV file
            var content = await csvService.ReadFile(args.main, false);

            // get list of files
            var files = fs.GetFiles(args.selection, "*.zip");
            files.AddRange(fs.GetFiles(args.selection, "*.7z"));

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

            messageHandler.Done($"Deleted {deleted} files", args.selection);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Checks a romset against a DAT file
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler</param>
    public async Task CheckDat(RomsActionCheckDat args, IMessageHandler messageHandler) {
        messageHandler.Init("Checking a romset against a DAT file");

        if (!fs.DirectoryExists(args.romset)) {
            messageHandler.Error(new DirectoryNotFoundException($"Folder {args.romset} not found"));
            return;
        }

        messageHandler.Progress("Reading DAT file", 0, 0);
        var filesInRomset = fs.GetFiles(args.romset, "*.zip");

        int total = filesInRomset.Count;
        int progress = 0;

        bool repair = args.action == "fix" || args.action == "change";
        bool change = args.action == "change";
        bool isSlow = args.speed == "slow";

        // total depends on chosen action (check=x1, repair=x2, rebuild=x3)
        if (repair) {
            total *= 2; // check then repair
        } else if (change) {
            total *= 3; // check then repair then rebuild
        }

        try
        {
            // get the DAT file path
            string dat = args.datfilePre;
            if (dat == "custom") {
                dat = args.datfileCustom;
            } else {
                dat = fs.GetDataPath("mame-xml", dat, "games.xml");
            }

            // build a found files dataset
            var processed = new List<GameRom>();

            // build an errors list
            var errors = new List<GameError>();
            
            // read the csv file if it is sent
            CsvGamesList csv = null;
            if (!string.IsNullOrEmpty(args.csvfilter) && fs.FileExists(args.csvfilter)) {
                csv = await csvService.ReadFile(args.csvfilter, false);
            }

            await fs.ReadFileStream(dat, async (datStream) => {
                messageHandler.Progress("Reading DAT file", 0, 0);

                // read DAT file and detect the tags names
                XDocument doc = await XDocument.LoadAsync(datStream, LoadOptions.None, new CancellationToken());
                if (messageHandler.MustCancel) { return; }
                string gameTag = doc.Root.Elements().Skip(1).First().Name.LocalName; // ensure we skip the header, if any

                // for each game in the dat file
                foreach (var gameXml in doc.Root.Elements(gameTag)) {
                    progress++;

                    // parse game infos
                    var game = GameRom.FromXml(gameXml);

                    if (messageHandler.MustCancel) { return; }

                    messageHandler.Progress(game.Name, total, progress);

                    // if the CSV filter is set, check if the game is in the list
                    if (csv != null && !csv.Games.Any(g => g.Name == game.Name)) {
                        continue;
                    }

                    // build file path
                    var gameFile = fs.PathJoin(args.romset, $"{game.Name}.zip");

                    // check if a matching file is on the disk
                    if (!fs.FileExists(gameFile)) {
                        if (args.actionReportAll) {
                            // report all errors
                            errors.Add(GameError.Missing(game.Name, $"{game.Name}.zip"));
                        }

                        // then skip to next file
                        continue;
                    }

                    if (messageHandler.MustCancel) { return; }

                    // check the existence of matching bios
                    var biosmatches = biosmatch.Where(bm => bm.Game == game.Name);
                    if (args.otherBios && biosmatches.Any()) {
                        foreach (var bm in biosmatches.Where(bm => !fs.FileExists(fs.PathJoin(args.romset, $"{bm.Bios}.zip")))) {
                            errors.Add(GameError.Missing(game.Name, $"{bm.Bios}.zip"));
                        }
                    }

                    if (messageHandler.MustCancel) { return; }

                    // check the existence of matching device
                    var devicematches = devicematch.Where(dm => dm.Game == game.Name);
                    if (args.otherDevices && devicematches.Any()) {
                        foreach (var dm in devicematches.SelectMany(dm => dm.Devices).Where(dm => !fs.FileExists(fs.PathJoin(args.romset, $"{dm}.zip")))) {
                            errors.Add(GameError.Missing(game.Name, $"{dm}.zip"));
                        }
                    }

                    if (messageHandler.MustCancel) { return; }

                    // open the zip
                    var zipFiles = fs.GetZipFiles(gameFile, isSlow);

                    var hasError = false;

                    // check the files that are supposed to be in the game
                    foreach (var datFile in game.RomFilesFromDat) {
                        if (messageHandler.MustCancel) { return; }

                        var zf = zipFiles.FirstOrDefault(zf => zf.Name.Equals(datFile.Name, StringComparison.InvariantCultureIgnoreCase));

                        // ensure the file exists in the zip
                        if (zf == null) {
                            errors.Add(GameError.Missing(game.Name, datFile.Name));
                            hasError = true;
                            continue;
                        }

                        // check size and crc
                        if (!zf.Crc.Equals(datFile.Crc, StringComparison.InvariantCultureIgnoreCase)) {
                            errors.Add(GameError.BadHash(game.Name, datFile.Name, $"Expected: {datFile.Crc}; actual: {zf.Crc}"));
                            hasError = true;
                            continue;
                        }

                        // if speed slow: check hash
                        if (isSlow && !string.IsNullOrEmpty(datFile.Sha1) && !zf.Sha1.Equals(datFile.Sha1, StringComparison.InvariantCultureIgnoreCase)) {
                            errors.Add(GameError.BadHash(game.Name, datFile.Name, $"Expected: {datFile.Sha1}; actual: {zf.Sha1}"));
                            hasError = true;
                            continue;
                        }
                    }
                    
                    // add to the games list
                    if (!hasError) {
                        processed.Add(game);
                    }
                }
            });

            if (!messageHandler.MustCancel && repair) {
                // if error fixing: loop on errors and try to find a file to fix it with
            }

            if (!messageHandler.MustCancel && change) {
                // if romset type change: move/copy files around
            }

            messageHandler.SetProcessed(processed);
            messageHandler.SetErrors(errors);
            messageHandler.Done($"Checked {progress} files", args.fixFolder);
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
            var sourceRom = fs.PathJoin(args.romset, zip);
            var ext = "zip";

            // always display progress
            messageHandler.Progress(game, total, i);

            // check that source rom exists
            if (!fs.FileExists(sourceRom))
            {
                zip = $"{game}.7z";
                sourceRom = fs.PathJoin(args.romset, zip);
                ext = "7z";
            }

            // still not found: next
            if (!fs.FileExists(sourceRom))
            {
                continue;
            }

            var destRom = fs.PathJoin(args.selection, zip);
            var fileSize = fs.FileSize(sourceRom);

            // replace progress with file size (so the user knows when a file is large)
            messageHandler.Progress($"{game} ({fs.HumanSize(fileSize)})", total, i);

            // copy rom
            if (!fs.FileExists(destRom) || args.overwrite)
            {
                fs.FileCopy(sourceRom, destRom, true);
                copied++;
            }

            // try to copy additional files for games if it's used and can be found
            List<string> additionalFiles = [.. this.biosmatch.GetBiosesForGame(game), .. this.devicematch.GetDevicesForGame(game)];
            foreach (var af in additionalFiles)
            {
                var sourceaf = fs.PathJoin(args.romset, $"{af}.{ext}");
                var destaf = fs.PathJoin(args.selection, $"{af}.{ext}");
                // never overwrite
                if (fs.FileExists(sourceaf) && !fs.FileExists(destaf))
                {
                    fs.FileCopy(sourceaf, destaf, false);
                    copied++;
                }
            }

            // try to copy chd if it can be found
            var sourceChd = fs.PathJoin(args.romset, game);
            var targetChd = fs.PathJoin(args.selection, game);
            if (fs.DirectoryExists(sourceChd))
            {
                if (messageHandler.MustCancel) { break; }

                messageHandler.Progress($"Copying {game} CHD ({fs.HumanSize(fs.DirectorySize(sourceChd))})", total, i);

                copied += fs.DirectoryCopy(sourceChd, targetChd, args.overwrite, false);
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