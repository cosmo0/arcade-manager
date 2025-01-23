using ArcadeManager.Exceptions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Services;

/// <summary>
/// The overlays service
/// </summary>
/// <seealso cref="ArcadeManager.Services.IOverlays"/>
public class Overlays : IOverlays {
    private readonly IDownloader downloaderService;
    private readonly IFileSystem fs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Overlays"/> class.
    /// </summary>
    /// <param name="downloaderService">The downloader service.</param>
    /// <param name="fs">The file system.</param>
    public Overlays(IDownloader downloaderService, IFileSystem fs) {
        this.downloaderService = downloaderService;
        this.fs = fs;
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
    public async Task Download(Actions.OverlaysAction data, IMessageHandler messageHandler) {
        messageHandler.Init("Download overlay pack");

        try {
            var os = ArcadeManagerEnvironment.SettingsOs;
            var pack = ArcadeManagerEnvironment.AppData.Overlays.First(o => o.Name == data.pack);

            // check if the destination of rom cfg is the rom folder
            var romCfgFolder = pack.Roms.Dest[os] == "roms"
                ? null // save rom cfg directly into rom folder(s)
                : fs.PathJoin(data.configFolder, pack.Roms.Dest[os]); // save rom cfg in config folder

            // list the available rom configs
            messageHandler.Progress("list of files to download", 1, 100);
            var romConfigs = await downloaderService.ListFiles(pack.Repository, pack.Roms.Src);

            // download common files
            messageHandler.Progress("common files", 1, 100);
            if (pack.Common != null && !string.IsNullOrWhiteSpace(pack.Common.Src)) {
                await DownloadCommon(pack, fs.PathJoin(data.configFolder, pack.Common.Dest[os]), data.overwrite, data.ratio, messageHandler, 100, 1);
            }

            if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

            // check that there is a matching game in any of the roms folders
            messageHandler.Progress("games list to process", 1, 100);
            var romsToProcess = GetRomsToProcess(data.romFolders, romConfigs.Tree);

            var total = romConfigs.Tree.Count;
            var current = 0;
            var installed = 0;

            foreach (var r in romsToProcess.OrderBy(r => r.Game)) {
                if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                current++;

                var game = r.Game;

                messageHandler.Progress($"{game}: download overlay (rom config)", total, current);

                // download the rom config and extract the overlay file name
                var romConfigContent = string.Empty;
                foreach (var romFolder in r.TargetFolder) {
                    if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                    var romConfigFile = fs.PathJoin(romCfgFolder ?? romFolder, $"{game}{r.Extension}.cfg");

                    // get rom config content
                    if (string.IsNullOrEmpty(romConfigContent)) {
                        if (data.overwrite || !fs.FileExists(romConfigFile)) {
                            // file doesn't exist or we'll overwrite it
                            romConfigContent = await downloaderService.DownloadFileText(pack.Repository, $"{pack.Roms.Src}/{game}{r.Extension}.cfg");

                            // fix resolution and paths
                            romConfigContent = ChangeResolution(romConfigContent, data.ratio);
                            romConfigContent = FixPaths(romConfigContent, pack);
                        }
                        else {
                            // file exist, we don"t overwrite: read it
                            romConfigContent = await fs.FileReadAsync(romConfigFile);
                        }
                    }

                    // write rom config
                    if (data.overwrite || !fs.FileExists(romConfigFile)) {
                        if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                        await fs.FileWriteAsync(romConfigFile, romConfigContent);
                        installed++;
                    }
                }

                if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                messageHandler.Progress($"{game}: download overlay (config)", total, current);

                // extract the overlay file name
                var overlayPath = GetCfgData(romConfigContent, "input_overlay");
                if (string.IsNullOrWhiteSpace(overlayPath)) { throw new PathNotFoundException($"Unable to parse rom config {game} to find overlay (input_overlay)"); }
                var overlayFi = fs.FileName(overlayPath);
                var overlayConfigDest = fs.PathJoin(data.configFolder, overlayFi);

                // download the overlay file name and extract the image file name
                var overlayConfigContent = string.Empty;
                if (data.overwrite || !fs.FileExists(overlayConfigDest)) {
                    if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                    overlayConfigContent = await downloaderService.DownloadFileText(pack.Repository, $"{pack.Overlays.Src}/{overlayFi}");

                    // fix path
                    overlayConfigContent = FixPaths(overlayConfigContent, pack);

                    await fs.FileWriteAsync(overlayConfigDest, overlayConfigContent);
                }
                else {
                    overlayConfigContent = await fs.FileReadAsync(overlayConfigDest);
                }

                if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                messageHandler.Progress($"{game}: download overlay (image)", total, current);

                // extract the image file name
                var imagePath = GetCfgData(overlayConfigContent, "overlay0_overlay");
                if (string.IsNullOrWhiteSpace(imagePath)) { throw new PathNotFoundException($"Unable to parse overlay config {game} to find image (overlay0_overlay)"); }
                var imageFi = fs.FileName(imagePath);
                var imageDest = fs.PathJoin(data.configFolder, imageFi);

                // download the image
                if (data.overwrite || !fs.FileExists(imageDest)) {
                    if (messageHandler.MustCancel) { throw new OperationCanceledException("Operation cancelled"); }

                    await downloaderService.DownloadFile(pack.Repository, $"{pack.Overlays.Src}/{imageFi}", imageDest);
                }
            }

            messageHandler.Done($"Installed {installed} overlays", null);
        }
        catch (Exception ex) {
            messageHandler.Error(ex);
        }
    }

    /// <summary>
    /// Builds a regex string to get the specified key value
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The corresponding value</returns>
    private static string BuildCfgRegex(string key) {
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
    private static string ChangeResolution(string content, float ratio) {
        if (ratio == 1) {
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

        foreach (var p in parameters) {
            var regex = new Regex(BuildCfgRegex(p), RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var foundValue = regex.Match(content).Groups[1].Value;
            if (double.TryParse(foundValue, out var value)) {
                value = Math.Round(value * ratio, 0);
                content = regex.Replace(content, $"{p} = {(int)value}");
            }
        }

        return content;
    }

    /// <summary>
    /// Fixes the paths in a config.
    /// </summary>
    /// <param name="content">The config content.</param>
    /// <param name="pack">The pack to download.</param>
    /// <returns>The fixed content</returns>
    private static string FixPaths(string content, OverlayBundle pack) {
        if (ArcadeManagerEnvironment.SettingsOs != pack.BaseOs) {
            return content.Replace(pack.Base[pack.BaseOs], pack.Base[ArcadeManagerEnvironment.SettingsOs], StringComparison.InvariantCultureIgnoreCase);
        }

        return content;
    }

    /// <summary>
    /// Gets data from the specified config file.
    /// </summary>
    /// <param name="fileContent">The content of the file.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns>The config value</returns>
    private static string GetCfgData(string fileContent, string key) {
        var match = Regex.Match(fileContent, BuildCfgRegex(key), RegexOptions.Multiline);
        if (match.Success && match.Captures.Any()) {
            return match.Groups[1].Value.Trim();
        }

        return null;
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
    private async Task DownloadCommon(OverlayBundle pack, string destination, bool overwrite, float ratio, IMessageHandler messageHandler, int total, int current) {
        if (messageHandler.MustCancel) { return; }

        if (pack.Common != null && !string.IsNullOrEmpty(pack.Common.Src)) {
            IEnumerable<string> files = await downloaderService.DownloadFolder(pack.Repository, pack.Common.Src, destination, overwrite, (entry) => {
                messageHandler.Progress($"downloading {entry.Path}", total, current);
            });

            foreach (var f in files.Where(f => f.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))) {
                var fi = fs.FileName(f);
                messageHandler.Progress($"fixing {fi}", total, current);

                if (overwrite || fs.FileExists(f)) {
                    var content = await fs.FileReadAsync(f);

                    content = ChangeResolution(content, ratio);
                    content = FixPaths(content, pack);

                    await fs.FileWriteAsync(f, content);
                }
            }
        }
    }

    /// <summary>
    /// Gets the list of roms to process and their folder(s).
    /// </summary>
    /// <param name="romFolders">The rom folders.</param>
    /// <param name="entries">The available entries.</param>
    /// <returns>The roms to process</returns>
    private IEnumerable<RomToProcess> GetRomsToProcess(string[] romFolders, IEnumerable<GithubTree.Entry> entries) {
        var result = new List<RomToProcess>();

        // list all the folders (arcade, fba, mame...)
        foreach (var folder in romFolders) {
            // get all rom files
            var files = fs.GetFiles(folder, "*.zip");
            files.AddRange(fs.GetFiles(folder, "*.7z"));
            foreach (var fi in files) {
                var game = fs.FileNameWithoutExtension(fi);
                var directoryName = fs.DirectoryName(fi);
                var extension = fs.FileExtension(fi);

                // only process files that are in the overlays pack
                if (entries.Any(e => e.Path.Equals($"{game}.zip.cfg", StringComparison.InvariantCultureIgnoreCase))) {
                    var existing = result.FirstOrDefault(r => r.Game.Equals(game, StringComparison.InvariantCultureIgnoreCase));
                    if (existing != null) {
                        existing.TargetFolder.Add(directoryName);
                    }
                    else {
                        result.Add(new RomToProcess {
                            Game = game,
                            TargetFolder = new List<string> { directoryName },
                            Extension = extension
                        });
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// A rom to process
    /// </summary>
    [DebuggerDisplay("{Game} -> {TargetFolder}")]
    private sealed class RomToProcess {

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
        public List<string> TargetFolder { get; set; } = new();
    }
}