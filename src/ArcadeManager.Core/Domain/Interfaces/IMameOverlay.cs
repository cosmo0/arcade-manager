using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Models.Bezels;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public interface IMameOverlay
{
    /// <summary>
    /// Extracts and deserializes the LAY and CFG files
    /// </summary>
    /// <param name="game">The game name.</param>
    /// <param name="zipFile">The zip file path.</param>
    /// <param name="cfgFile">The CFG file path.</param>
    /// <param name="tmpFolder">The temporary folder path.</param>
    /// <returns>The deserialized LAY and CFG files</returns>
    /// <exception cref="LayFileException">
    /// Unable to find a view in the LAY file
    /// </exception>
    Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> ExtractFiles(string game, string zipFile, string cfgFile, MameToRaAction options);

    /// <summary>
    /// Factory for the MAME processor
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="lay">The LAY file</param>
    /// <param name="cfg">The CFG file</param>
    /// <returns>The processor</returns>
    MameBezel MameGetBezel(MameToRaAction options, MameLayFile lay, MameCfgFile cfg);

    /// <summary>
    /// Reads the files in the specified folder
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="folder">The folder to read</param>
    /// <param name="cfgFile">The config file</param>
    /// <param name="options">The options</param>
    /// <returns>The parsed files</returns>
    Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> MameReadFiles(string game, string folder, string cfgFile, MameToRaAction options);
}