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
    private readonly IDatFile datFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="Roms"/> class.
    /// </summary>
    /// <param name="csvService">The CSV service.</param>
    /// <param name="fs">The filesystem infrastructure.</param>
    public Roms(ICsv csvService, IFileSystem fs, IDatFile datFile)
    {
        this.csvService = csvService;
        this.fs = fs;
        this.datFile = datFile;

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
    public async Task CheckDat(RomsActionCheckDat args, IMessageHandler messageHandler)
    {
        messageHandler.Init("Checking a romset against a DAT file");

        if (!fs.DirectoryExists(args.romset))
        {
            messageHandler.Error(new DirectoryNotFoundException($"Folder {args.romset} not found"));
            return;
        }

        messageHandler.Progress("Reading DAT file", 0, 0);
        var filesInRomset = fs.GetFiles(args.romset, "*.zip");

        int total = filesInRomset.Count;
        int progress = 0;

        bool repair = args.action == "fix" || args.action == "change";
        bool change = args.action == "change";

        // total depends on chosen action (check=x1, repair=x2, rebuild=x3)
        if (repair)
        {
            total *= 3; // check, then list files in fix folder, then repair
        }
        else if (change)
        {
            total *= 4; // same + rebuild
        }

        try
        {
            // get the DAT file path
            string dat = args.datfilePre;
            if (dat == "custom")
            {
                dat = args.datfileCustom;
            }
            else
            {
                dat = fs.GetDataPath("mame-xml", dat, "games.xml");
            }

            // build a found files dataset
            var processed = new GameRomList();

            // read the csv file if it is sent
            CsvGamesList csv = null;
            if (!string.IsNullOrEmpty(args.csvfilter) && fs.FileExists(args.csvfilter))
            {
                csv = await csvService.ReadFile(args.csvfilter, false);
            }

            messageHandler.Progress("Reading DAT file", 0, 0);
            var games = await datFile.GetRoms(dat);

            // get the list of files in the repair folder
            var fixFolderFiles = new GameRomFilesList();
            List<string> fixFiles = [];
            if (!messageHandler.MustCancel && (repair || change))
            {
                messageHandler.Progress("Reading files in fix folder", 0, 0);
                fixFiles = fs.GetFiles(args.otherFolder, "*.zip");
                total += fixFiles.Count;
            }

            // process check
            foreach (var game in games.OrderBy(g => g.Name))
            {
                progress = CheckGame(total, progress, game, args, processed, messageHandler, csv);
                if (messageHandler.MustCancel) { break; }
            }

            // get the files details from the repair folder
            if (!messageHandler.MustCancel && (repair || change))
            {
                foreach (var ff in fixFiles)
                {
                    messageHandler.Progress($"Reading {fs.FileName(ff)} in additional folder", total, progress++);

                    // read infos of files in zip
                    fixFolderFiles.AddRange(fs.GetZipFiles(ff, args.isslow));

                    if (messageHandler.MustCancel) { break; }
                }
            }

            // if error fixing: loop on errors and try to find a file to fix it with
            if (!messageHandler.MustCancel && repair)
            {
                // if the other folder is empty, look in the other files of the romset
                if (string.IsNullOrEmpty(args.otherFolder)) {
                    args.otherFolder = args.romset;
                }

                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await FixGame(total, progress, game, args, processed, fixFolderFiles, messageHandler, csv);

                    if (messageHandler.MustCancel) { break; }
                }
            }

            // if romset type change: move/copy files around
            if (!messageHandler.MustCancel && change)
            {
                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await ChangeGame(total, progress, game, args, processed, fixFolderFiles, messageHandler, csv);

                    if (messageHandler.MustCancel) { break; }
                }
            }

            messageHandler.Done($"Checked {progress} roms", args.fixFolder);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Checks the specified game
    /// </summary>
    /// <param name="total">The total number of games to check</param>
    /// <param name="progress">The current progress</param>
    /// <param name="game">The game to check</param>
    /// <param name="args">The action arguments</param>
    /// <param name="processed">The list of already processed games</param>
    /// <param name="messageHandler">The message handler</param>
    /// <param name="csv">The CSV filter file</param>
    /// <returns>The new progress value</returns>
    public int CheckGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList processed, IMessageHandler messageHandler, CsvGamesList csv)
    {
        if (messageHandler.MustCancel) { return progress; }

        // build file path
        var gameFile = fs.PathJoin(args.romset, $"{game.Name}.zip");

        // check if a matching file is on the disk
        if (!fs.FileExists(gameFile))
        {
            if (args.actionReportAll)
            {
                // report all errors
                game.Error(ErrorReason.MissingFile, $"Missing rom {game.Name}.zip", $"{game.Name}.zip");
                processed.Add(game);
            }

            // then skip to next file
            return progress;
        }

        // only increment progress if the file exists on the disk, since the total is based on disk
        progress++;

        // if the CSV filter is set, check if the game is in the list
        if (csv != null && !csv.Games.Any(g => g.Name == game.Name))
        {
            return progress;
        }

        // progress only if we actually process the game
        messageHandler.Progress($"Checking {game.Name}", total, progress);

        // if we are here: we are always processing the game, and we do not have added it to the list yet
        processed.Add(game);

        if (messageHandler.MustCancel) { return progress; }

        // check the existence of matching bios
        var biosmatches = biosmatch.Where(bm => bm.Game == game.Name);
        if (args.otherBios && biosmatches.Any())
        {
            foreach (var bm in biosmatches.Where(bm => !fs.FileExists(fs.PathJoin(args.romset, $"{bm.Bios}.zip"))))
            {
                game.Error(ErrorReason.MissingFile, $"Missing BIOS {bm.Bios}.zip", $"{bm.Bios}.zip");
                break;
            }
        }

        if (messageHandler.MustCancel) { return progress; }

        // check the existence of matching device
        var devicematches = devicematch.Where(dm => dm.Game == game.Name);
        if (args.otherDevices && devicematches.Any())
        {
            foreach (var dm in devicematches.SelectMany(dm => dm.Devices).Where(dm => !fs.FileExists(fs.PathJoin(args.romset, $"{dm}.zip"))))
            {
                game.Error(ErrorReason.MissingFile, $"Missing device {dm}.zip", $"{dm}.zip");
                break;
            }
        }

        if (messageHandler.MustCancel) { return progress; }

        CheckFilesOfGame(game, gameFile, args.isslow, messageHandler);

        messageHandler.Processed(game);

        return progress;
    }

    private void CheckFilesOfGame(GameRom game, string filePath, bool checkSha1, IMessageHandler messageHandler)
    {
        // open the zip
        var zipFiles = fs.GetZipFiles(filePath, checkSha1);

        if (messageHandler.MustCancel) { return; }

        // check the files that are supposed to be in the game
        foreach (var datFile in game.RomFiles)
        {
            if (messageHandler.MustCancel) { return; }

            var zf = zipFiles.FirstOrDefault(zf => zf.Name.Equals(datFile.Name, StringComparison.InvariantCultureIgnoreCase));

            // ensure the file exists in the zip
            if (zf == null)
            {
                game.Error(ErrorReason.MissingFile, "Missing file in zip", datFile.Name);
                continue;
            }

            // check size and crc
            if (!zf.Crc.Equals(datFile.Crc, StringComparison.InvariantCultureIgnoreCase))
            {
                game.Error(ErrorReason.BadHash, $"Bad CRC - expected: {datFile.Crc}; actual: {zf.Crc}", datFile.Name);
                continue;
            }

            // if speed slow: check hash
            if (checkSha1 && !string.IsNullOrEmpty(datFile.Sha1) && !zf.Sha1.Equals(datFile.Sha1, StringComparison.InvariantCultureIgnoreCase))
            {
                game.Error(ErrorReason.BadHash, $"Bad SHA1 - expected: {datFile.Sha1}; actual: {zf.Sha1}", datFile.Name);
                continue;
            }
        }
    }

    /// <summary>
    /// Fixes the specified game
    /// </summary>
    /// <param name="total">The total number of games to check</param>
    /// <param name="progress">The current progress</param>
    /// <param name="game">The game to check</param>
    /// <param name="args">The action arguments</param>
    /// <param name="processed">The list of already processed games</param>
    /// <param name="fixFolder">The list of files in the fix folder</param>
    /// <param name="messageHandler">The message handler</param>
    /// <param name="csv">The CSV filter file</param>
    /// <returns>The new progress value</returns>
    public async Task<int> FixGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList processed, GameRomFilesList fixFolder, IMessageHandler messageHandler, CsvGamesList csv)
    {
        if (messageHandler.MustCancel) { return progress; }

        progress++;

        // no error in the files
        if (!game.HasError && !game.RomFiles.HasError)
        {
            return progress;
        }

        // build file path
        var gameFile = fs.PathJoin(args.romset, $"{game.Name}.zip");
        var gameFileFixed = fs.PathJoin(args.fixFolder, $"{game.Name}.zip");

        // if the CSV filter is set, check if the game is in the list
        if (csv != null && !csv.Games.Any(g => g.Name == game.Name))
        {
            return progress;
        }

        // progress only if we actually process the game
        messageHandler.Progress($"Fixing {game.Name}", total, progress);

        if (messageHandler.MustCancel) { return progress; }

        // TODO: check that either the parent exists or the current zip has all the parent files

        // TODO: check the bios in game.romof or parent's game.romof

        if (messageHandler.MustCancel) { return progress; }

        // check the existence of matching device
        var devicematches = devicematch.Where(dm => dm.Game == game.Name);
        if (args.otherDevices && devicematches.Any())
        {
            foreach (var dm in devicematches.SelectMany(dm => dm.Devices).Where(dm => !fs.FileExists(fs.PathJoin(args.romset, $"{dm}.zip"))))
            {
                var toCopy = fs.PathJoin(args.otherFolder, $"{dm}.zip");
                var copyTo = fs.PathJoin(args.fixFolder, $"{dm}.zip");
                if (fs.FileExists(toCopy) && !fs.FileExists(copyTo)) {
                    fs.FileCopy(toCopy, copyTo, false);
                }
            }
        }

        if (messageHandler.MustCancel) { return progress; }

        // the whole rom has not been found
        if (game.ErrorReason == ErrorReason.MissingFile)
        {
            // check if it exists in the fix folder
            var existing = fixFolder.FirstOrDefault(f => f.ZipFileName.Equals($"{game.Name}.zip", StringComparison.InvariantCultureIgnoreCase));

            if (existing != null && !fs.FileExists(gameFileFixed))
            {
                fs.FileCopy(gameFile, gameFileFixed, true);
            }

            // check that the files match the expected values; if not it will be rebuilt in the next step
            CheckFilesOfGame(game, gameFileFixed, args.isslow, messageHandler);
        }

        if (messageHandler.MustCancel) { return progress; }

        // check files with errors
        foreach (var romFile in game.RomFiles.Where(f => f.HasError))
        {
            // try to find the file in the already-processed files
            var fixFile = processed
                .SelectMany(r => r.RomFiles)
                .Where(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            // not found: try to find the file in the fix folder
            fixFile ??= fixFolder
                    .Where(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

            if (messageHandler.MustCancel) { return progress; }

            // file is found: fix the game
            if (fixFile != null)
            {
                // copy existing file, if it doesn't exist already
                if (!fs.FileExists(gameFileFixed))
                {
                    fs.FileCopy(gameFile, gameFileFixed, false);
                }

                await fs.ReplaceZipFile(fs.PathJoin(args.otherFolder, fixFile.ZipFileName), gameFileFixed, fixFile.Name);

                // update the metadata
                romFile.ErrorReason = ErrorReason.None;
                romFile.ErrorDetails = null;
            }
        }

        // re-check the rom
        CheckFilesOfGame(game, gameFileFixed, args.isslow, messageHandler);

        messageHandler.Fixed(game);

        return progress;
    }

    public async Task<int> ChangeGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList processed, GameRomFilesList fixFolder, IMessageHandler messageHandler, CsvGamesList csv)
    {


        return progress;
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