using System.Collections.Generic;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// Interface for file system access
/// </summary>
public interface IFileSystem {

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
    /// Gets the file name without extension
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file name</returns>
    string FileNameWithoutExtension(string path);

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
    /// Reads all the lines in a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file content</returns>
    string[] ReadAllLines(string path);
}