using ArcadeManager.Core.Infrastructure.Interfaces;

namespace ArcadeManager.Core.Infrastructure;

/// <summary>
/// Factory for the web client
/// </summary>
public class WebClientFactory : IWebClientFactory {

    /// <summary>
    /// Gets a new web client.
    /// </summary>
    /// <returns>The new web client</returns>
    public IArcadeManagerWebClient GetWebClient() {
        return new ArcadeManagerWebClient();
    }
}