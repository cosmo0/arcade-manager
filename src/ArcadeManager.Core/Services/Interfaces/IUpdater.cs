using System.Threading.Tasks;
using ArcadeManager.Core.Models.Github;

namespace ArcadeManager.Core.Services.Interfaces;

/// <summary>
/// Interface for the app updater
/// </summary>
public interface IUpdater {

    /// <summary>
    /// Checks for app updates.
    /// </summary>
    /// <param name="currentVersion">The current version.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <returns>The new release details, if any</returns>
    Task<GithubRelease> CheckUpdate(string currentVersion);
}