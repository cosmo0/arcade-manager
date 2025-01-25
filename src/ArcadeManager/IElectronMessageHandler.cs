using System;
using System.Threading.Tasks;

namespace ArcadeManager;

/// <summary>
/// Interface for message handlers
/// </summary>
public interface IElectronMessageHandler : IMessageHandler
{
    /// <summary>
    /// Handles the messages for the specified window.
    /// </summary>
    /// <param name="window">The window.</param>
    Task Handle(ElectronNET.API.BrowserWindow window);
}