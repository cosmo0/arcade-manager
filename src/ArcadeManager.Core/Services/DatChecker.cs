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

        if (!fs.DirectoryExists(args.Romset))
        {
            messageHandler.Error(new DirectoryNotFoundException($"Folder {args.Romset} not found"));
            return;
        }

        messageHandler.Progress("Reading DAT file", 0, 0);
        var filesInRomset = fs.GetFiles(args.Romset, "*.zip");

        // if the other folder is empty, look in the other files of the romset
        if (string.IsNullOrEmpty(args.OtherFolder)) {
            args.OtherFolder = args.Romset;
        }

        bool repair = args.Action == "fix" || args.Action == "change";
        bool change = args.Action == "change";

        int progress = 0;

        try
        {
            // get the DAT file path
            string dat = GetDatPath(args.DatFile, args.DatFilePath);

            // build a found files dataset
            var processed = new GameRomList();

            // read the csv file if it is sent
            CsvGamesList csv = await GetCsvData(args.CsvFilter);

            messageHandler.Progress("Reading DAT file", 0, 0);
            var games = await datFile.GetRoms(dat);

            // get the list of files in the repair folder
            List<string> otherFiles = GetOtherFolderFiles(args.OtherFolder, repair, change, messageHandler);
            int total = ComputeTotal(filesInRomset.Count, repair, change, otherFiles.Count);

            // process check
            foreach (var game in games.OrderBy(g => g.Name))
            {
                progress = CheckGame(total, progress, game, args, processed, messageHandler, csv);
                if (messageHandler.MustCancel) { break; }
            }

            GameRomFilesList otherFolderFiles = [];
            if (!messageHandler.MustCancel && repair) {
                // get the files details from the repair folder
                (otherFolderFiles, progress) = GetOtherFolderFilesDetails(total, progress, otherFiles, args.CheckSha1, messageHandler);
            }

            // if error fixing: loop on errors and try to find a file to fix it with
            if (!messageHandler.MustCancel && repair)
            {
                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await FixGame(total, progress, game, args, games, otherFolderFiles, messageHandler, csv);

                    if (messageHandler.MustCancel) { break; }
                }
            }

            // if romset type change: move/copy files around
            if (!messageHandler.MustCancel && change)
            {
                foreach (var game in games.OrderBy(g => g.Name))
                {
                    progress = await ChangeGame(total, progress, game, args, otherFolderFiles, messageHandler, csv);

                    if (messageHandler.MustCancel) { break; }
                }
            }

            messageHandler.Done($"Checked {progress} roms", args.TargetFolder);
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
    public int CheckGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList processed, IMessageHandler messageHandler, CsvGamesList csv)
    {
        if (messageHandler.MustCancel) { return progress; }

        // build file path
        var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");

        // if the CSV filter is set, check if the game is in the list
        if (csv != null && !csv.Games.Any(g => g.Name == game.Name))
        {
            return progress + 1; // increment anyway, since the total is based on number of files on the disk
        }

        // check if a matching file is on the disk
        if (!fs.FileExists(gameFile))
        {
            if (args.ReportAll)
            {
                // report all errors
                game.Error(ErrorReason.MissingFile, $"Missing rom {game.Name}.zip", $"{game.Name}.zip");
                processed.Add(game);
            }

            // then skip to next file
            return progress;
        }

        progress++;

        // progress only if we actually check the game
        messageHandler.Progress($"Checking {game.Name}", total, progress);

        // if we are here: we are always processing the game, and we do not have added it to the list yet
        processed.Add(game);

        if (messageHandler.MustCancel) { return progress; }

        CheckFilesOfGame(game, gameFile, args.CheckSha1, messageHandler);

        messageHandler.Processed(game);

        return progress;
    }

    /// <summary>
    /// Checks the files of a game
    /// </summary>
    /// <param name="game">The game infos to check the files of</param>
    /// <param name="filePath">The folder of the roms</param>
    /// <param name="checkSha1">Whether to check the SHA1</param>
    /// <param name="messageHandler">The message handler</param>
    public void CheckFilesOfGame(GameRom game, string filePath, bool checkSha1, IMessageHandler messageHandler)
    {
        if (!fs.FileExists(filePath)) {
            return;
        }

        // open the zip
        var zipFiles = fs.GetZipFiles(filePath, checkSha1);

        if (messageHandler.MustCancel) { return; }

        // check the files that are supposed to be in the game
        foreach (var gameFile in game.RomFiles)
        {
            if (messageHandler.MustCancel) { return; }

            var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(gameFile.Name, StringComparison.InvariantCultureIgnoreCase));

            CheckFileOfGame(game, gameFile, zipFile, checkSha1);
        }

        // if the set is merged, check the parent's files in the zip
        if (game.Parent != null) {
            var filesOnlyInParent = GetFilesOnlyInParent(game);
            foreach (var fip in filesOnlyInParent) {
                if (messageHandler.MustCancel) { return; }

                var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(fip.Name, StringComparison.InvariantCultureIgnoreCase));
                if (zipFile != null) { // don't trigger file missing error if it's not a merged set
                    CheckFileOfGame(game, fip, zipFile, checkSha1);
                }
            }
        }
    
        // check clones that are located inside the zip in case of merged set
        if (zipFiles.Any(zf => !string.IsNullOrEmpty(zf.Path))) {
            foreach (var clone in game.Clones) {
                foreach (var cloneFile in clone.RomFiles) {
                    var cloneZipFile = zipFiles.FirstOrDefault(zf => zf.Path.Equals(clone.Name, StringComparison.InvariantCultureIgnoreCase) && zf.Name.Equals(cloneFile.Name, StringComparison.InvariantCultureIgnoreCase));
                    CheckFileOfGame(clone, cloneFile, cloneZipFile, checkSha1);
                }
            }
        }

        // check that the bios exists (don't check files inside, as it *should* be also in the dat)
        if (game.Bios != null && !fs.FileExists(fs.PathJoin(filePath, $"{game.BiosName}.zip"))) {
            game.Error(ErrorReason.MissingFile, $"Missing BIOS file {game.BiosName}", $"{game.BiosName}.zip");
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
    /// <param name="otherFolderFiles">The list of files in the additional folder</param>
    /// <param name="messageHandler">The message handler</param>
    /// <param name="csv">The CSV filter file</param>
    /// <returns>The new progress value</returns>
    public async Task<int> FixGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomList allGames, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler, CsvGamesList csv)
    {
        if (messageHandler.MustCancel) { return progress; }

        progress++;

        // no error in the files
        if (!game.HasError && !game.RomFiles.HasError)
        {
            return progress;
        }

        // if the CSV filter is set, check if the game is in the list
        if (csv != null && !csv.Games.Any(g => g.Name == game.Name))
        {
            return progress;
        }

        // progress only if we actually process the game
        messageHandler.Progress($"Fixing {game.Name}", total, progress);

        // build file path
        var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");
        var gameFileFixed = fs.PathJoin(args.TargetFolder, $"{game.Name}.zip");

        // always copy the file, if it doesn't exist already
        if (fs.Exists(gameFile) && !fs.FileExists(gameFileFixed)) {
            fs.FileCopy(gameFile, gameFileFixed, false);
        }

        if (messageHandler.MustCancel) { return progress; }

        // the whole rom has not been found
        if (game.ErrorReason == ErrorReason.MissingFile)
        {
            // check if it exists in the fix folder
            var existing = otherFolderFiles.FirstOrDefault(f => f.ZipFileName.Equals($"{game.Name}.zip", StringComparison.InvariantCultureIgnoreCase));
            if (existing != null && !fs.FileExists(gameFileFixed))
            {
                fs.FileCopy(fs.PathJoin(args.OtherFolder, $"{game.Name}.zip"), gameFileFixed, true);
                    
                if (messageHandler.MustCancel) { return progress; }

                // check that the files match the expected values; if not it will be rebuilt in the next step
                CheckFilesOfGame(game, gameFileFixed, args.CheckSha1, messageHandler);
            }
        }

        if (messageHandler.MustCancel) { return progress; }

        // check files with errors
        await FixGameFiles(game, allGames, args.OtherFolder, otherFolderFiles, gameFile, gameFileFixed, messageHandler);

        if (messageHandler.MustCancel) { return progress; }

        // re-check the rom
        CheckFilesOfGame(game, gameFileFixed, args.CheckSha1, messageHandler);

        messageHandler.Fixed(game);

        return progress;
    }

    /// <summary>
    /// Fixes the files of a game
    /// </summary>
    /// <param name="game">The gmae informations</param>
    /// <param name="allGames">All the games informations</param>
    /// <param name="otherFolder">The path to the additional folder</param>
    /// <param name="otherFolderFiles">The list of files in the additional folder</param>
    /// <param name="gameFile">The game file to copy from if missing</param>
    /// <param name="gameFileFixed">The target game file to fix</param>
    /// <param name="messageHandler">The message handler</param>
    public async Task FixGameFiles(GameRom game, GameRomList allGames, string otherFolder, GameRomFilesList otherFolderFiles, string gameFile, string gameFileFixed, IMessageHandler messageHandler) {
        if (!fs.Exists(gameFile)) { return; }

        foreach (var romFile in game.RomFiles.Where(f => f.HasError))
        {
            // try to find the file in the already-processed files
            var repairFile = allGames
                .SelectMany(r => r.RomFiles)
                .FirstOrDefault(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase));

            // not found: try to find the file in the fix folder
            repairFile ??= otherFolderFiles
                    .FirstOrDefault(f => f.Name.Equals(romFile.Name, StringComparison.InvariantCultureIgnoreCase) && f.Crc.Equals(romFile.Crc, StringComparison.InvariantCultureIgnoreCase));

            if (messageHandler.MustCancel) { return; }

            // file is found: fix the game
            if (repairFile != null)
            {
                // copy existing file, if it doesn't exist already
                if (!fs.FileExists(gameFileFixed))
                {
                    fs.FileCopy(gameFile, gameFileFixed, false);
                }

                if (messageHandler.MustCancel) { return; }

                await fs.ReplaceZipFile(fs.PathJoin(otherFolder, repairFile.ZipFileName), gameFileFixed, repairFile.Name);

                // update the metadata
                romFile.ErrorReason = ErrorReason.None;
                romFile.ErrorDetails = null;
            }
        }
    }

    /// <summary>
    /// Changes a game to a non-merged romset
    /// </summary>
    /// <param name="total">The total number of items</param>
    /// <param name="progress">The current item</param>
    /// <param name="game">The game to process</param>
    /// <param name="args">The action arguments</param>
    /// <param name="otherFolderFiles">The path </param>
    /// <param name="messageHandler"></param>
    /// <param name="csv"></param>
    /// <returns></returns>
    public async Task<int> ChangeGame(int total, int progress, GameRom game, RomsActionCheckDat args, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler, CsvGamesList csv)
    {
        if (messageHandler.MustCancel) { return progress; }

        progress++;

        // if the CSV filter is set, check if the game is in the list
        if (csv != null && !csv.Games.Any(g => g.Name == game.Name))
        {
            return progress;
        }

        // build path
        var targetRom = fs.PathJoin(args.TargetFolder, $"{game.Name}.zip");

        // progress only if we actually process the game
        messageHandler.Progress($"Changing {game.Name}", total, progress);

        // if the rom to change does not exist, we can't do anything
        if (!fs.FileExists(targetRom)) {
            return progress;
        }

        var targetZipFiles = fs.GetZipFiles(targetRom, args.CheckSha1);

        if (messageHandler.MustCancel) { return progress; }

        // parent roms: if the zip contains the clones, extract the clones
        if (game.Parent == null && game.Clones.Any()) {
            await ExtractClone(game, targetRom, targetZipFiles, args, messageHandler);

            if (messageHandler.MustCancel) { return progress; }

            CleanupFilesOfGame(game, targetRom);

            if (messageHandler.MustCancel) { return progress; }

            // check the files again
            CheckFilesOfGame(game, targetRom, args.CheckSha1, messageHandler);

            return progress;
        }

        // clone roms: copy the files from the parent, if they're missing
        if (game.Parent != null) {
            await RebuildFromParent(game, targetRom, targetZipFiles, otherFolderFiles, args, messageHandler);

            if (messageHandler.MustCancel) { return progress; }

            CleanupFilesOfGame(game, targetRom);

            if (messageHandler.MustCancel) { return progress; }

            // check the files again
            CheckFilesOfGame(game, targetRom, args.CheckSha1, messageHandler);
        }

        return progress;
    }

    private async Task ExtractClone(GameRom game, string targetRom, IEnumerable<GameRomFile> targetZipFiles, RomsActionCheckDat args, IMessageHandler messageHandler) {
        // get the clones inside the zip
        foreach (var cloneName in targetZipFiles.Where(f => !string.IsNullOrEmpty(f.Path)).Select(f => f.Path).Distinct()) {
            var targetCloneFile = fs.PathJoin(args.TargetFolder, $"{cloneName}.zip");

            // get the files related to the clone
            foreach (var cloneZipFile in targetZipFiles.Where(f => f.Path == cloneName)) {
                // check that we actually want this file
                if (game.Clones[cloneName].RomFiles.Any(f => f.Name == cloneZipFile.Name && f.Crc == cloneZipFile.Crc)) {
                    await fs.ReplaceZipFile(targetRom, targetCloneFile, cloneZipFile.Name, cloneZipFile.Path);
                    
                    cloneZipFile.ErrorReason = ErrorReason.None;
                    cloneZipFile.ErrorDetails = null;

                    if (messageHandler.MustCancel) { return; }
                }
            }
            
            if (messageHandler.MustCancel) { return; }
        }
    }

    private async Task RebuildFromParent(GameRom game, string targetRom, IEnumerable<GameRomFile> targetZipFiles, GameRomFilesList otherFolderFiles, RomsActionCheckDat args, IMessageHandler messageHandler) {
        var parentFileName = $"{game.Parent.Name}";
            
        // get the files of the parent that are not in the current clone
        var filesOnlyInParent = GetFilesOnlyInParent(game);

        // get the missing files from the parent that are not already in the clone
        foreach (var missingFile in filesOnlyInParent.Where(pf => !targetZipFiles.Any(zf => zf.Name == pf.Name && zf.Crc == pf.Crc))) {
            await fs.ReplaceZipFile(fs.PathJoin(args.Romset, parentFileName), targetRom, missingFile.Name);
            
            if (messageHandler.MustCancel) { return; }
        }

        if (messageHandler.MustCancel) { return; }

        if (otherFolderFiles.Any()) {
            // get the list of files again
            targetZipFiles = fs.GetZipFiles(targetRom, args.CheckSha1);

            // get the still-missing files from the other folder
            foreach (var missingFile in filesOnlyInParent.Where(pf => !targetZipFiles.Any(zf => zf.Name == pf.Name && zf.Crc == pf.Crc))) {
                var otherFile = otherFolderFiles.FirstOrDefault(of => of.Name == missingFile.Name && of.Crc == missingFile.Crc);
                if (otherFile != null) {
                    var otherFilePath = fs.PathJoin(args.OtherFolder, otherFile.ZipFileName);
                    await fs.ReplaceZipFile(otherFilePath, targetRom, otherFile.Name, otherFile.Path);
                }

                if (messageHandler.MustCancel) { return; }
            }
        }
    }

    private void CleanupFilesOfGame(GameRom game, string file) {
        if (!fs.FileExists(file)) { return; }

        // the list of files in the current rom
        var romFiles = GetAllFilesOfGame(game);

        // get the files in the zip
        var zipFiles = fs.GetZipFiles(file, false);

        // get the files that are in the zip but shouldn't
        var excessFiles = zipFiles.Where(zf => !romFiles.Any(rf => rf.Name == zf.Name));

        // remove them
        fs.DeleteZipFile(file, excessFiles);
    }

    private async Task<CsvGamesList> GetCsvData(string csvfilter) {
        if (!string.IsNullOrEmpty(csvfilter) && fs.FileExists(csvfilter))
        {
            return await csvService.ReadFile(csvfilter, false);
        }

        return null;
    }

    private List<string> GetOtherFolderFiles(string otherFolder, bool repair, bool change, IMessageHandler messageHandler) {
        if (!messageHandler.MustCancel && (repair || change))
        {
            messageHandler.Progress("Reading files in fix folder", 0, 0);
            return fs.GetFiles(otherFolder, "*.zip");
        }

        return [];
    }

    private (GameRomFilesList files, int progress) GetOtherFolderFilesDetails(int total, int progress, IEnumerable<string> fixFiles, bool isslow, IMessageHandler messageHandler) {
        if (messageHandler.MustCancel) { return ([], progress); }

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
    
    private static void CheckFileOfGame(GameRom game, GameRomFile datFile, GameRomFile zipFile, bool checkSha1) {
        // ensure the file exists in the zip
        if (zipFile == null)
        {
            game.Error(ErrorReason.MissingFile, "Missing file in zip", datFile.Name);
            return;
        }

        // check size and crc
        if (!zipFile.Crc.Equals(datFile.Crc, StringComparison.InvariantCultureIgnoreCase))
        {
            game.Error(ErrorReason.BadHash, $"Bad CRC - expected: {datFile.Crc}; actual: {zipFile.Crc}", datFile.Name);
            return;
        }

        // if speed slow: check hash
        if (checkSha1 && !string.IsNullOrEmpty(datFile.Sha1) && !zipFile.Sha1.Equals(datFile.Sha1, StringComparison.InvariantCultureIgnoreCase))
        {
            game.Error(ErrorReason.BadHash, $"Bad SHA1 - expected: {datFile.Sha1}; actual: {zipFile.Sha1}", datFile.Name);
        }
    }
    
    private string GetDatPath(string datfilePre, string datfileCustom) {
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

    private static List<GameRomFile> GetAllFilesOfGame(GameRom game) {
        var romFiles = game.RomFiles.ToList(); // ToList does a copy
        if (game.Parent != null) {
            // add the roms of the parent that are not in the clone
            romFiles.AddRange(game.Parent.RomFiles.Where(pf => !game.RomFiles.Any(cf => cf.Name == pf.Name)));
        }

        return romFiles;
    }

    private static IEnumerable<GameRomFile> GetFilesOnlyInParent(GameRom game) {
        if (game.Parent == null) { return []; }

        return game.Parent.RomFiles.Where(pf => !game.RomFiles.Any(cf => cf.Name == pf.Name));
    }
}
