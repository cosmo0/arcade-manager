using ArcadeManager.Core.Models.Bezels;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

/// <summary>
/// Interface for the RetroArch overlay processor
/// </summary>
public interface IRetroArchOverlay
{
    /// <summary>
    /// Creates a config file
    /// </summary>
    /// <param name="templatePath">The path to the template</param>
    /// <param name="game">The game name</param>
    /// <param name="dest">The destination path</param>
    /// <param name="bounds">The screen bounds</param>
    /// <param name="resolution">The target resolution</param>
    Task CreateConfig(string templatePath, string game, string dest, Bounds bounds, Bounds resolution);

    /// <summary>
    /// Fill a template config with the specified values
    /// </summary>
    /// <param name="configPath">The path to the config file to fill</param>
    /// <param name="game">The game name</param>
    /// <param name="position">The position of the image</param>
    /// <param name="resolution">The target resolution</param>
    Task FillTemplate(string configPath, string game, Bounds position, Bounds resolution);

    /// <summary>
    /// Gets the bounds written in a config file
    /// </summary>
    /// <param name="fileContent">The content of the file</param>
    /// <returns>The rom file content</returns>
    Bounds GetBoundsFromConfig(string fileContent);

    /// <summary>
    /// Gets data from the specified config file.
    /// </summary>
    /// <param name="fileContent">The content of the file.</param>
    /// <param name="key">The key to look for.</param>
    /// <returns>The config value</returns>
    string GetCfgData(string fileContent, string key);

    /// <summary>
    /// Generates a RetroArch config based on some options
    /// </summary>
    /// <param name="romFile">The rom file.</param>
    /// <param name="sourceConfigs">The source configs folder.</param>
    /// <param name="targetResolution">The target resolution.</param>
    /// <returns>
    /// The RetroArch config
    /// </returns>
    Task<RetroArchBezel> GetRetroArchConfig(string romFile, string sourceConfigs, Bounds targetResolution);

    /// <summary>
    /// Normalizes a path so it's understandable by System.IO.Path
    /// </summary>
    /// <param name="path">The path to normalize</param>
    /// <returns>The normalized path</returns>
    string NormalizePath(string path);

    /// <summary>
    /// Sets the bounds in the specified config
    /// </summary>
    /// <param name="filePath">The path to the file to set</param>
    /// <param name="bounds">The bounds to set</param>
    /// <returns>The modified config</returns>
    Task SetBounds(string filePath, string game, Bounds bounds, Bounds resolution);

    /// <summary>
    /// Sets a value in the specified config file
    /// </summary>
    /// <param name="fileContent">The contents of the file</param>
    /// <param name="key">The key to set</param>
    /// <returns>The modified content</returns>
    string SetCfgData(string fileContent, string key, string value);
}