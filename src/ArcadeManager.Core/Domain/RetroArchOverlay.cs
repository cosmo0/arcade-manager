using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Models.Bezels;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public class RetroArchOverlay(IFileSystem fs)
{
    /// <summary>
    /// Gets the bounds written in a config file
    /// </summary>
    /// <param name="fileContent">The content of the file</param>
    /// <returns>The rom file content</returns>
    public static Bounds GetBoundsFromConfig(string fileContent)
    {
        if (int.TryParse(GetCfgData(fileContent, "custom_viewport_width"), out int width)
            && int.TryParse(GetCfgData(fileContent, "custom_viewport_height"), out int height)
            && int.TryParse(GetCfgData(fileContent, "custom_viewport_x"), out int x)
            && int.TryParse(GetCfgData(fileContent, "custom_viewport_y"), out int y))
        {
            return new Bounds
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }

        return new Bounds { X = 0, Y = 0, Width = 0, Height = 0 };
    }

    /// <summary>
    /// Gets data from the specified config file.
    /// </summary>
    /// <param name="fileContent">The content of the file.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns>The config value</returns>
    public static string GetCfgData(string fileContent, string key)
    {
        var match = Regex.Match(fileContent, BuildCfgRegex(key), RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        if (match.Success && match.Captures.Count != 0)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    /// <summary>
    /// Normalizes a path so it's understandable by System.IO.Path
    /// </summary>
    /// <param name="path">The path to normalize</param>
    /// <returns>The normalized path</returns>
    public static string NormalizePath(string path)
    {
        // RetroArch has this weird syntax where ":\" means "at the RA root"
        path = path.StartsWith(':') ? path[1..] : path;

        // Windows handles *nix slashes fine, the opposite is not true
        path = path.Replace("\\", "/");

        return path;
    }

    /// <summary>
    /// Sets a value in the specified config file
    /// </summary>
    /// <param name="fileContent">The contents of the file</param>
    /// <param name="key">The key to set</param>
    /// <returns>The modified content</returns>
    public static string SetCfgData(string fileContent, string key, string value)
    {
        var r = BuildCfgRegex(key);
        var v = $"{key} = {value}";

        // if it exists: replace value
        if (Regex.IsMatch(fileContent, r, RegexOptions.Multiline, TimeSpan.FromSeconds(1)))
        {
            return Regex.Replace(fileContent, r, v, RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        }

        // if it doesn't exist: add value
        return $"{fileContent}\n{v}";
    }

    /// <summary>
    /// Creates a config file
    /// </summary>
    /// <param name="templatePath">The path to the template</param>
    /// <param name="game">The game name</param>
    /// <param name="dest">The destination path</param>
    /// <param name="bounds">The screen bounds</param>
    /// <param name="resolution">The target resolution</param>
    public async Task CreateConfig(string templatePath, string game, string dest, Bounds bounds, Bounds resolution)
    {
        fs.FileCopy(templatePath, dest, false);
        await FillTemplate(dest, game, null, null);

        if (bounds != null)
        {
            await SetBounds(dest, game, bounds, resolution);
        }
    }

    /// <summary>
    /// Fill a template config with the specified values
    /// </summary>
    /// <param name="configPath">The path to the config file to fill</param>
    /// <param name="game">The game name</param>
    /// <param name="position">The position of the image</param>
    /// <param name="resolution">The target resolution</param>
    public async Task FillTemplate(string configPath, string game, Bounds position, Bounds resolution)
    {
        var content = await fs.FileReadAsync(configPath);

        content = FillTemplateContent(content, game, position, resolution);

        await fs.FileWriteAsync(configPath, content);
    }

    public async Task<RetroArchBezel> GetRetroArchConfig(string romFile, RaToMameAction options)
    {
        // get rom file content
        var romFileContent = await fs.FileReadAsync(romFile);

        // get overlay content
        var overlayCfgFileSourcePath = GetCfgData(romFileContent, "input_overlay");
        var overlayCfgFileName = fs.FileName(overlayCfgFileSourcePath);
        var overlayCfgFilePath = fs.PathJoin(options.SourceConfigs, overlayCfgFileName);
        var overlayCfgFileContent = await fs.FileReadAsync(overlayCfgFilePath);

        // extract data from configs
        var overlayImageFileName = GetCfgData(overlayCfgFileContent, "overlay0_overlay");
        var screenBounds = GetBoundsFromConfig(romFileContent);

        var xres = GetCfgData(romFileContent, "video_fullscreen_x");
        var yres = GetCfgData(romFileContent, "video_fullscreen_y");

        var resolution = new Bounds
        {
            X = 0,
            Y = 0,
            Width = int.Parse(xres ?? options.TargetResolutionBounds.Width.ToString()),
            Height = int.Parse(yres ?? options.TargetResolutionBounds.Height.ToString())
        };

        return new RetroArchBezel
        {
            OverlayImageFileName = overlayImageFileName,
            OverlayImagePath = fs.PathJoin(options.SourceConfigs, overlayImageFileName),
            SourceScreenPosition = screenBounds,
            SourceResolution = resolution
        };
    }

    /// <summary>
    /// Sets the bounds in the specified config
    /// </summary>
    /// <param name="filePath">The path to the file to set</param>
    /// <param name="bounds">The bounds to set</param>
    /// <returns>The modified config</returns>
    public async Task SetBounds(string filePath, string game, Bounds bounds, Bounds resolution)
    {
        var fileContent = await fs.FileReadAsync(filePath);

        fileContent = SetCfgData(fileContent, "custom_viewport_width", bounds.Width.ToString());
        fileContent = SetCfgData(fileContent, "custom_viewport_height", bounds.Height.ToString());
        fileContent = SetCfgData(fileContent, "custom_viewport_x", bounds.X.ToString());
        fileContent = SetCfgData(fileContent, "custom_viewport_y", bounds.Y.ToString());

        // fill placeholders
        fileContent = FillTemplateContent(fileContent, game, bounds, resolution);

        await fs.FileWriteAsync(filePath, fileContent);
    }

    private static string BuildCfgRegex(string key)
    {
        /// searched value looks like:
        /// key = "value"
        /// with or without spaces, with or without quotes, with or without trailing spaces
        return $"^{key}\\s*=\\s?\"?([^\"\\n]*)\"?\\s*$";
    }

    /// <summary>
    /// Fills the specified config content template with the specified infos
    /// </summary>
    /// <param name="content">The content to fill</param>
    /// <param name="game">The game name</param>
    /// <param name="position">The screen position</param>
    /// <param name="resolution">The target resolution</param>
    /// <returns>The filled content</returns>
    private static string FillTemplateContent(string content, string game, Bounds position, Bounds resolution)
    {
        content = content.Replace("{{game}}", game);

        if (position != null)
        {
            content = content.Replace("{{width}}", Math.Round(position.Width, 0).ToString())
                .Replace("{{height}}", Math.Round(position.Height, 0).ToString())
                .Replace("{{x}}", Math.Round(position.X, 0).ToString())
                .Replace("{{y}}", Math.Round(position.Y, 0).ToString())
                .Replace("{{orientation}}", position.Orientation.ToString().ToLower());
        }

        if (resolution != null)
        {
            content = content
                .Replace("{{width_res}}", Math.Round(resolution.Width, 0).ToString())
                .Replace("{{height_res}}", Math.Round(resolution.Height, 0).ToString());
        }

        return content;
    }
}