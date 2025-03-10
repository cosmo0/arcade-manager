using System;
using ArcadeManager.Core;
using ArcadeManager.Core.Actions;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models;
using ArcadeManager.Core.Models.Roms;
using ArcadeManager.Core.Models.Zip;
using ArcadeManager.Core.Services.Interfaces;

namespace ArcadeManager.Core.Services;

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

        if (args.Romset.Equals(args.TargetFolder, StringComparison.InvariantCultureIgnoreCase))
        {
            messageHandler.Error(new ArgumentException($"Source and target folder are identical"));
            return;
        }

        fs.DirectoryEnsure(args.TargetFolder);

        messageHandler.Progress("Reading DAT file", 0, 0);
        var filesInRomset = fs.FilesGetList(args.Romset, "*.zip");

        // if the other folder is empty, look in the other files of the romset
        if (string.IsNullOrEmpty(args.OtherFolder))
        {
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
            IReadOnlyList<GameRom> allGames = KeepCsvGames(await datFile.GetRoms(dat, args.Romset), csv);

            // get the list of files in the repair folder
            List<string> otherFiles = GetOtherFolderFiles(args, messageHandler);
            int total = ComputeTotal(filesInRomset.Count, args.ChangeType, otherFiles.Count);

            messageHandler.TotalItems = total;

            // process check
            var gamesFiles = CheckGames(allGames, processed, args, messageHandler);

            if (args.ChangeType)
            {
                List<ReadOnlyGameRomFile> otherFolderFiles = [];
                if (!messageHandler.MustCancel && args.OtherFolder != args.Romset)
                {
                    // get the files details from the repair folder
                    otherFolderFiles.AddRange(GetOtherFolderFilesDetails(otherFiles, args, messageHandler));
                }

                ReadOnlyGameRomFileList allFiles = new(gamesFiles.Concat(otherFolderFiles));

                await RebuildGames(args, allGames, allFiles, messageHandler);
            }

            var andFixed = args.ChangeType ? "and fixed" : "";
            messageHandler.Done($"Checked {andFixed} {allGames.Count} roms", args.TargetFolder);
        }
        catch (Exception ex)
        {
            messageHandler.Error(ex);
        }
    }

    private List<ReadOnlyGameRomFile> CheckGames(IReadOnlyList<GameRom> allGames, GameRomList processed, RomsActionCheckDat args, IMessageHandler messageHandler)
    {
        var result = new List<ReadOnlyGameRomFile>();

        foreach (var game in allGames.OrderBy(g => g.Name))
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;

            result.AddRange(CheckGame(game, args, processed, messageHandler));
        }

        return result;
    }

    public List<ReadOnlyGameRomFile> CheckGame(GameRom game, RomsActionCheckDat args, GameRomList processed, IMessageHandler messageHandler)
    {
        if (messageHandler.MustCancel) { return []; }

        // build file path
        var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");

        // check if a matching file is on the disk
        if (!fs.FileExists(gameFile))
        {
            game.Error(ErrorReason.MissingFile, $"Missing rom {game.Name}.zip", $"{game.Name}.zip");
            foreach (var f in game.RomFiles)
            {
                game.Error(ErrorReason.MissingFile, "Missing file in zip", f.Name);
            }

            if (args.ReportAll)
            {
                // report all errors
                messageHandler.Processed(game);

                processed.Add(game);
            }

            // then skip to next file
            return [];
        }

        // progress only if we actually check the game
        messageHandler.Progress($"Checking {game.Name}");

        // if we are here: we are always processing the game, and we do not have added it to the list yet
        processed.Add(game);

        if (messageHandler.MustCancel) { return []; }

        using var zip = fs.OpenZipRead(gameFile);
        var files = fs.GetZipFiles(zip, gameFile, args.Romset, args.CheckSha1);
        CheckFilesOfGame(zip, game, args.CheckSha1, messageHandler);

        messageHandler.Processed(game);

        return [.. files.Select(f => f.ToReadOnly(gameFile))];
    }

    private IEnumerable<GameRomFile> CheckFilesOfGame(string zipPath, GameRom game, bool checkSha1, IMessageHandler messageHandler)
    {
        using var zip = fs.OpenZipRead(zipPath);
        return CheckFilesOfGame(zip, game, checkSha1, messageHandler);
    }

    public IEnumerable<GameRomFile> CheckFilesOfGame(ZipFile zip, GameRom game, bool checkSha1, IMessageHandler messageHandler)
    {
        var fileName = fs.FileName(zip?.FilePath);
        var fileFolder = fs.DirectoryName(zip?.FilePath);

        var zipFiles = fs.GetZipFiles(zip, fileName, fileFolder, checkSha1);

        // check the files that are supposed to be in the game
        foreach (var gameFile in game.RomFiles)
        {
            if (messageHandler.MustCancel) { return []; }

            var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(gameFile.Name, StringComparison.InvariantCultureIgnoreCase));

            CheckFileOfGame(game, gameFile, zipFile, checkSha1);
        }

        // if the set is merged, check the parent's files in the zip
        if (game.Parent != null)
        {
            CheckFilesOfParent(game, zipFiles, checkSha1, messageHandler);
        }

        if (messageHandler.MustCancel) { return []; }

        // check clones that are located inside the zip in case of merged set
        if (zipFiles.Any(zf => !string.IsNullOrEmpty(zf.Path)))
        {
            foreach (var clone in game.Clones)
            {
                CheckFilesOfClonesInZip(clone, zipFiles, checkSha1);
            }
        }

        // check that the bios exists (don't check files inside, as it *should* be also in the dat)
        if (game.Bios != null && !fs.FileExists(fs.PathJoin(fileFolder, $"{game.BiosName}.zip")))
        {
            game.Error(ErrorReason.MissingFile, $"Missing BIOS file {game.BiosName}", $"{game.BiosName}.zip");
        }

        return zipFiles;
    }

    private static void CheckFilesOfParent(GameRom game, IEnumerable<GameRomFile> zipFiles, bool checkSha1, IMessageHandler messageHandler)
    {
        foreach (var fip in game.RomFiles)
        {
            if (messageHandler.MustCancel) { return; }

            var zipFile = zipFiles.FirstOrDefault(zf => zf.Name.Equals(fip.Name, StringComparison.InvariantCultureIgnoreCase));
            if (zipFile != null)
            {
                CheckFileOfGame(game, fip, zipFile, checkSha1);
            }
        }
    }

    private static void CheckFilesOfClonesInZip(GameRom clone, IEnumerable<GameRomFile> zipFiles, bool checkSha1)
    {
        foreach (var cloneFile in clone.RomFiles)
        {
            var cloneZipFile = zipFiles.FirstOrDefault(zf => zf.Path.Equals(clone.Name, StringComparison.InvariantCultureIgnoreCase) && zf.Name.Equals(cloneFile.Name, StringComparison.InvariantCultureIgnoreCase));
            CheckFileOfGame(clone, cloneFile, cloneZipFile, checkSha1);
        }
    }

    private static void CheckFileOfGame(GameRom game, GameRomFile datFile, GameRomFile zipFile, bool checkSha1)
    {
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
            return;
        }

        // reset error, if any
        datFile.ErrorDetails = null;
        datFile.ErrorReason = ErrorReason.None;
    }

    private async Task RebuildGames(RomsActionCheckDat args, IReadOnlyList<GameRom> allGames, ReadOnlyGameRomFileList allFiles, IMessageHandler messageHandler)
    {
        if (!args.ChangeType) { return; }

        // first, copy all bioses, so that the checks after rebuilding each file don't trigger "missing bios" error
        foreach (var bios in allGames.Where(g => !string.IsNullOrEmpty(g.BiosName)).Select(g => g.BiosName).Distinct())
        {
            messageHandler.Progress($"Copying bios file {bios}.zip");
            fs.FileCopy(fs.PathJoin(args.Romset, $"{bios}.zip"), fs.PathJoin(args.TargetFolder, $"{bios}.zip"), true);
        }

        foreach (var game in allGames.OrderBy(g => g.Name))
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;

            var gameFile = fs.PathJoin(args.Romset, $"{game.Name}.zip");
            var gameFileFixed = fs.PathJoin(args.TargetFolder, $"{game.Name}.zip");

            if (fs.FileExists(gameFileFixed))
            {
                fs.FileDelete(gameFileFixed);
            }

            // if no error: just copy
            if (!game.HasError)
            {
                messageHandler.Progress($"Copying {game.Name}.zip to target folder");

                fs.FileCopy(gameFile, gameFileFixed, true);

                // cleanup excess files
                GameRomFilesList copiedZipFiles = [.. fs.GetZipFiles(gameFileFixed, false)];
                CleanupFilesOfGame(gameFileFixed, game, copiedZipFiles);

                messageHandler.Processed(game);

                continue;
            }

            // search the romset and other if the game files exist
            ReadOnlyGameRomFileList foundFiles = FindFilesOfGame(game.RomFiles, allFiles, args);

            // if any required file is missing skip the game
            if (game.RomFiles.Count != foundFiles.Count)
            {
                continue;
            }

            if (messageHandler.MustCancel) { break; }

            messageHandler.Progress($"Rebuilding {game.Name}");

            // rebuild the zip
            await RebuildGame(gameFileFixed, foundFiles, game.RomFiles, args);
                
            if (messageHandler.MustCancel) { break; }

            // reset game own errors
            game.Error(ErrorReason.None, null, null);

            // re-check the files
            var zipFiles = CheckFilesOfGame(gameFileFixed, game, args.CheckSha1, messageHandler);

            RebuildGameHandleResult(game, zipFiles, gameFileFixed, messageHandler);
        }
    }

    public async Task RebuildGame(string gameFileFixed, ReadOnlyGameRomFileList foundFiles, GameRomFilesList gameFiles, RomsActionCheckDat args)
    {
        using var targetZip = fs.OpenZipWrite(gameFileFixed);

        foreach (var groupFile in foundFiles.GroupBy(f => f.ZipFilePath))
        {
            using var sourceZip = fs.OpenZipRead(groupFile.Key);
            foreach (var sourceFile in groupFile)
            {
                // get the matching rom from the game, in case the name has changed
                var romFile = gameFiles.First(gf => gf.Crc == sourceFile.Crc && (!args.CheckSha1 || gf.Sha1 == sourceFile.Sha1));

                if (!await fs.ReplaceZipFile(sourceZip, targetZip, sourceFile, romFile))
                {
                    Console.WriteLine($"warning: did not replace {sourceFile.Name} from {sourceZip?.FilePath} to {targetZip?.FilePath}");
                }
            }
        }
    }

    private static ReadOnlyGameRomFileList FindFilesOfGame(List<GameRomFile> requiredFiles, ReadOnlyGameRomFileList allFiles, RomsActionCheckDat args)
    {
        var result = new List<ReadOnlyGameRomFile>();

        foreach (var rf in requiredFiles)
        {
            // check in the processed games
            var foundFile = allFiles.FirstOrDefault(f => f.Crc == rf.Crc && (!args.CheckSha1 || f.Sha1 == rf.Sha1));
            if (foundFile != null)
            {
                result.Add(foundFile);
            }
        }

        return new(result);
    }

    private void RebuildGameHandleResult(GameRom game, IEnumerable<GameRomFile> zipFiles, string gameFileFixed, IMessageHandler messageHandler)
    {
        if (!zipFiles.Any() || game.HasError)
        {
            fs.FileDelete(gameFileFixed);
        }

        messageHandler.Processed(game);
    }

    private void CleanupFilesOfGame(string zipPath, GameRom game, GameRomFilesList zipFiles)
    {
        using var zip = fs.OpenZipWrite(zipPath);
        CleanupFilesOfGame(zip, game, zipFiles);
    }

    public void CleanupFilesOfGame(ZipFile zip, GameRom game, GameRomFilesList zipFiles)
    {
        if (!zipFiles.Any()) { return; }

        // get folders to remove
        var folders = zipFiles.Where(f => !string.IsNullOrEmpty(f.Path)).Select(f => f.Path).Distinct().ToList();

        // get the files that are in the zip but shouldn't
        var excessFiles = zipFiles.Where(zf => !game.RomFiles.Any(rf => rf.Name == zf.Name)).ToList(); // The ToList() clones the list so the zipFiles collection can be modified

        foreach (var f in excessFiles)
        {
            // remove them
            fs.DeleteZipFile(zip, f);

            zipFiles.RemoveFile(f.Name, f.Path);
        }

        // remove remaining folders
        foreach (var folder in folders)
        {
            fs.DeleteZipFile(zip, new GameRomFile { Name = folder.EndsWith('/') ? folder : $"{folder}/" });
        }
    }

    private async Task<CsvGamesList> GetCsvData(string csvfilter)
    {
        if (!string.IsNullOrEmpty(csvfilter) && fs.FileExists(csvfilter))
        {
            return await csvService.ReadFile(csvfilter, false);
        }

        return null;
    }

    private List<string> GetOtherFolderFiles(RomsActionCheckDat args, IMessageHandler messageHandler)
    {
        if (args.OtherFolder == args.Romset || !args.ChangeType)
        {
            return [];
        }

        messageHandler.Progress("Reading files in fix folder", 0, 0);
        return [.. fs.FilesGetList(args.OtherFolder, "*.zip").OrderBy(f => f)];
    }

    private List<ReadOnlyGameRomFile> GetOtherFolderFilesDetails(IEnumerable<string> fixFiles, RomsActionCheckDat args, IMessageHandler messageHandler)
    {
        if (messageHandler.MustCancel || !args.ChangeType) { return new([]); }

        List<ReadOnlyGameRomFile> result = [];

        foreach (var ff in fixFiles)
        {
            if (messageHandler.MustCancel) { break; }

            messageHandler.CurrentItem++;
            messageHandler.Progress($"Reading {fs.FileName(ff)} in additional folder");

            // read infos of files in zip
            result.AddRange(fs.GetZipFiles(ff, args.CheckSha1).Select(f => f.ToReadOnly(ff)));
        }

        return result;
    }

    private static int ComputeTotal(int filesCount, bool change, int fixFilesCount)
    {
        int total = filesCount;

        if (change)
        {
            total *= 2; // check, rebuild
        }

        // listing the files in the fix folder takes a lot of time
        total += fixFilesCount;

        return total;
    }

    private string GetDatPath(string datfilePre, string datfileCustom)
    {
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

    private static GameRomList KeepCsvGames(GameRomList allGames, CsvGamesList csv)
    {
        if (csv == null) { return allGames; }

        allGames.RemoveAll(gdat => csv.Games.Any(gcsv => gcsv.Name.Equals(gdat.Name, StringComparison.InvariantCultureIgnoreCase)));

        return allGames;
    }
}
