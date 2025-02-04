using System;
using ArcadeManager.Actions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using ArcadeManager.Services;

namespace ArcadeManager.Services;

public class DatChecker(IFileSystem fs, ICsv csvService, IDatFile datFile) : IDatChecker
{
    
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

        // if the other folder is empty, look in the other files of the romset
        if (string.IsNullOrEmpty(args.otherFolder)) {
            args.otherFolder = args.romset;
        }

        bool repair = args.action == "fix" || args.action == "change";
        bool change = args.action == "change";

        int progress = 0;

        try
        {
            // get the DAT file path
            string dat = GetDatPath(args.datfilePre, args.datfileCustom);

            // build a found files dataset
            var processed = new GameRomList();

            // read the csv file if it is sent
            CsvGamesList csv = await GetCsvData(args.csvfilter);

            messageHandler.Progress("Reading DAT file", 0, 0);
            var games = await datFile.GetRoms(dat);

            // get the list of files in the repair folder
            List<string> fixFiles = GetFixFolderFiles(args.otherFolder, repair, change, messageHandler);
            int total = ComputeTotal(filesInRomset.Count, repair, change, fixFiles.Count);

            // process check
            foreach (var game in games.OrderBy(g => g.Name))
            {
                progress = CheckGame(total, progress, game, args, processed, games, messageHandler, csv);
                if (messageHandler.MustCancel) { break; }
            }

            // get the files details from the repair folder
            (var fixFolderFiles, progress) = GetFixFolderFilesDetails(total, progress, fixFiles, args.isslow, repair || change, messageHandler);

            // if error fixing: loop on errors and try to find a file to fix it with
            if (!messageHandler.MustCancel && repair)
            {
                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await FixGame(total, progress, game, args, games, fixFolderFiles, messageHandler, csv);

                    if (messageHandler.MustCancel) { break; }
                }
            }

            // if romset type change: move/copy files around
            if (!messageHandler.MustCancel && change)
            {
                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await ChangeGame(total, progress, game, args, games, fixFolderFiles, messageHandler, csv);

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
    /// <param name="allGames">The list of all games in the DAT</param>
    /// <param name="messageHandler">The message handler</param>
    /// <param name="csv">The CSV filter file</param>
    /// <returns>The new progress value</returns>
    public int CheckGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList processed, GameRomList allGames, IMessageHandler messageHandler, CsvGamesList csv)
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

        CheckFilesOfGame(game, gameFile, args.isslow, messageHandler);

        messageHandler.Processed(game);

