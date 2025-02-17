using System;
using System.IO.Compression;
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

        if (args.Romset.Equals(args.TargetFolder, StringComparison.InvariantCultureIgnoreCase)) {
            messageHandler.Error(new ArgumentException($"Source and target folder are identical"));
            return;
        }

        fs.EnsureDirectory(args.TargetFolder);

        messageHandler.Progress("Reading DAT file", 0, 0);
        var filesInRomset = fs.GetFiles(args.Romset, "*.zip");

        // if the other folder is empty, look in the other files of the romset
        if (string.IsNullOrEmpty(args.OtherFolder)) {
            args.OtherFolder = args.Romset;
        }

        try
        {
            // get the DAT file path
            string dat = GetDatPath(args.DatFile, args.DatFilePath);

            // build a found files dataset
            var processed = new GameRomList();

            // read the csv file if it is sent
            CsvGamesList csv = await GetCsvData(args.CsvFilter);

            messageHandler.Progress("Reading DAT file", 0, 0);
            var allGames = await datFile.GetRoms(dat, args.Romset);

            // remove games that are not in the CSV
            KeepCsvGames(allGames, csv);

            // get the list of files in the repair folder
            List<string> otherFiles = GetOtherFolderFiles(args, messageHandler);
            int total = ComputeTotal(filesInRomset.Count, args.ChangeType, otherFiles.Count);

            messageHandler.TotalItems = total;

            // process check
            CheckGames(allGames, processed, args, messageHandler);

            GameRomFilesList otherFolderFiles = [];
            if (!messageHandler.MustCancel && args.ChangeType && args.OtherFolder != args.Romset) {
                // get the files details from the repair folder
                otherFolderFiles = GetOtherFolderFilesDetails(otherFiles, args, messageHandler);
            }

            await RebuildGames(args, allGames, otherFolderFiles, messageHandler);

            var andFixed = args.ChangeType ? "and fixed" : "";
            messageHandler.Done($"Checked {andFixed} {allGames.Count} roms", args.TargetFolder);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    private void CheckGames(GameRomList allGames, GameRomList processed, RomsActionCheckDat args, IMessageHandler messageHandler) {
        foreach (var game in allGames.OrderBy(g => g.Name))
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;

            CheckGame(game, args, processed, messageHandler);
        }
    }

    /// <summary>
    /// Checks the specified game
    /// </summary>
    /// <param name="game">The game to check</param>
    /// <param name="args">The action arguments</param>
    /// <param name="processed">The list of already processed games</param>
    /// <param name="messageHandler">The message handler</param>
    /// <returns>The new progress value</returns>
    public void CheckGame(GameRom game, RomsActionCheckDat args, GameRomList processed, IMessageHandler messageHandler)
    {
        if (messageHandler.MustCancel) { return; }

        // build file path
        var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");

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
            return;
        }

        // progress only if we actually check the game
        messageHandler.Progress($"Checking {game.Name}");

        // if we are here: we are always processing the game, and we do not have added it to the list yet
        processed.Add(game);

        if (messageHandler.MustCancel) { return; }

        using var zip = fs.OpenZipRead(gameFile);
        CheckFilesOfGame(zip, game, gameFile, args.CheckSha1, messageHandler);

        messageHandler.Processed(game);
    }

    /// <summary>
    /// Checks the files of a game
    /// </summary>
    /// <param name="zip">The zip file to check</param>
    /// <param name="game">The game infos to check the files of</param>
    /// <param name="filePath">The folder of the roms</param>
    /// <param name="checkSha1">Whether to check the SHA1</param>
    /// <param name="messageHandler">The message handler</param>
    public void CheckFilesOfGame(ZipArchive zip, GameRom game, string filePath, bool checkSha1, IMessageHandler messageHandler)
    {
        if (!fs.FileExists(filePath)) { return; }

        // open the zip
        var fileName = fs.FileName(filePath);
        var fileFolder = fs.DirectoryName(filePath);

        var zipFiles = fs.GetZipFiles(zip, fileName, fileFolder, checkSha1);

        // check the files that are supposed to be in the game
        foreach (var gameFile in game.RomFiles)
        {
            if (messageHandler.MustCancel) { return; }

            var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(gameFile.Name, StringComparison.InvariantCultureIgnoreCase));

            CheckFileOfGame(game, gameFile, zipFile, checkSha1);
        }

        // if the set is merged, check the parent's files in the zip
        if (game.Parent != null) {
            CheckFilesOfParent(game, zipFiles, checkSha1, messageHandler);
        }
    
        // check clones that are located inside the zip in case of merged set
        if (zipFiles.Any(zf => !string.IsNullOrEmpty(zf.Path))) {
            foreach (var clone in game.Clones) {
                CheckFilesOfClonesInZip(clone, zipFiles, checkSha1);
            }
        }

        // check that the bios exists (don't check files inside, as it *should* be also in the dat)
        if (game.Bios != null && !fs.FileExists(fs.PathJoin(filePath, $"{game.BiosName}.zip"))) {
            game.Error(ErrorReason.MissingFile, $"Missing BIOS file {game.BiosName}", $"{game.BiosName}.zip");
        }
    }

    private static void CheckFilesOfParent(GameRom game, IEnumerable<GameRomFile> zipFiles, bool checkSha1, IMessageHandler messageHandler) {
        var filesOnlyInParent = GetFilesOnlyInParent(game);
        foreach (var fip in filesOnlyInParent) {
            if (messageHandler.MustCancel) { return; }

            var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(fip.Name, StringComparison.InvariantCultureIgnoreCase));
            if (zipFile != null) { // don't trigger file missing error if it's not a merged set
                CheckFileOfGame(game, fip, zipFile, checkSha1);
            }
        }
    }

    private static void CheckFilesOfClonesInZip(GameRom clone, IEnumerable<GameRomFile> zipFiles, bool checkSha1) {
        foreach (var cloneFile in clone.RomFiles) {
            var cloneZipFile = zipFiles.FirstOrDefault(zf => zf.Path.Equals(clone.Name, StringComparison.InvariantCultureIgnoreCase) && zf.Name.Equals(cloneFile.Name, StringComparison.InvariantCultureIgnoreCase));
            CheckFileOfGame(clone, cloneFile, cloneZipFile, checkSha1);
        }
    }

    private static void CheckFileOfGame(GameRom game, GameRomFile datFile, GameRomFile zipFile, bool checkSha1) {
        // ensure the file exists in the zip
        if (zipFile == null)
        {
            game.Error(ErrorReason.MissingFile, "Missing file in zip", datFile.Name);
            return;
        }

        // check size and crc
        if (zipFile.Crc != datFile.Crc)
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

    private async Task RebuildGames(RomsActionCheckDat args, GameRomList allGames, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler) {
        if (!args.ChangeType) { return; }

        foreach (var game in allGames.OrderBy(g => g.Name))
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;

            var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");
            var gameFileFixed = fs.PathJoin(args.TargetFolder, $"{game.Name}.zip");

            // create the file in target folder and list the files
            var fileFixedZipFiles = EnsureFileExists(game, gameFile, gameFileFixed, otherFolderFiles, args, messageHandler);

            if (messageHandler.MustCancel) { break; }

            // open the target zip in write mode
            using var targetZip = fs.OpenZipWrite(gameFileFixed);

            if (messageHandler.MustCancel) { break; }

            // try to fix the rom
            await FixGame(targetZip, game, fileFixedZipFiles, args, allGames, otherFolderFiles, messageHandler);

            if (messageHandler.MustCancel) { break; }

            // try to convert the rom to non-merged
            await ChangeGame(targetZip, game, fileFixedZipFiles, args, otherFolderFiles, messageHandler);
        }
    }

    /// <summary>
    /// Fixes the specified game
    /// </summary>
    /// <param name="targetZip">The target zip to fix</param>
    /// <param name="game">The game to check</param>
    /// <param name="fileFixedZipFiles">The list of files in the fixed zip</param>
    /// <param name="args">The action arguments</param>
    /// <param name="allGames">The list of all games in the DAT</param>
    /// <param name="otherFolderFiles">The list of files in the additional folder</param>
    /// <param name="messageHandler">The message handler</param>
    /// <returns>The new progress value</returns>
    public async Task FixGame(ZipArchive targetZip, GameRom game, GameRomFilesList fileFixedZipFiles, RomsActionCheckDat args, GameRomList allGames, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler)
    {
        // no error in the files
        if (!game.HasError && !game.RomFiles.HasError) { return; }
        
        // progress only if we actually process the game
        messageHandler.Progress($"Fixing {game.Name}");

        if (messageHandler.MustCancel) { return; }

        // fix the files with errors
        if (game.HasError) {
            if (messageHandler.MustCancel) { return; }

            await FixGameFiles(targetZip, game, fileFixedZipFiles, allGames, otherFolderFiles, messageHandler);
        }

        messageHandler.Fixed(game);
    }

    /// <summary>
    /// Fixes the files of a game
    /// </summary>
    /// <param name="targetZip">The target zip to fix</param>
    /// <param name="game">The gmae informations</param>
    /// <param name="fileFixedZipFiles">The list of files in the fixed zip</param>
    /// <param name="allGames">All the games informations</param>
    /// <param name="otherFolderFiles">The list of files in the additional folder</param>
    /// <param name="messageHandler">The message handler</param>
    public async Task FixGameFiles(ZipArchive targetZip, GameRom game, GameRomFilesList fileFixedZipFiles, GameRomList allGames, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler) {
        foreach (var romFile in game.RomFiles.Where(f => f.HasError))
        {
            // try to find the file in the other game files
            var repairFile = allGames?
                .SelectMany(r => r.RomFiles)
                .FirstOrDefault(f => f.Name == romFile.Name && f.Crc == romFile.Crc);

            // not found: try to find the file in the fix folder
            repairFile ??= otherFolderFiles?
                    .FirstOrDefault(f => f.Name == romFile.Name && f.Crc == romFile.Crc);

            if (messageHandler.MustCancel) { return; }

            // file is found: fix the game
            if (repairFile != null)
            {
                messageHandler.Progress($"Fixing {game.Name} - file {romFile.Name}");

                await fs.ReplaceZipFile(targetZip, repairFile);

                // update the metadata
                romFile.ErrorReason = ErrorReason.None;
                romFile.ErrorDetails = null;

                // update the fileFixedZipFiles list
                fileFixedZipFiles.ReplaceFile(romFile, game);

                if (messageHandler.MustCancel) { return; }
            }
        }
    }

    /// <summary>
    /// Changes a game to a non-merged romset
    /// </summary>
    /// <param name="game">The game to process</param>
    /// <param name="args">The action arguments</param>
    /// <param name="otherFolderFiles">The path </param>
    /// <param name="messageHandler">The message handler</param>
    public async Task ChangeGame(ZipArchive targetZip, GameRom game, GameRomFilesList fileFixedZipFiles, RomsActionCheckDat args, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler)
    {
        // build path
        var targetRom = fs.PathJoin(args.TargetFolder, $"{game.Name}.zip");

        // if the rom to change does not exist, we can't do anything
        if (!fs.FileExists(targetRom)) { return; }

        // progress only if we actually process the game
        messageHandler.Progress($"Changing {game.Name}");

        if (messageHandler.MustCancel) { return; }

        // parent roms: if the zip contains the clones, extract the clones
        if (game.Parent == null && game.Clones.Any()) {
            await ExtractClone(targetZip, game, fileFixedZipFiles, messageHandler);

            if (messageHandler.MustCancel) { return; }

            CleanupFilesOfGame(targetZip, game, fileFixedZipFiles);

            return;
        }

        // clone roms: copy the files from the parent, if they're missing
        if (game.Parent != null) {
            await RebuildFromParent(targetZip, game, fileFixedZipFiles, otherFolderFiles, messageHandler);

            if (messageHandler.MustCancel) { return; }

            CleanupFilesOfGame(targetZip, game, fileFixedZipFiles);
        }
    }

    private async Task ExtractClone(ZipArchive zip, GameRom game, GameRomFilesList targetZipFiles, IMessageHandler messageHandler) {
        GameRomFilesList filesToReplace = [];

        // get the clones inside the zip
        foreach (var cloneName in targetZipFiles.Where(f => !string.IsNullOrEmpty(f.Path)).Select(f => f.Path).Distinct()) {
            // get the files related to the clone
            foreach (var cloneZipFile in targetZipFiles.Where(tf => tf.Path == cloneName)) {
                // check that we actually want this file
                if (game.Clones[cloneName].RomFiles.Any(cf => cf.Name == cloneZipFile.Name && cf.Crc == cloneZipFile.Crc)) {
                    await fs.ReplaceZipFile(zip, cloneZipFile);
                    
                    cloneZipFile.ErrorReason = ErrorReason.None;
                    cloneZipFile.ErrorDetails = null;

                    filesToReplace.Add(cloneZipFile);

                    if (messageHandler.MustCancel) { return; }
                }
            }
            
            if (messageHandler.MustCancel) { return; }
        }

        foreach (var f in filesToReplace) {
            targetZipFiles.ReplaceFile(f, game);
        }
    }

    private async Task RebuildFromParent(ZipArchive targetZip, GameRom game, GameRomFilesList targetZipFiles, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler) {
        // get the files of the parent that are not in the current clone
        var filesOnlyInParent = GetFilesOnlyInParent(game);

        // get the missing files from the parent that are not already in the clone
        foreach (var missingFile in filesOnlyInParent.Where(pf => !targetZipFiles.Any(zf => zf.Name == pf.Name && zf.Crc == pf.Crc))) {
            await fs.ReplaceZipFile(targetZip, missingFile);
            
            // update targetZipFiles
            targetZipFiles.ReplaceFile(missingFile, game);

            if (messageHandler.MustCancel) { return; }
        }

        await RebuildFromParentUsingOther(targetZip, game, targetZipFiles, filesOnlyInParent, otherFolderFiles, messageHandler);
    }

    private async Task RebuildFromParentUsingOther(ZipArchive targetZip, GameRom game, GameRomFilesList targetZipFiles, IEnumerable<GameRomFile> filesOnlyInParent, GameRomFilesList otherFolderFiles, IMessageHandler messageHandler) {
        if (otherFolderFiles == null || !otherFolderFiles.Any()) {
            return;
        }

        if (messageHandler.MustCancel) { return; }
        
        // get the still-missing files from the other folder
        foreach (var missingFile in filesOnlyInParent.Where(pf => !targetZipFiles.Any(zf => zf.Name == pf.Name && zf.Crc == pf.Crc))) {
            var otherFile = otherFolderFiles.FirstOrDefault(of => of.Name == missingFile.Name && of.Crc == missingFile.Crc);
            if (otherFile != null) {
                await fs.ReplaceZipFile(targetZip, otherFile);

                // update targetZipFiles
                targetZipFiles.ReplaceFile(otherFile, game);
            }

            if (messageHandler.MustCancel) { return; }
        }
    }

    private void CleanupFilesOfGame(ZipArchive zip, GameRom game, GameRomFilesList zipFiles) {
        // the list of files in the current rom
        var romFiles = GetAllFilesOfGame(game);

        // get the files that are in the zip but shouldn't
        var excessFiles = zipFiles.Where(zf => !romFiles.Any(rf => rf.Name == zf.Name));

        // remove them
        fs.DeleteZipFile(zip, excessFiles);

        // update zipFiles (the ToList() clones the list so zipFiles can be modified)
        foreach (var f in excessFiles.ToList()) {
            zipFiles.RemoveFile(f.Name, f.Path);
        }
    }

    private GameRomFilesList EnsureFileExists(GameRom game, string gameFile, string gameFileFixed, GameRomFilesList otherFolderFiles, RomsActionCheckDat args, IMessageHandler messageHandler) {
        // try to copy the file, if it doesn't exist already
        if (fs.FileExists(gameFile) && !fs.FileExists(gameFileFixed)) {
            fs.FileCopy(gameFile, gameFileFixed, false);
        }

        // still not found: try to copy from other folder
        if (!fs.FileExists(gameFileFixed) && otherFolderFiles != null && otherFolderFiles.Any()) {
            var otherFile = otherFolderFiles.FirstOrDefault(f => f.ZipFileName.Equals($"{game.Name}.zip", StringComparison.InvariantCultureIgnoreCase));
            if (otherFile != null) {
                fs.FileCopy(fs.PathJoin(args.OtherFolder, otherFile.ZipFileName), gameFileFixed, false);

                if (messageHandler.MustCancel) { return []; }
            }
        }

        if (fs.FileExists(gameFileFixed)) {
            // check that the files match the expected values; if not it will be rebuilt in the next step
            using var zip = fs.OpenZipRead(gameFileFixed);
            CheckFilesOfGame(zip, game, gameFileFixed, args.CheckSha1, messageHandler);
            return [.. fs.GetZipFiles(zip, $"{game.Name}.zip", fs.DirectoryName(gameFileFixed), args.CheckSha1)];
        }

        // still not found: it will be rebuilt in a later step
        return [];
    }

    private async Task<CsvGamesList> GetCsvData(string csvfilter) {
        if (!string.IsNullOrEmpty(csvfilter) && fs.FileExists(csvfilter))
        {
            return await csvService.ReadFile(csvfilter, false);
        }

        return null;
    }

    private List<string> GetOtherFolderFiles(RomsActionCheckDat args, IMessageHandler messageHandler) {
        if (args.OtherFolder == args.Romset || !args.ChangeType) {
            return [];
        }

        messageHandler.Progress("Reading files in fix folder", 0, 0);
        return [.. fs.GetFiles(args.OtherFolder, "*.zip").OrderBy(f => f)];
    }

    private GameRomFilesList GetOtherFolderFilesDetails(IEnumerable<string> fixFiles, RomsActionCheckDat args, IMessageHandler messageHandler) {
        if (messageHandler.MustCancel || !args.ChangeType) { return []; }

        GameRomFilesList result = [];

        foreach (var ff in fixFiles)
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;
            messageHandler.Progress($"Reading {fs.FileName(ff)} in additional folder");

            // read infos of files in zip
            result.AddRange(fs.GetZipFiles(ff, args.CheckSha1));
        }

        return result;
    }

    private static int ComputeTotal(int filesCount, bool change, int fixFilesCount) {
        int total = filesCount;

        if (change)
        {
            total *= 2; // check, rebuild
        }

        // listing the files in the fix folder takes a lot of time
        total += fixFilesCount;

        return total;
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

    /// <summary>
    /// Gets all the files of a game rom + its parent if it exists
    /// </summary>
    /// <param name="game">The game to get the rom files of</param>
    /// <returns>The rom files</returns>
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
    
    private static void KeepCsvGames(GameRomList allGames, CsvGamesList csv)
    {
        if (csv == null) { return; }

        allGames.RemoveAll(gdat => csv.Games.Any(gcsv => gcsv.Name.Equals(gdat.Name, StringComparison.InvariantCultureIgnoreCase)));
    }
}
