using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ArcadeManager.Models;
using ArcadeManager.Models.Roms;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// Interface for file system access
/// </summary>
public interface IFileSystem {

    /// <summary>
    /// Creates the specified directory.
    /// </summary>
    /// <param name="path">The path.</param>
    void DirectoryCreate(string path);

    /// <summary>
    /// Removes all files in the specified directory
    /// </summary>
    /// <param name="path">The path to the directory</param>
    void DirectoryEmpty(string path);

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
    void DirectoryEnsure(string targetFolder);

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
    /// Moves a file to a folder
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <param name="toFolder">The path to the folder to move it to</param>
    void FileMove(string filePath, string toFolder);

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
    List<string> FilesGetList(string path, string pattern);

    /// <summary>
    /// Gets the invalid file name characters.
    /// </summary>
    /// <returns>The invalid file name characters</returns>
    char[] GetInvalidFileNameChars();

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

    /// <summary>
    /// Reads a file using a stream.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="action">The action to execute on the file stream.</param>
    Task ReadFileStream(string path, Func<StreamReader, Task> action);

    /// <summary>
    /// Writes in a file using a stream.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="action">The action to execute on the stream writer.</param>
    Task WriteFileStream(string path, Func<StreamWriter, Task> action);

    /// <summary>
    /// Lists the files inside a zip
    /// </summary>
    /// <param name="zip">The zip archive</param>
    /// <param name="fileName">The file name of the zip</param>
    /// <param name="folder">The folder of the zip</param>
    /// <param name="getSha1">Whether to get the SHA1 hash of the file</param>
    /// <returns>The zip file infos</returns>
    IEnumerable<GameRomFile> GetZipFiles(ZipArchive zip, string fileName, string folder, bool getSha1);

    /// <summary>
    /// Lists the files inside a zip
    /// </summary>
    /// <param name="path">The path to the zip file</param>
    /// <param name="getSha1">Whether to get the SHA1 hash of the file</param>
    /// <returns>The zip file infos</returns>
    IEnumerable<GameRomFile> GetZipFiles(string path, bool getSha1);

    /// <summary>
    /// Opens a zip in read mode
    /// </summary>
    /// <param name="path">The path of the zip to open</param>
    /// <returns>The zip archive data</returns>
    ZipArchive OpenZipRead(string path);

    /// <summary>
    /// Opens a zip in write mode (create or update)
    /// </summary>
    /// <param name="path">The path of the zip to open</param>
    /// <returns>The zip archive data</returns>
    ZipArchive OpenZipWrite(string path);

    /// <summary>
    /// Replaces a file in a zip with another file
    /// </summary>
    /// <param name="source">The source zip to read the file from</param>
    /// <param name="target">The target zip to write to</param>
    /// <param name="file">The file to replace</param>
    /// <returns>A value indicating whether the file has been replaced</returns>
    Task<bool> ReplaceZipFile(ZipArchive source, ZipArchive target, IGameRomFile file);

    /// <summary>
    /// Deletes a file in a zip
    /// </summary>
    /// <param name="zip">The zip file</param>
    /// <param name="file">A file to delete</param>
    void DeleteZipFile(ZipArchive zip, IGameRomFile file);
}