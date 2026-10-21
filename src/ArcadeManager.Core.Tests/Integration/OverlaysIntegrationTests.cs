using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models;
using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Integration;

public class OverlaysIntegrationTests : BaseIntegrationTest
{
    private readonly Overlays sut;

    public OverlaysIntegrationTests()
    {
        sut = new(downloader, fs, env, imageProcessor, retroarchOverlay, mameOverlay);
    }

    [IgnoreLocallyFact]
    public async Task Check_checks_overlays_configs()
    {
        // arrange
        var options = new CheckAction
        {
            OverlaysConfigFolder = GetTestPath("overlays", "retroarch", "configs"),
            RomsConfigFolder = GetTestPath("overlays", "retroarch", "roms"),
            AutoFix = false,
            ErrorMargin = 0,
            InputOverlayConfigPathInRomConfig = "/opt/retropie/configs/all/retroarch/overlay/arcade-realistic/"
        };

        // arrange: record messages
        var allmessages = new StringBuilder();
        A.CallTo(() => messageHandler.ProgressMessage(A<string>._)).Invokes((string msg) => allmessages.AppendLine(msg));

        // act
        await sut.Check(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        allmessages.ToString().Trim().Should().Be(@"005 - rom points to a non-existing overlay: generic_sega_bally_midway.cfg
005 - fixing screen: overlay file does not exist: generic_sega_bally_midway.cfg
10yard85 - image has wrong coordinates in config
1942 - image has wrong coordinates in config
20pacgal - image has wrong coordinates in config
25pacman - image has wrong coordinates in config
3wonders - image has wrong coordinates in config
88games - image has wrong coordinates in config");
    }

    [IgnoreLocallyFact]
    public async Task ConvertMameToRa_converts_mame_files_to_ra()
    {
        // arrange
        var options = new MameToRaAction
        {
            OutputRoms = GetTestPath("overlays", "mame2ra_files_output", "roms"),
            OutputOverlays = GetTestPath("overlays", "mame2ra_files_output", "overlays"),
            ScanBezelForScreenCoordinates = false,
            Source = GetTestPath("overlays", "mame_files")
        };

        CleanupFolder(options.OutputRoms, true);
        CleanupFolder(options.OutputOverlays, true);

        // act
        await sut.ConvertMameToRa(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("overlays", "mame2ra_files_output"), GetTestPath("overlays", "mame2ra_expected"));
    }

    [IgnoreLocallyFact]
    public async Task ConvertMameToRa_converts_mame_folders_to_ra()
    {
        // arrange
        var options = new MameToRaAction
        {
            OutputRoms = GetTestPath("overlays", "mame2ra_folders_output", "roms"),
            OutputOverlays = GetTestPath("overlays", "mame2ra_folders_output", "overlays"),
            ScanBezelForScreenCoordinates = false,
            Source = GetTestPath("overlays", "mame_folders")
        };

        CleanupFolder(options.OutputRoms, true);
        CleanupFolder(options.OutputOverlays, true);

        // act
        await sut.ConvertMameToRa(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("overlays", "mame2ra_folders_output"), GetTestPath("overlays", "mame2ra_expected"));
    }

    [IgnoreLocallyFact]
    public async Task ConvertMameToRa_converts_mame_to_ra_with_changed_resolution()
    {
        // arrange
        var options = new MameToRaAction
        {
            OutputRoms = GetTestPath("overlays", "mame2ra_resolution_output", "roms"),
            OutputOverlays = GetTestPath("overlays", "mame2ra_resolution_output", "overlays"),
            ScanBezelForScreenCoordinates = true,
            Source = GetTestPath("overlays", "mame_folders"),
            TargetResolution = "1280x720"
        };

        CleanupFolder(options.OutputRoms, true);
        CleanupFolder(options.OutputOverlays, true);

        // act
        await sut.ConvertMameToRa(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("overlays", "mame2ra_resolution_output"), GetTestPath("overlays", "mame2ra_resolution_expected"));
    }

    [IgnoreLocallyFact]
    public async Task ConvertRaToMame_converts_ra_to_mame()
    {
        // arrange
        var options = new RaToMameAction
        {
            SourceConfigs = GetTestPath("overlays", "retroarch", "configs"),
            SourceRoms = GetTestPath("overlays", "retroarch", "roms"),
            Output = GetTestPath("overlays", "ra2mame_output"),
            Zip = false
        };

        CleanupFolder(options.Output, true);

        // act
        await sut.ConvertRaToMame(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(options.Output, GetTestPath("overlays", "ra2mame_expected"));
    }

    [IgnoreLocallyFact]
    public async Task Download_downloads_overlays()
    {
        // arrange
        var options = new InstallOverlaysAction
        {
            Pack = "Realistic",
            ConfigFolder = GetTestPath("overlays", "download_output", "configs"),
            RomFolders = [GetTestPath("overlays", "download_output", "roms")],
            Width = 1024,
            Height = 720
        };

        CleanupFolder(GetTestPath("overlays", "download_output"), true);

        // arrange: copy (fake) roms folder
        CopyFolder(GetTestPath("overlays", "download"), options.RomFolders[0]);

        // arrange: appdata
        string appDataContent = await File.ReadAllTextAsync(Path.Join(basePath, "Data", "appdata.json"), TestContext.Current.CancellationToken);
        A.CallTo(() => env.GetAppData()).Returns(Serializer.Deserialize<AppData>(appDataContent));

        // arrange: os
        A.CallTo(() => env.GetSettingsOs()).Returns("Retropie");

        // act
        await sut.Download(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("overlays", "download_output"), GetTestPath("overlays", "download_expected"));
    }

    [IgnoreLocallyFact]
    public async Task Generate_generates_configs()
    {
        // arrange
        var options = new GenerateAction
        {
            ImagesFolder = GetTestPath("overlays", "generate_output", "images"),
            RomsFolder = GetTestPath("overlays", "generate_output", "roms")
        };

        CleanupFolder(options.RomsFolder, true);
        CleanupFolder(options.ImagesFolder, true);

        // arrange: copy source images in the output
        CopyFolder(GetTestPath("overlays", "generate_images"), options.ImagesFolder);

        // act
        await sut.Generate(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("overlays", "generate_output"), GetTestPath("overlays", "generate_expected"));
    }
}