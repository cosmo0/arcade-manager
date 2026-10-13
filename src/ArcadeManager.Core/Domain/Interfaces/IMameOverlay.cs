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
    /// <param name="useFirstView">if set to <c>true</c> use the first found view in the file.</param>
    /// <returns>
    /// The deserialized LAY and CFG files
    /// </returns>
    /// <exception cref="LayFileException">Unable to find a view in the LAY file</exception>
    Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> ReadZip(string game, string zipFile, string cfgFile, bool useFirstView);

    /// <summary>
    /// Factory for the MAME processor
    /// </summary>
    /// <param name="lay">The LAY file</param>
    /// <param name="cfg">The CFG file</param>
    /// <param name="useFirstView">if set to <c>true</c> use the first found view in the file.</param>
    /// <returns>
    /// The processor
    /// </returns>
    MameBezel GetBezel(MameLayFile lay, MameCfgFile cfg, bool useFirstView);

    /// <summary>
    /// Reads the files in the specified folder
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="folder">The folder to read</param>
    /// <param name="cfgFile">The config file</param>
    /// <param name="useFirstView">if set to <c>true</c> use the first found view in the file.</param>
    /// <returns>
    /// The parsed files
    /// </returns>
    Task<(MameLayFile lay, MameCfgFile cfg, byte[] bezel)> ReadFolder(string game, string folder, string cfgFile, bool useFirstView);
}