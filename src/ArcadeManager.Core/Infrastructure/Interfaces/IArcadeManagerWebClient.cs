using System;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Infrastructure.Interfaces;

/// <summary>
/// Interface for a custom web client
/// </summary>
/// <seealso cref="IDisposable"/>
public interface IArcadeManagerWebClient : IDisposable {

    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="url">The file URL.</param>
    /// <param name="localPath">The local download path.</param>
    Task DownloadFile(string url, string localPath);

    /// <summary>
    /// Gets the bytes of an URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URL bytes</returns>
    Task<byte[]> GetBytes(string url);

    /// <summary>
    /// Gets the string of an URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URL string</returns>
    Task<string> GetString(string url);
}