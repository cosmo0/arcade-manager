using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Exceptions;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models;
using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Models.Bezels;
using ArcadeManager.Core.Models.Github;
using ArcadeManager.Core.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Services;

/// <summary>
/// The overlays service
/// </summary>
/// <seealso cref="IOverlays"/>
/// <remarks>
/// Initializes a new instance of the <see cref="Overlays"/> class.
/// </remarks>
/// <param name="downloaderService">The downloader service.</param>
/// <param name="fs">The file system.</param>
/// <param name="environment">The environment data</param>
public class Overlays(IDownloader downloaderService, IFileSystem fs, IEnvironment environment, IImageProcessor imageProcessor) : IOverlays
{
    private static readonly OperationCanceledException cancel = new("Operation cancelled");

    private static readonly string inputOverlayProperty = "input_overlay";

    private static readonly string templatesFolder = "templates";

    private readonly MameOverlay mameProcessor = new(fs);

    private readonly RetroArchOverlay raProcessor = new(fs);

    /// <summary>
    /// Checks the Retroarch configuration files.
    /// </summary>
    /// <param name="options">The options.</param>
    public async Task Check(CheckAction options, IMessageHandler messageHandler)
    {
        ConcurrentBag<Config> configs = [];

        messageHandler.ProgressInit("Check RetroArch config files");

        // list of items to process
        var romConfigs = fs.FilesGetList(options.RomsConfigFolder, "*.cfg");
        var configFiles = fs.FilesGetList(options.OverlaysConfigFolder, "*.cfg");
        var images = fs.FilesGetList(options.OverlaysConfigFolder, "*.png");
        var total = romConfigs.Count() + configFiles.Count() + images.Count();

        var current = 0;
        var errorsNb = 0;
        var fixedNb = 0;

        try
        {
            // read all the rom config files and check the ones that have an overlay defined
            foreach (var f in romConfigs)
            {
                current++;
                if (messageHandler.MustCancel) { return; }
                messageHandler.Progress($"Processing rom config {fs.FileName(f)}", total, current);

                (Config conf, int err, int fix) = await CheckOrFixRomConfigPaths(options, f, messageHandler);

                configs.Add(conf);
                errorsNb += err;
                fixedNb += fix;
            }

            // check overlay config files
            foreach (var f in configFiles)
            {
                current++;
                if (messageHandler.MustCancel) { return; }
                messageHandler.Progress($"Processing overlay config {fs.FileName(f)}", total, current);

                var (err, fix) = await CheckOrFixOverlayConfig(options, configs, f, messageHandler);
                errorsNb += err;
                fixedNb += fix;
            }

            // check that all images have an associated overlay config
            foreach (var f in images)
            {
                current++;
                if (messageHandler.MustCancel) { return; }
                messageHandler.Progress($"Processing image {fs.FileName(f)}", total, current);

                (int err, int fix) = await CheckOrFixImages(options, configs, f, messageHandler);
                errorsNb += err;
                fixedNb += fix;
            }

            // get list of roms configs, again (in case some have been created)
            romConfigs = fs.FilesGetList(options.RomsConfigFolder, "*.cfg");
            foreach (var f in romConfigs)
            {
                // TODO: total and current are now out of sync with what's happening because we may be processing new files
                if (messageHandler.MustCancel) { return; }

                messageHandler.Progress($"Processing config {fs.FileName(f)}", total, current);

                (int err, int fix) = await CheckOrFixRomConfigScreen(options, f, messageHandler);
                errorsNb += err;
                fixedNb += fix;
            }

            messageHandler.ProgressDone($"Processed {total} files: {errorsNb} errors, {fixedNb} fixed", options.RomsConfigFolder);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Converts MAME bezels to RetroArch overlays
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertMameToRa(MameToRaAction options, IMessageHandler messageHandler)
    {
        var fsEntries = fs.FilesGetList(options.Source, "*.*");

        messageHandler.ProgressInit("Convert MAME files to RetroArch");
        var total = fsEntries.Count();
        var current = 0;

        try
        {
            foreach (var f in fsEntries)
            {
                await ConvertMameFile(f, options, messageHandler, total, current++);
            }

            messageHandler.ProgressDone("Done", options.OutputRoms);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Converts the RetroArch overlays to MAME bezels
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task ConvertRaToMame(RaToMameAction options, IMessageHandler messageHandler)
    {
        // get files to process
        var romFiles = fs.FilesGetList(options.SourceRoms, "*.zip.cfg");

        messageHandler.ProgressInit("Convert MAME files to RetroArch");
        var total = romFiles.Count();
        var current = 0;

        try
        {
            foreach (var f in romFiles)
            {
                await ConvertRetroarchFile(f, options, messageHandler, total, current++);
            }

            messageHandler.ProgressDone("Done", options.Output);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Downloads an overlay pack
    /// </summary>
    /// <param name="data">The parameters</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <exception cref="FileNotFoundException">
    /// Unable to parse rom config {game} to find overlay (input_overlay) or Unable to parse overlay
    /// config {game} to find image (overlay0_overlay)
    /// </exception>
    public async Task Download(InstallOverlaysAction data, IMessageHandler messageHandler)
    {
        messageHandler.ProgressInit("Download overlay pack");

        try
        {
            var os = environment.GetSettingsOs();
            var pack = environment.GetAppData().Overlays.First(o => o.Name == data.Pack);

            // check if the destination of rom cfg is the rom folder
            var romCfgFolder = pack.Roms.Dest[os] == "roms"
                ? null // save rom cfg directly into rom folder(s)
                : fs.PathJoin(data.ConfigFolder, pack.Roms.Dest[os]); // save rom cfg in config folder

            // list the available rom configs
            messageHandler.Progress("list of files to download", 1, 100);
            var romConfigs = await downloaderService.ListFiles(pack.Repository, pack.Roms.Src);

            // download common files
            messageHandler.Progress("common files", 1, 100);
            if (pack.Common != null && !string.IsNullOrWhiteSpace(pack.Common.Src))
            {
                await DownloadCommon(pack, fs.PathJoin(data.ConfigFolder, pack.Common.Dest[os]), data.Overwrite, data.Ratio, messageHandler, 100, 1);
            }

            if (messageHandler.MustCancel) { throw cancel; }

            // check that there is a matching game in any of the roms folders
            messageHandler.Progress("games list to process", 1, 100);
            var romsToProcess = GetRomsToProcess(data.RomFolders, romConfigs.Tree);

            var total = romConfigs.Tree.Count;
            var current = 0;
            var installed = 0;

            foreach (var r in romsToProcess.OrderBy(r => r.Game))
            {
                installed += await DownloadOverlayForRom(total, current, r, pack, data, romCfgFolder, messageHandler);
            }

            messageHandler.ProgressDone($"Installed {installed} overlays", "");
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Generates overlays based on images.
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    public async Task Generate(GenerateAction options, IMessageHandler messageHandler)
    {
        messageHandler.ProgressInit("Generating rom config files based on images");

        var images = fs.FilesGetList(options.ImagesFolder, "*.png");
        var total = images.Count();
        var current = 0;
        var createdNb = 0;
        var errorsNb = 0;

        try
        {
            foreach (var f in images)
            {
                var (created, errors) = await GenerateConfigFile(options, messageHandler, f, total, current++);
                createdNb += created;
                errorsNb += errors;
            }

            messageHandler.ProgressDone($"{createdNb} game files created ; {errorsNb} errors", options.RomsFolder);
        }
        catch (Exception ex)
        {
            messageHandler.ProgressError(ex);
        }
    }

    /// <summary>
    /// Applies the specified offset to the specified bounds
    /// </summary>
    /// <param name="sourcePosition">The source screen position</param>
    /// <param name="offset">The offset to apply</param>
    /// <param name="sourceResolution">The source resolution</param>
    /// <param name="targetResolution">The target resolution</param>
    /// <returns>The new bounds</returns>
    private static Bounds ApplyOffset(Bounds sourcePosition, Offset offset, Bounds sourceResolution, Bounds targetResolution)
    {
        var newPos = sourcePosition.Clone();

        if (offset != null)
        {
            // multiply w/h by stretch = get target screen size, centered => NEW DIMENSIONS AT
            // SOURCE RESOLUTION
            newPos.Width *= offset.HStretch;
            newPos.Height *= offset.VStretch;

            // compute new base x/y (top/left): x = cx - (w / 2)
            newPos.X = sourcePosition.Center.X - (newPos.Width / 2);
            newPos.Y = sourcePosition.Center.Y - (newPos.Height / 2);

            // apply offset: x = x + ((hres / w * h) * hoffset) ; y = y + (vres * voffset) =>
            // NEW POSITION at source resolution
            if (offset.HOffset != 0)
            {
                if (newPos.Orientation == Orientation.Horizontal)
                {
                    newPos.X += (sourcePosition.Width / newPos.Width * newPos.Height) * offset.HOffset;
                }
                else
                {
                    newPos.X += sourcePosition.Width * offset.HOffset;
                }
            }

            if (offset.VOffset != 0)
            {
                if (newPos.Orientation == Orientation.Horizontal)
                {
                    newPos.Y += sourcePosition.Height * offset.VOffset;
                }
                else
                {
                    newPos.Y += (sourcePosition.Height / newPos.Height * newPos.Width) * offset.VOffset;
                }
            }
        }

        // apply target resolution => NEW COORDINATES AT TARGET RESOLUTION
        newPos.X *= targetResolution.Width / sourceResolution.Width;
        newPos.Y *= targetResolution.Height / sourceResolution.Height;
        newPos.Width *= targetResolution.Width / sourceResolution.Width;
        newPos.Height *= targetResolution.Height / sourceResolution.Height;

        return newPos;
    }

    /// <summary>
    /// Builds a regex string to get the specified key value
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The corresponding value</returns>
    private static string BuildCfgRegex(string key)
    {
        /// searched value looks like: key = "value" with or without spaces, with or without quotes,
        /// with or without trailing spaces
        return $"^{key}\\s*=\\s?\"?([^\"\\n]*)\"?\\s*$";
    }

    /// <summary>
    /// Changes the resolution in a file content.
    /// </summary>
    /// <param name="content">The file content.</param>
    /// <param name="ratio">The ratio.</param>
    /// <returns>The modified content</returns>
    private static string ChangeResolution(string content, float ratio)
    {
        if (Math.Abs(ratio - 1) < 0.001)
        {
            return content;
        }

        var parameters = new List<string> {
            "custom_viewport_width",
            "custom_viewport_height",
            "custom_viewport_x",
            "custom_viewport_y",
            "video_fullscreen_x",
            "video_fullscreen_y"
        };

        foreach (var p in parameters)
        {
            var regex = new Regex(BuildCfgRegex(p), RegexOptions.Multiline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            var foundValue = regex.Match(content).Groups[1].Value;
            if (double.TryParse(foundValue, out var value))
            {
                value = Math.Round(value * ratio, 0);
                content = regex.Replace(content, $"{p} = {(int)value}");
            }
        }

        return content;
    }

    private static bool CheckCoordinate(double a, double b, int margin)
    {
        return Math.Abs(a - b) <= margin;
    }

    /// <summary>
    /// Gets data from the specified config file.
    /// </summary>
    /// <param name="fileContent">The content of the file.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns>The config value</returns>
    private static string GetCfgData(string fileContent, string key)
    {
        var match = Regex.Match(fileContent, BuildCfgRegex(key), RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        if (match.Success && match.Captures.Count != 0)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private async Task<(int err, int fix)> CheckOrFixBounds(CheckAction options, string game, string f, string imagePath, string romContent, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var imageContent = await fs.FileReadBinaryAsync(imagePath);

        // get bounds
        var boundsInImage = imageProcessor.FindScreen(imageContent, 0);
        var boundsInConf = RetroArchOverlay.GetBoundsFromConfig(romContent);

        // make sure the bounds match
        if (!CheckCoordinate(boundsInImage.X, boundsInConf.X, options.ErrorMargin)
            || !CheckCoordinate(boundsInImage.Y, boundsInConf.Y, options.ErrorMargin)
            || !CheckCoordinate(boundsInImage.Width, boundsInConf.Width, options.ErrorMargin * 2)
            || !CheckCoordinate(boundsInImage.Height, boundsInConf.Height, options.ErrorMargin * 2))
        {
            boundsInImage = boundsInImage.ApplyMargin(options.Margin);

            if (!string.IsNullOrWhiteSpace(options.OutputDebug))
            {
                // output debug whether fixing or not
                if (boundsInConf.Width > 0 && boundsInConf.Height > 0)
                {
                    imageProcessor.DebugDraw($"{game}_conf", options.OutputDebug, imagePath, boundsInConf, options.TargetResolutionBounds);
                }

                imageProcessor.DebugDraw($"{game}_image", options.OutputDebug, imagePath, boundsInImage, null);
            }

            if (messageHandler.MustCancel) { throw cancel; }

            // fix the image
            if (options.AutoFix)
            {
                await raProcessor.SetBounds(f, game, boundsInImage, options.TargetResolutionBounds);
                messageHandler.ProgressMessage($"{game} - Fixed screen position in config");
                fix++;
            }
            else
            {
                messageHandler.ProgressMessage($"{game} - image has wrong coordinates in config");
                err++;
            }
        }

        return (err, fix);
    }

    private async Task<(int err, int fix)> CheckOrFixImages(CheckAction options, ConcurrentBag<Config> configs, string f, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var fileName = fs.FileName(f);
        var game = fileName.Replace(".png", "");

        // check that the image is used by an overlay
        var cfgEntry = configs.FirstOrDefault(c => c.Image != null && c.Image.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
        if (cfgEntry == null)
        {
            if (options.AutoFix)
            {
                if (messageHandler.MustCancel) { throw cancel; }

                var (err2, fix2) = await CreateConfigForOverlay(options, f, game, configs, messageHandler);
                err += err2;
                fix += fix2;
            }
            else
            {
                messageHandler.ProgressMessage($"{game} - image is not used by any overlay: {fileName}");
                err++;
            }
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // check that image is not too large
        var imgSize = imageProcessor.GetSize(f);
        if (imgSize.Width > options.TargetResolutionBounds.Width || imgSize.Height > options.TargetResolutionBounds.Height)
        {
            if (options.AutoFix)
            {
                messageHandler.ProgressMessage($"{game} - resizing image (previous size: {imgSize.Width}x{imgSize.Height})");
                imageProcessor.Resize(f, (int)options.TargetResolutionBounds.Width, (int)options.TargetResolutionBounds.Height);
                fix++;
            }
            else
            {
                messageHandler.ProgressMessage($"{game} - image has wrong size: {imgSize.Width}x{imgSize.Height}");
                err++;
            }
        }

        return (err, fix);
    }

    private async Task<(int err, int fix)> CheckOrFixOverlayConfig(CheckAction options, ConcurrentBag<Config> configs, string f, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var fileName = fs.FileName(f);
        var game = fileName.Replace(".cfg", "");

        var cfgContent = await fs.FileReadAsync(f);
        var overlayFileName = RetroArchOverlay.GetCfgData(cfgContent, "overlay0_overlay");

        // check that the overlay is used
        var cfgEntry = configs.FirstOrDefault(c => c.Overlay != null && c.Overlay.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
        if (cfgEntry == null)
        {
            if (options.AutoFix)
            {
                (Config cfg, int err2, int fix2) = await CreateRomConfig(options, game, fileName, overlayFileName, messageHandler);
                err += err2;
                fix += fix2;
                if (cfg != null) cfgEntry = cfg;
            }
            else
            {
                messageHandler.ProgressMessage($"{game} - overlay is not used by any game");
            }
        }

        // check that the image exists
        if (!fs.FileExists(fs.PathJoin(options.OverlaysConfigFolder, overlayFileName)))
        {
            messageHandler.ProgressMessage($"{game} - overlay points to a non-existing image: {overlayFileName}");
            err++;
        }
        else
        {
            if (cfgEntry == null)
            {
                configs.Add(new Config { Overlay = fileName, Image = overlayFileName });
            }
        }

        return (err, fix);
    }

    private async Task<(Config conf, int err, int fix)> CheckOrFixRomConfigPaths(CheckAction options, string f, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var fileName = fs.FileName(f);
        var game = fileName.Replace(".zip.cfg", "").Replace(".cfg", "");
        var romConfEntry = new Config() { Rom = fileName };

        var cfgContent = await fs.FileReadAsync(f);
        var overlayPath = RetroArchOverlay.GetCfgData(cfgContent, inputOverlayProperty);

        // no overlay image
        if (string.IsNullOrWhiteSpace(overlayPath))
        {
            messageHandler.ProgressMessage($"{game} - rom has no overlay config");
            err++;
            return (romConfEntry, err, fix);
        }

        // make sure a Windows path is converted to Unix under *nix, and vice versa
        var overlayFileName = fs.FileName(RetroArchOverlay.NormalizePath(overlayPath));

        // check that there is an matching overlay file at the expected localtion
        if (fs.FileExists(fs.PathJoin(options.OverlaysConfigFolder, overlayFileName)))
        {
            romConfEntry.Overlay = overlayFileName;
        }
        else
        {
            messageHandler.ProgressMessage($"{game} - rom points to a non-existing overlay: {overlayFileName}");
            err++;
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // check that the path in the rom config is valid
        if (!string.IsNullOrEmpty(options.InputOverlayConfigPathInRomConfig))
        {
            var separator = options.InputOverlayConfigPathInRomConfig.EndsWith('/') ? "" : "/";
            var overlayShouldBe = $"{options.InputOverlayConfigPathInRomConfig}{separator}{overlayFileName}";
            if (!overlayPath.Equals(overlayShouldBe, StringComparison.InvariantCultureIgnoreCase))
            {
                if (options.AutoFix)
                {
                    cfgContent = RetroArchOverlay.SetCfgData(cfgContent, inputOverlayProperty, overlayShouldBe);

                    await fs.FileWriteAsync(f, cfgContent);

                    messageHandler.ProgressMessage($"{game} - fixed overlay path in rom config: {overlayShouldBe}");
                    fix++;
                }
                else
                {
                    messageHandler.ProgressMessage($"{game} - rom has a wrong overlay path: {overlayPath}");
                    err++;
                }
            }
        }

        return (romConfEntry, err, fix);
    }

    private async Task<(int err, int fix)> CheckOrFixRomConfigScreen(CheckAction options, string f, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var fileName = fs.FileName(f);
        var game = fileName.Replace(".cfg", "").Replace(".zip", "");

        // get overlay file name
        var romContent = await fs.FileReadAsync(f);
        var overlayFileName = RetroArchOverlay.GetCfgData(romContent, inputOverlayProperty);
        if (string.IsNullOrWhiteSpace(overlayFileName))
        {
            messageHandler.ProgressMessage($"{game} - fixing screen: rom config doesn't have an input_overlay");
            err++;

            return (err, fix);
        }

        var overlayFile = fs.FileName(RetroArchOverlay.NormalizePath(overlayFileName));
        var overlayPath = fs.PathJoin(options.OverlaysConfigFolder, overlayFile);

        if (!fs.FileExists(overlayPath))
        {
            messageHandler.ProgressMessage($"{game} - fixing screen: overlay file does not exist: {overlayPath}");
            err++;

            return (err, fix);
        }

        var overlayContent = await fs.FileReadAsync(overlayPath);
        var imageFile = RetroArchOverlay.GetCfgData(overlayContent, "overlay0_overlay");
        var imagePath = fs.PathJoin(options.OverlaysConfigFolder, imageFile);

        if (!fs.FileExists(imagePath))
        {
            messageHandler.ProgressMessage($"{game} - fixing screen: image file does not exist: {imagePath}");
            err++;

            return (err, fix);
        }

        if (messageHandler.MustCancel) { throw cancel; }

        (int err2, int fix2) = await CheckOrFixBounds(options, game, f, imagePath, romContent, messageHandler);
        err += err2;
        fix += fix2;

        return (err, fix);
    }

    /// <summary>
    /// Processes a Mame file
    /// </summary>
    /// <param name="fsEntry">The file system entry.</param>
    /// <param name="options">The options</param>
    private async Task ConvertMameFile(string fsEntry, MameToRaAction options, IMessageHandler messageHandler, int total, int current)
    {
        var isFolder = fs.IsDirectory(fsEntry);
        var entryName = isFolder ? fs.DirectoryName(fsEntry) : fs.FileName(fsEntry);

        // don't process files that are not zip
        if (!isFolder && !entryName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        var game = entryName.Replace(".zip", "", StringComparison.InvariantCultureIgnoreCase);

        var cfgFile = string.IsNullOrEmpty(options.SourceConfigs) ? string.Empty : fs.PathJoin(options.SourceConfigs, $"{game}.cfg");

        messageHandler.Progress($"{game} processing start", total, current);

        if (messageHandler.MustCancel) { throw cancel; }

        var (lay, cfg, bezel) = isFolder
            ? await mameProcessor.MameReadFiles(game, fsEntry, cfgFile, options)
            : await mameProcessor.ExtractFiles(game, fsEntry, cfgFile, options);

        // extracts the data from the MAME files
        var mameBezel = MameOverlay.MameGetBezel(options, lay, cfg);

        // resize the bezel image
        bezel = imageProcessor.Resize(
            bezel,
            (int)options.TargetResolutionBounds.Width,
            (int)options.TargetResolutionBounds.Height);

        Bounds newPosition = ConvertPosition(options, bezel, mameBezel);

        if (newPosition.Width <= 0 || newPosition.Height <= 0)
        {
            messageHandler.ProgressMessage($"{game} - Width/height of screen are invalid: {newPosition}");
            return;
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // get bezel image
        var outputImage = fs.PathJoin(options.OutputOverlays, $"{game}.png");
        if (options.Overwrite && fs.FileExists(outputImage))
        {
            fs.FileDelete(outputImage);
        }

        if (options.Overwrite || !fs.FileExists(outputImage))
        {
            await fs.FileWriteBinaryAsync(outputImage, bezel);
        }

        // debug: draw target position
        imageProcessor.DebugDraw(game, options.OutputDebug, outputImage, newPosition, options.TargetResolutionBounds);

        if (messageHandler.MustCancel) { throw cancel; }

        // create game config files
        var outputGameCfg = fs.PathJoin(options.OutputRoms, $"{game}.zip.cfg");
        fs.FileCopy(fs.GetDataPath(templatesFolder, "game.cfg"), outputGameCfg, options.Overwrite);
        await raProcessor.FillTemplate(outputGameCfg, game, newPosition, options.TargetResolutionBounds);

        if (messageHandler.MustCancel) { throw cancel; }

        // create overlay config files
        var outputOverlayCfg = fs.PathJoin(options.OutputOverlays, $"{game}.cfg");
        fs.FileCopy(fs.GetDataPath(templatesFolder, "overlay.cfg"), outputOverlayCfg, options.Overwrite);
        await raProcessor.FillTemplate(outputOverlayCfg, game, newPosition, options.TargetResolutionBounds);
    }

    private Bounds ConvertPosition(MameToRaAction options, byte[] bezel, MameBezel mameBezel)
    {
        Bounds newPosition;
        if (options.ScanBezelForScreenCoordinates)
        {
            // scan image for transparent pixels
            newPosition = imageProcessor.FindScreen(bezel, options.Margin);
        }
        else
        {
            // convert from LAY and CFG
            newPosition = ApplyOffset(
                                mameBezel.SourceScreenPosition,
                                mameBezel.Offset,
                                mameBezel.SourceResolution,
                                options.TargetResolutionBounds);
        }

        return newPosition;
    }

    /// <summary>
    /// Processes the Retroarch file.
    /// </summary>
    /// <param name="romFile">The rom config file.</param>
    /// <param name="options">The options.</param>
    private async Task ConvertRetroarchFile(string romFile, RaToMameAction options, IMessageHandler messageHandler, int total, int current)
    {
        var romFileName = fs.FileName(romFile);
        var game = romFileName.Replace(".zip.cfg", "");

        messageHandler.Progress($"{game} processing start", total, current);

        var target = fs.PathJoin(options.Output, game);

        if (messageHandler.MustCancel) { throw cancel; }

        // get RA processor
        var processor = await raProcessor.GetRetroArchConfig(romFile, options);

        Bounds newPosition;
        if (options.ScanBezelForScreenCoordinates)
        {
            newPosition = imageProcessor.FindScreen(await fs.FileReadBinaryAsync(processor.OverlayImagePath), options.Margin);
        }
        else
        {
            // convert from LAY and CFG
            newPosition = processor.SourceScreenPosition;
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // create destination folder
        if (options.Overwrite && fs.DirectoryExists(target)) { fs.DirectoryDelete(target, true); }
        if (!fs.DirectoryExists(target)) { fs.DirectoryCreate(target); }

        // copy overlay image
        fs.FileCopy(processor.OverlayImagePath, fs.PathJoin(target, processor.OverlayImageFileName), options.Overwrite);

        // resize the bezel image
        imageProcessor.Resize(
            processor.OverlayImagePath,
            (int)processor.SourceResolution.Width,
            (int)processor.SourceResolution.Height);

        if (messageHandler.MustCancel) { throw cancel; }

        // debug: draw target position
        imageProcessor.DebugDraw(game, options.OutputDebug, processor.OverlayImagePath, newPosition, options.TargetResolutionBounds);

        if (messageHandler.MustCancel) { throw cancel; }

        // create lay file
        var outputLay = fs.PathJoin(target, "default.lay");
        fs.FileCopy(fs.GetDataPath(templatesFolder, "default.lay"), outputLay, options.Overwrite);
        await raProcessor.FillTemplate(outputLay, game, newPosition, processor.SourceResolution);

        if (messageHandler.MustCancel) { throw cancel; }

        // zip overlay
        if (options.Zip)
        {
            var targetZip = fs.PathJoin(options.Output, $"{game}.zip");
            if (fs.FileExists(targetZip)) { fs.FileDelete(targetZip); }
            fs.CompressFolderContent(target, targetZip);
            fs.DirectoryDelete(target, true);
        }
    }

    private async Task<(int err, int fix)> CreateConfigForOverlay(CheckAction options, string f, string game, ConcurrentBag<Config> configs, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;

        var cfgFilesName = $"{game}.cfg";
        var dest = fs.PathJoin(options.OverlaysConfigFolder, cfgFilesName);
        if (fs.FileExists(dest))
        {
            messageHandler.ProgressMessage($"{game} - trying to create overlay {dest} but file already exists");
            err++;
        }
        else
        {
            messageHandler.ProgressMessage($"{game} - creating overlay config for orphan image at {dest}");
            await raProcessor.CreateConfig(fs.GetDataPath("templates", "overlay.cfg"), game, dest, null, options.TargetResolutionBounds);

            var romDest = fs.PathJoin(options.RomsConfigFolder, cfgFilesName);
            messageHandler.ProgressMessage($"{game} - creating rom config for orphan image at {romDest}");

            // create the config
            var bounds = imageProcessor.FindScreen(await fs.FileReadBinaryAsync(f), options.Margin);
            await raProcessor.CreateConfig(fs.GetDataPath("templates", "game.cfg"), game, romDest, bounds, options.TargetResolutionBounds);

            configs.Add(new Config { Rom = cfgFilesName, Overlay = cfgFilesName, Image = fs.FileName(f) });

            fix++;
        }

        return (err, fix);
    }

    private async Task<(Config cfg, int err, int fix)> CreateRomConfig(CheckAction options, string game, string fileName, string overlayFileName, IMessageHandler messageHandler)
    {
        int err = 0;
        int fix = 0;
        Config cfgEntry = null;

        // create a rom
        var romFile = fileName.Replace(".cfg", ".zip.cfg");
        var dest = fs.PathJoin(options.RomsConfigFolder, romFile);
        if (fs.FileExists(dest))
        {
            messageHandler.ProgressMessage($"{game} - overlay matches rom file but is not used by it: {romFile}");
            err++;
        }
        else
        {
            messageHandler.ProgressMessage($"{game} - creating rom config file for unused overlay at {dest}");

            if (messageHandler.MustCancel) { throw cancel; }

            var imgFileName = fs.PathJoin(options.OverlaysConfigFolder, overlayFileName);
            if (fs.FileExists(imgFileName))
            {
                // get bounds
                var img = await fs.FileReadBinaryAsync(imgFileName);
                var bounds = imageProcessor.FindScreen(img, options.Margin);

                await raProcessor.CreateConfig(fs.GetDataPath("templates", "game.cfg"), game, dest, bounds, options.TargetResolutionBounds);

                cfgEntry = new Config { Rom = romFile, Overlay = fileName, Image = overlayFileName };

                fix++;
            }
            else
            {
                messageHandler.ProgressMessage($"{game} - overlay points to a non-existing image: {imgFileName}");
                err++;
            }
        }

        return (cfgEntry, err, fix);
    }

    /// <summary>
    /// Downloads the common files.
    /// </summary>
    /// <param name="pack">The overlay pack to download.</param>
    /// <param name="destination">The destination path.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrite existing files.</param>
    /// <param name="ratio">The ratio to change resolution.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="current">The current item number.</param>
    private async Task DownloadCommon(OverlayBundle pack, string destination, bool overwrite, float ratio, IMessageHandler messageHandler, int total, int current)
    {
        if (messageHandler.MustCancel) { throw cancel; }

        if (pack.Common != null && !string.IsNullOrEmpty(pack.Common.Src))
        {
            IEnumerable<string> files = await downloaderService.DownloadFolder(pack.Repository, pack.Common.Src, destination, overwrite, (entry) =>
            {
                messageHandler.Progress($"downloading {entry.Path}", total, current);
            });

            foreach (var f in files.Where(f => f.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase)))
            {
                var fi = fs.FileName(f);
                messageHandler.Progress($"fixing {fi}", total, current);

                if (overwrite || fs.FileExists(f))
                {
                    var content = await fs.FileReadAsync(f);

                    content = ChangeResolution(content, ratio);
                    content = FixPaths(content, pack);

                    await fs.FileWriteAsync(f, content);
                }
            }
        }
    }

    private async Task<int> DownloadOverlayForRom(int total, int current, RomToProcess rom, OverlayBundle pack, InstallOverlaysAction data, string romCfgFolder, IMessageHandler messageHandler)
    {
        if (messageHandler.MustCancel) { throw cancel; }

        current++;

        var game = rom.Game;

        messageHandler.Progress($"{game}: download overlay (rom config)", total, current);

        // download the rom config and extract the overlay file name
        var (romConfigContent, installed) = await GetRomConfigContent(pack, rom, data, romCfgFolder, messageHandler);

        if (messageHandler.MustCancel) { throw cancel; }

        messageHandler.Progress($"{game}: download overlay (config)", total, current);

        // extract the overlay file name
        var overlayPath = GetCfgData(romConfigContent, inputOverlayProperty);
        if (string.IsNullOrWhiteSpace(overlayPath)) { throw new PathNotFoundException($"Unable to parse rom config {game} to find overlay (input_overlay)"); }
        var overlayFileName = fs.FileName(overlayPath);
        var overlayConfigDest = fs.PathJoin(data.ConfigFolder, overlayFileName);

        // download the overlay file name and extract the image file name
        var overlayConfigContent = await GetOverlayConfigContent(data, pack, overlayConfigDest, overlayFileName, messageHandler);

        if (messageHandler.MustCancel) { throw cancel; }

        messageHandler.Progress($"{game}: download overlay (image)", total, current);

        // extract the image file name
        var imagePath = GetCfgData(overlayConfigContent, "overlay0_overlay");
        if (string.IsNullOrWhiteSpace(imagePath)) { throw new PathNotFoundException($"Unable to parse overlay config {game} to find image (overlay0_overlay)"); }
        var imageFi = fs.FileName(imagePath);
        var imageDest = fs.PathJoin(data.ConfigFolder, imageFi);

        // download the image
        if (data.Overwrite || !fs.FileExists(imageDest))
        {
            if (messageHandler.MustCancel) { throw cancel; }

            await downloaderService.DownloadFile(pack.Repository, $"{pack.Overlays.Src}/{imageFi}", imageDest);
        }

        return installed;
    }

    /// <summary>
    /// Fixes the paths in a config.
    /// </summary>
    /// <param name="content">The config content.</param>
    /// <param name="pack">The pack to download.</param>
    /// <returns>The fixed content</returns>
    private string FixPaths(string content, OverlayBundle pack)
    {
        if (environment.GetSettingsOs() != pack.BaseOs)
        {
            return content.Replace(pack.Base[pack.BaseOs], pack.Base[environment.GetSettingsOs()], StringComparison.InvariantCultureIgnoreCase);
        }

        return content;
    }

    private async Task<(int created, int errors)> GenerateConfigFile(GenerateAction options, IMessageHandler messageHandler, string f, int total, int current)
    {
        var fileName = fs.FileName(f);
        var game = fileName.Replace(".png", "");
        var errorsNb = 0;
        var createdNb = 0;

        messageHandler.Progress($"{game} generating config", total, current);

        if (messageHandler.MustCancel) { throw cancel; }

        // resize
        imageProcessor.Resize(f, (int)options.TargetResolutionBounds.Width, (int)options.TargetResolutionBounds.Height);

        // get data from image
        var bounds = imageProcessor.FindScreen(await fs.FileReadBinaryAsync(f), options.Margin);

        // generate config
        var config = fs.PathJoin(options.ImagesFolder, $"{game}.cfg");
        if (!options.Overwrite && fs.FileExists(config))
        {
            messageHandler.ProgressMessage($"{game} - config file already exists: {config}");
            errorsNb++;
        }
        else
        {
            fs.FileDelete(config);
            await raProcessor.CreateConfig(fs.GetDataPath(templatesFolder, "overlay.cfg"), game, config, bounds, options.TargetResolutionBounds);
            messageHandler.ProgressMessage($"{game} - created config: {config}");
            createdNb++;
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // generate rom
        var rom = fs.PathJoin(options.RomsFolder, $"{game}.zip.cfg");
        if (!options.Overwrite && fs.FileExists(rom))
        {
            messageHandler.ProgressMessage($"{game} - rom config file already exists: {rom}");
            errorsNb++;
        }
        else
        {
            fs.FileDelete(rom);
            await raProcessor.CreateConfig(fs.GetDataPath(templatesFolder, "game.cfg"), game, rom, bounds, options.TargetResolutionBounds);
            messageHandler.ProgressMessage($"{game} - created rom config file: {rom}");
            createdNb++;
        }

        if (messageHandler.MustCancel) { throw cancel; }

        // debug
        if (!string.IsNullOrWhiteSpace(options.OutputDebug))
        {
            imageProcessor.DebugDraw($"{game}_image", options.OutputDebug, f, bounds, options.TargetResolutionBounds);
        }

        return (createdNb, errorsNb);
    }

    private async Task<string> GetOverlayConfigContent(InstallOverlaysAction data, OverlayBundle pack, string overlayConfigDest, string overlayFileName, IMessageHandler messageHandler)
    {
        string overlayConfigContent;
        if (data.Overwrite || !fs.FileExists(overlayConfigDest))
        {
            if (messageHandler.MustCancel) { throw cancel; }

            overlayConfigContent = await downloaderService.DownloadFileText(pack.Repository, $"{pack.Overlays.Src}/{overlayFileName}");

            // fix path
            overlayConfigContent = FixPaths(overlayConfigContent, pack);

            await fs.FileWriteAsync(overlayConfigDest, overlayConfigContent);
        }
        else
        {
            overlayConfigContent = await fs.FileReadAsync(overlayConfigDest);
        }

        return overlayConfigContent;
    }

    private async Task<(string Content, int Installed)> GetRomConfigContent(OverlayBundle pack, RomToProcess rom, InstallOverlaysAction data, string romCfgFolder, IMessageHandler messageHandler)
    {
        var romConfigContent = string.Empty;
        var installed = 0;
        foreach (var romFolder in rom.TargetFolder)
        {
            if (messageHandler.MustCancel) { throw cancel; }

            var romConfigFile = fs.PathJoin(romCfgFolder ?? romFolder, $"{rom.Game}{rom.Extension}.cfg");

            // get rom config content
            if (data.Overwrite || !fs.FileExists(romConfigFile))
            {
                // file doesn't exist or we'll overwrite it
                romConfigContent = await downloaderService.DownloadFileText(pack.Repository, $"{pack.Roms.Src}/{rom.Game}{rom.Extension}.cfg");

                // fix resolution and paths
                romConfigContent = ChangeResolution(romConfigContent, data.Ratio);
                romConfigContent = FixPaths(romConfigContent, pack);
            }
            else
            {
                // file exist, we don"t overwrite: read it
                romConfigContent = await fs.FileReadAsync(romConfigFile);
            }

            // write rom config
            if (data.Overwrite || !fs.FileExists(romConfigFile))
            {
                if (messageHandler.MustCancel) { throw cancel; }

                await fs.FileWriteAsync(romConfigFile, romConfigContent);
                installed++;
            }
        }

        return (romConfigContent, installed);
    }

    /// <summary>
    /// Gets the list of roms to process and their folder(s).
    /// </summary>
    /// <param name="romFolders">The rom folders.</param>
    /// <param name="entries">The available entries.</param>
    /// <returns>The roms to process</returns>
    private List<RomToProcess> GetRomsToProcess(string[] romFolders, IEnumerable<GithubTree.Entry> entries)
    {
        var result = new List<RomToProcess>();

        // list all the folders (arcade, fba, mame...)
        foreach (var folder in romFolders)
        {
            // get all rom files
            var files = fs.FilesGetList(folder, "*.zip").ToList();
            files.AddRange(fs.FilesGetList(folder, "*.7z"));
            foreach (var fi in files)
            {
                var game = fs.FileNameWithoutExtension(fi);
                var directoryName = fs.DirectoryName(fi);
                var extension = fs.FileExtension(fi);

                // only process files that are in the overlays pack
                if (entries.Any(e => e.Path.Equals($"{game}.zip.cfg", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var existing = result.FirstOrDefault(r => r.Game != null && r.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase));
                    if (existing != null)
                    {
                        existing.TargetFolder.Add(directoryName);
                    }
                    else
                    {
                        result.Add(new RomToProcess
                        {
                            Game = game,
                            TargetFolder = [directoryName],
                            Extension = extension
                        });
                    }
                }
            }
        }

        return result;
    }

    private sealed class Config
    {
        public string Image;
        public string Overlay;
        public string Rom;
    }

    /// <summary>
    /// A rom to process
    /// </summary>
    [DebuggerDisplay("{Game} -> {TargetFolder}")]
    private sealed class RomToProcess
    {
        /// <summary>
        /// Gets or sets the rom file extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the name of the game.
        /// </summary>
        public string Game { get; set; }

        /// <summary>
        /// Gets or sets the folders in which the game is present.
        /// </summary>
        public List<string> TargetFolder { get; set; } = [];
    }
}