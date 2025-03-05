using ArcadeManager.Core;
using ArcadeManager.Core.Actions;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Services.Interfaces;

/// <summary>
/// Interface for the overlays service
/// </summary>
public interface IOverlays {

    /// <summary>
    /// Downloads an overlay pack
    /// </summary>
    /// <param name="data">The parameters</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <returns></returns>
    Task Download(OverlaysAction data, IMessageHandler messageHandler);
}