namespace ArcadeManager.Infrastructure;

/// <summary>
/// Interface for a web client factory
/// </summary>
public interface IWebClientFactory {

    /// <summary>
    /// Gets a new web client.
    /// </summary>
    /// <returns>The new web client</returns>
    IArcadeManagerWebClient GetWebClient();
}