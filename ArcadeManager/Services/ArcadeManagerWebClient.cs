using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcadeManager.Services;

/// <summary>
/// Custom web client
/// </summary>
public class ArcadeManagerWebClient : IDisposable {
    private readonly HttpClient client;
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcadeManagerWebClient"/> class.
    /// </summary>
    public ArcadeManagerWebClient() {
        this.client = new HttpClient();

        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        this.client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("arcade-manager", version));
        this.client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("(+https://github.com/cosmo0/arcade-manager)"));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="url">The file URL.</param>
    /// <param name="localPath">The local download path.</param>
    public async Task DownloadFile(string url, string localPath) {
        var bytes = await this.client.GetByteArrayAsync(url);
        File.WriteAllBytes(localPath, bytes);
    }

    /// <summary>
    /// Gets the bytes of an URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URL bytes</returns>
    public async Task<byte[]> GetBytes(string url) {
        return await this.client.GetByteArrayAsync(url);
    }

    /// <summary>
    /// Gets the string of an URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URL string</returns>
    public async Task<string> GetString(string url) {
        return await this.client.GetStringAsync(url);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    /// unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                this.client.Dispose();
            }

            disposedValue = true;
        }
    }
}