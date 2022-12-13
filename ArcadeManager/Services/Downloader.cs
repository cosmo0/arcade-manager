using ArcadeManager.Actions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Services;

/// <summary>
/// The downloader service
/// </summary>
/// <seealso cref="ArcadeManager.Services.IDownloader"/>
public class Downloader : IDownloader {
    private const string api = "api.github.com";
    private const string protocol = "https:";
    private const string raw = "raw.githubusercontent.com";
    private readonly IFileSystem fs;
    private readonly IWebClientFactory webclientfactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Downloader"/> class.
    /// </summary>
    /// <param name="webclientfactory">The webclient factory.</param>
    /// <param name="fs">The file system infrastructure.</param>
    public Downloader(IWebClientFactory webclientfactory, IFileSystem fs) {
        this.webclientfactory = webclientfactory;
        this.fs = fs;
    }

    /// <summary>
    /// Downloads the specified URL in the Github API.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="path">The path to the file.</param>
    /// <returns>The URL content</returns>
    public async Task<string> DownloadApiUrl(string repository, string path) {
        var url = $"{protocol}//{api}/repos/{repository}/{path}";

        using (var wc = webclientfactory.GetWebClient()) {
            return await wc.GetString(url);
        }
    }

    /// <summary>
    /// Downloads a binary file
    /// </summary>
    /// <param name="repository">The repository</param>
    /// <param name="filePath">The file path</param>
    /// <param name="localPath">The local file path to save</param>
    public async Task DownloadFile(string repository, string filePath, string localPath) {
        var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

        using (var wc = webclientfactory.GetWebClient()) {
            await wc.DownloadFile(url, localPath);
        }
    }

    /// <summary>
    /// Gets the contents of a file.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file contents</returns>
    public async Task<byte[]> DownloadFile(string repository, string filePath) {
        var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

        using (var wc = webclientfactory.GetWebClient()) {
            return await wc.GetBytes(url);
        }
    }

    /// <summary>
    /// Downloads a JSON file and deserializes it to T
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The downloaded file</returns>
    public async Task<T> DownloadFile<T>(string repository, string filePath) {
        var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

        using (var wc = webclientfactory.GetWebClient()) {
            return Serializer.Deserialize<T>(await wc.GetString(url));
        }
    }

    /// <summary>
    /// Downloads the content of a text file.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file contents</returns>
    public async Task<string> DownloadFileText(string repository, string filePath) {
        var url = $"{protocol}//{raw}/{repository}/master/{filePath}";

        using (var wc = webclientfactory.GetWebClient()) {
            return await wc.GetString(url);
        }
    }

    /// <summary>
    /// Downloads the specified folder.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="folder">The folder path.</param>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites existing files.</param>
    /// <param name="progress">A method called when a file is downloaded.</param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> DownloadFolder(string repository, string folder, string targetFolder, bool overwrite, Action<GithubTree.Entry> progress) {
        fs.EnsureDirectory(targetFolder);

        var result = new List<string>();

        var tree = await ListFiles(repository, folder);
        foreach (var item in tree.Tree) {
            progress?.Invoke(item);

            if (item.IsFile) {
                var target = Path.Join(targetFolder, item.Path);
                if (overwrite || !File.Exists(target)) {
                    await DownloadFile(repository, $"{folder}/{item.Path}", target);
                }

                result.Add(target);
            }
            else {
                result.AddRange(await DownloadFolder(repository, $"{folder}/{item.Path}", Path.Join(targetFolder, item.Path), overwrite, progress));
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the list of available CSV files in the specified repository folder
    /// </summary>
    /// <param name="data">The download parameters</param>
    /// <returns>The list of files</returns>
    public async Task<IEnumerable<CsvFile>> GetList(DownloadAction data) {
        // get the content of the JSON file that lists the possible CSV files and their descriptions
        var descriptor = await DownloadFile<CsvFilesList>(data.repository, data.details);

        // list the actual files in the folder
        var files = await ListFiles(data.repository, data.folder);

        // return the files that match in both lists
        return descriptor.files.Where(d => files.Tree.Any((f) => f.Path == d.filename));
    }

    /// <summary>
    /// Lists the files in a Github folder
    /// </summary>
    /// <param name="repository">The repository</param>
    /// <param name="folder">The folder path</param>
    /// <returns>The list of files</returns>
    public async Task<GithubTree> ListFiles(string repository, string folder) {
        // get level-up folder to get the SHA of the folder - easy to access but limited to 1000
        // files docs.github.com/en/rest/reference/repos#get-repository-content
        var urlUpFolders = $"{protocol}//{api}/repos/{repository}/contents/{folder.Substring(0, folder.LastIndexOf("/"))}";

        using (var wc = webclientfactory.GetWebClient()) {
            var data = Serializer.Deserialize<IEnumerable<GithubContent>>(await wc.GetString(urlUpFolders));

            // get SHA of the folder we're insterested in
            var sha = data.Where(d => d.path == folder).Select(d => d.sha).First();

            // get files list through git/tree, which has a much higher limit of items - docs.github.com/en/rest/reference/git#get-a-tree
            var filesListUrl = $"{protocol}//{api}/repos/{repository}/git/trees/{sha}";
            return Serializer.Deserialize<GithubTree>(await wc.GetString(filesListUrl));
        }
    }
}