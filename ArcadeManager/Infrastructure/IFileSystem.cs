using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// Interface for file system access
/// </summary>
public interface IFileSystem {

    /// <summary>
    /// Creates the specified directory.
    /// </summary>
    /// <param name="path">The path.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Copies a directory
    /// </summary>
    /// <param name="sourceDirName">The source directory name</param>
    /// <param name="destDirName">The destination directory name</param>
    /// <param name="overwrite">Wether to overwrite existing files</param>
    /// <param name="copySubDirs">Whether to copy the sub-directories</param>
    /// <exception cref="DirectoryNotFoundException">
    /// Source directory does not exist or could not be found
    /// </exception>
    int DirectoryCopy(string sourceDirName, string destDirName, bool overwrite, bool copySubDirs);

    /// <summary>
    /// Checks if a directory exists
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>Whether the directory exists</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Gets the directory name
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The directory name</returns>
    string DirectoryName(string path);

    /// <summary>
    /// Computes a directory size
    /// </summary>
    /// <param name="directory">The path to the directory</param>
    /// <returns>The directory size</returns>
    long DirectorySize(string directory);

    /// <summary>
    /// Makes sure that a folder exists
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    void EnsureDirectory(string targetFolder);

    /// <summary>
    /// Checks if a path exists
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>Whether the path exists</returns>
    bool Exists(string path);

    /// <summary>
    /// Copies a file
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="dest">The destination path.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrite existing file.</param>
    void FileCopy(string source, string dest, bool overwrite);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    void FileDelete(string filePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>Whether the file exists</returns>
    bool FileExists(string path);

    /// <summary>
    /// Gets the file extension
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file extension</returns>
    string FileExtension(string path);

    /// <summary>
    /// Gets a file name
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file name</returns>
    string FileName(string path);

    /// <summary>
    /// Gets the file name without extension
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file name</returns>
    string FileNameWithoutExtension(string path);

    /// <summary>
    /// Reads a file content
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file content</returns>
    string FileRead(string path);

    /// <summary>
    /// Reads a file content asynchronously.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file content</returns>
    Task<string> FileReadAsync(string path);

    /// <summary>
    /// Gets the file size
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file size, in bytes</returns>
    long FileSize(string path);

    /// <summary>
    /// Writes a file content asynchronously
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="content">The content.</param>
    Task FileWriteAsync(string path, string content);

    /// <summary>
    /// Gets the path to a file or folder in the Data folder.
    /// </summary>
    /// <param name="paths">The paths parts.</param>
    /// <returns>The path to the file or folder</returns>
    string GetDataPath(params string[] paths);

    /// <summary>
    /// Gets the files in a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="pattern">The file matching pattern.</param>
    /// <returns>The list of files</returns>
    List<string> GetFiles(string path, string pattern);

    /// <summary>
    /// Makes a file size human-readable
    /// </summary>
    /// <param name="size">The source file size</param>
    /// <returns>The human-readable file size</returns>
    string HumanSize(long size);

    /// <summary>
    /// Checks if a path is a directory
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>true if the path is a directory ; otherwise, false</returns>
    bool IsDirectory(string path);

    /// <summary>
    /// Joins paths
    /// </summary>
    /// <param name="paths">The paths.</param>
    /// <returns>The joined paths</returns>
    string PathJoin(params string[] paths);

    /// <summary>
    /// Reads all the lines in a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file content</returns>
    string[] ReadAllLines(string path);
}