        return progress;
    }

    public void CheckFilesOfGame(GameRom game, string filePath, bool checkSha1, IMessageHandler messageHandler)
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

            // TODO: check clones that are located inside the zip in case of merged set

            // TODO: check the bios
        }
    }

    /// <summary>
    /// Fixes the specified game
    /// </summary>
    /// <param name="total">The total number of games to check</param>
    /// <param name="progress">The current progress</param>
    /// <param name="game">The game to check</param>
    /// <param name="args">The action arguments</param>
    /// <param name="allGames">The list of all games in the DAT</param>
    /// <param name="fixFolderFiles">The list of files in the fix folder</param>
    /// <param name="messageHandler">The message handler</param>
    /// <param name="csv">The CSV filter file</param>
    /// <returns>The new progress value</returns>
    public async Task<int> FixGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList allGames, GameRomFilesList fixFolderFiles, IMessageHandler messageHandler, CsvGamesList csv)
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

        // the whole rom has not been found
        if (game.ErrorReason == ErrorReason.MissingFile)
        {
            // check if it exists in the fix folder
            var existing = fixFolderFiles.FirstOrDefault(f => f.ZipFileName.Equals($"{game.Name}.zip", StringComparison.InvariantCultureIgnoreCase));

            if (existing != null && !fs.FileExists(gameFileFixed))
            {
                fs.FileCopy(gameFile, gameFileFixed, true);
            }

            // check that the files match the expected values; if not it will be rebuilt in the next step
            CheckFilesOfGame(game, gameFileFixed, args.isslow, messageHandler);
        }

        if (messageHandler.MustCancel) { return progress; }

        // check files with errors
        await FixGameFiles(game, allGames, args.otherFolder, fixFolderFiles, gameFile, gameFileFixed, messageHandler);

        if (messageHandler.MustCancel) { return progress; }

        // re-check the rom
        CheckFilesOfGame(game, gameFileFixed, args.isslow, messageHandler);

        messageHandler.Fixed(game);

        return progress;
    }

    public async Task FixGameFiles(GameRom game, GameRomList allGames, string otherFolder, GameRomFilesList otherFolderFiles, string gameFile, string gameFileFixed, IMessageHandler messageHandler) {
        foreach (var romFile in game.RomFiles.Where(f => f.HasError))
        {
            // try to find the file in the already-processed files
            var fixFile = allGames
                .SelectMany(r => r.RomFiles)
                .Where(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            // not found: try to find the file in the fix folder
            fixFile ??= otherFolderFiles
                    .Where(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

            if (messageHandler.MustCancel) { return; }

            // file is found: fix the game
            if (fixFile != null)
            {
                // copy existing file, if it doesn't exist already
                if (!fs.FileExists(gameFileFixed))
                {
                    fs.FileCopy(gameFile, gameFileFixed, false);
                }

                await fs.ReplaceZipFile(fs.PathJoin(otherFolder, fixFile.ZipFileName), gameFileFixed, fixFile.Name);

                // update the metadata
                romFile.ErrorReason = ErrorReason.None;
                romFile.ErrorDetails = null;
            }
        }
    }

    public string GetDatPath(string datfilePre, string datfileCustom) {
        var dat = datfilePre;
        if (dat == "custom")
        {
            dat = datfileCustom;
        }
        else
        {
            dat = fs.GetDataPath("mame-xml", dat, "games.xml");
        }

        return dat;
    }

    public async Task<int> ChangeGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList allGames, GameRomFilesList fixFolder, IMessageHandler messageHandler, CsvGamesList csv)
    {
        // TODO

        return progress;
    }

    private async Task<CsvGamesList> GetCsvData(string csvfilter) {
        if (!string.IsNullOrEmpty(csvfilter) && fs.FileExists(csvfilter))
        {
            return await csvService.ReadFile(csvfilter, false);
        }

        return null;
    }

    private List<string> GetFixFolderFiles(string otherFolder, bool repair, bool change, IMessageHandler messageHandler) {
        if (!messageHandler.MustCancel && (repair || change))
        {
            messageHandler.Progress("Reading files in fix folder", 0, 0);
            return fs.GetFiles(otherFolder, "*.zip");
        }

        return [];
    }

    private (GameRomFilesList files, int progress) GetFixFolderFilesDetails(int total, int progress, IEnumerable<string> fixFiles, bool isslow, bool repairOrChange, IMessageHandler messageHandler) {
        if (!messageHandler.MustCancel && repairOrChange)
        {
            GameRomFilesList result = [];

            foreach (var ff in fixFiles)
            {
                messageHandler.Progress($"Reading {fs.FileName(ff)} in additional folder", total, progress++);

                // read infos of files in zip
                result.AddRange(fs.GetZipFiles(ff, isslow));

                if (messageHandler.MustCancel) { break; }
            }

            return (result, progress);
        }

        return ([], progress);
    }

    private static int ComputeTotal(int filesCount, bool repair, bool change, int fixFilesCount) {
        int total = filesCount + fixFilesCount;

        // total depends on chosen action (check=x1, repair=x2, rebuild=x3)
        if (repair)
        {
            total *= 3; // check, then list files in fix folder, then repair
        }
        else if (change)
        {
            total *= 4; // same + rebuild
        }

        return total;
    }
}
