using System.Threading.Tasks;

namespace ArcadeManager.Services;

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
    Task<Models.GithubRelease> CheckUpdate(string currentVersion);
}