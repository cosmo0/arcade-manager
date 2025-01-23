using ArcadeManager.Actions;
using ArcadeManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcadeManager.Services;

/// <summary>
/// Interface for the downloader
/// </summary>
public interface IDownloader {

    /// <summary>
    /// Downloads the specified URL in the Github API.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="path">The path to the file.</param>
    /// <returns>The URL content</returns>
    Task<string> DownloadApiUrl(string repository, string path);

    /// <summary>
    /// Downloads a binary file
    /// </summary>
    /// <param name="repository">The repository</param>
    /// <param name="filePath">The file path</param>
    /// <param name="localPath">The local file path to save</param>
    /// <returns></returns>
    Task DownloadFile(string repository, string filePath, string localPath);

    /// <summary>
    /// Gets the contents of a file.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file contents</returns>
    Task<byte[]> DownloadFile(string repository, string filePath);

    /// <summary>
    /// Downloads a JSON file and deserializes it to T
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The downloaded file</returns>
    Task<T> DownloadFile<T>(string repository, string filePath);

    /// <summary>
    /// Downloads the content of a text file.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file contents</returns>
    Task<string> DownloadFileText(string repository, string filePath);

    /// <summary>
    /// Downloads the specified folder.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="folder">The folder path.</param>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites existing files.</param>
    /// <param name="progress">A method called when a file is downloaded.</param>
    /// <returns></returns>
    Task<IEnumerable<string>> DownloadFolder(string repository, string folder, string targetFolder, bool overwrite, Action<GithubTree.Entry> progress);

    /// <summary>
    /// Returns the list of available CSV files in the specified repository folder
    /// </summary>
    /// <param name="data">The download parameters</param>
    /// <returns>The list of files</returns>
    Task<IEnumerable<CsvFile>> GetList(DownloadAction data);

    /// <summary>
    /// Lists the files in a Github folder
    /// </summary>
    /// <param name="repository">The repository</param>
    /// <param name="folder">The folder path</param>
    /// <returns>The list of files</returns>
    Task<GithubTree> ListFiles(string repository, string folder);
}