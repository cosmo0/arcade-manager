using ArcadeManager.Core;
using ArcadeManager.Core.Models.Actions.Overlays;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Services.Interfaces;

/// <summary>
/// Interface for the overlays service
/// </summary>
public interface IOverlays
{
    /// <summary>
    /// Checks that overlays are properly configured
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    Task Check(CheckAction options, IMessageHandler messageHandler);

    /// <summary>
    /// Converts MAME bezels to RetroArch overlays
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    Task ConvertMameToRa(MameToRaAction options, IMessageHandler messageHandler);

    /// <summary>
    /// Converts the RetroArch overlays to MAME bezels
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    Task ConvertRaToMame(RaToMameAction options, IMessageHandler messageHandler);

    /// <summary>
    /// Downloads an overlay pack
    /// </summary>
    /// <param name="data">The parameters</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <returns></returns>
    Task Download(InstallOverlaysAction data, IMessageHandler messageHandler);

    /// <summary>
    /// Generates overlays based on images.
    /// </summary>
    /// <param name="options">The arguments.</param>
    /// <param name="messageHandler">The message handler.</param>
    Task Generate(GenerateAction options, IMessageHandler messageHandler);
}