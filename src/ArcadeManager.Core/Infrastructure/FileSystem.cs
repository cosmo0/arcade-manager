using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArcadeManager.Models;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// File system utilities
/// </summary>
public class FileSystem(IEnvironment environment) : IFileSystem
{

    /// <summary>
    /// Creates the specified directory.
    /// </summary>
    /// <param name="path">The path.</param>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

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
    /// <remarks>From docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories</remarks>
    public int DirectoryCopy(string sourceDirName, string destDirName, bool overwrite, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        int nbCopied = 0;

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string targetPath = Path.Combine(destDirName, file.Name);
            if (!File.Exists(targetPath) || overwrite)
            {
                file.CopyTo(targetPath, overwrite);
                nbCopied++;
            }
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                nbCopied += DirectoryCopy(subdir.FullName, tempPath, overwrite, copySubDirs);
            }
        }

        return nbCopied;
    }

    /// <summary>
    /// Checks if a directory exists
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>Whether the directory exists</returns>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Gets the directory name
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The directory name</returns>
    public string DirectoryName(string path)
    {
        return new FileInfo(path).DirectoryName ?? string.Empty;
    }

    /// <summary>
    /// Computes a directory size
    /// </summary>
    /// <param name="directory">The path to the directory</param>
    /// <returns>The directory size</returns>
    /// <remarks>From stackoverflow.com/a/468131/6776</remarks>
    public long DirectorySize(string directory)
    {
        var d = new DirectoryInfo(directory);

        long size = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis)
        {
            size += fi.Length;
        }

        // Add subdirectory sizes.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            size += DirectorySize(di.FullName);
        }

        return size;
    }

    /// <summary>
    /// Makes sure that a folder exists
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    public void EnsureDirectory(string targetFolder)
    {
        if (string.IsNullOrEmpty(targetFolder)) {
            return;
        }
        
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
    }

    /// <summary>
    /// Checks if a path exists
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>Whether the path exists</returns>
    public bool Exists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return IsDirectory(path)
            ? Directory.Exists(path)
            : File.Exists(path);
    }

    /// <summary>
    /// Copies a file
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="dest">The destination path.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrite existing file.</param>
    public void FileCopy(string source, string dest, bool overwrite)
    {
        if (!File.Exists(dest) || overwrite)
        {
            File.Copy(source, dest, overwrite);
        }
    }

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public void FileDelete(string filePath)
    {
        File.Delete(filePath);
    }

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>Whether the file exists</returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Gets the file extension
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file extension</returns>
    public string FileExtension(string path)
    {
        return new FileInfo(path).Extension;
    }

    /// <summary>
    /// Gets a file name
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file name</returns>
    public string FileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Gets the file name without extension
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file name</returns>
    public string FileNameWithoutExtension(string path)
    {
        var filename = Path.GetFileName(path);
        filename = filename[..filename.IndexOf('.')];
        return filename;
    }

    /// <summary>
    /// Reads a file content
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file content</returns>
    public string FileRead(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Reads a file content asynchronously.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file content</returns>
    public async Task<string> FileReadAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    /// <summary>
    /// Gets the file size
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file size, in bytes</returns>
    public long FileSize(string path)
    {
        return new FileInfo(path).Length;
    }

    /// <summary>
    /// Writes a file content asynchronously
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="content">The content.</param>
    /// <returns></returns>
    public async Task FileWriteAsync(string path, string content)
    {
        await File.WriteAllTextAsync(path, content);
    }

    /// <summary>
    /// Gets the path to a file or folder in the Data folder.
    /// </summary>
    /// <param name="paths">The paths parts.</param>
    /// <returns>The path to the file or folder</returns>
    public string GetDataPath(params string[] paths)
    {
        paths = (new string[] { environment.GetBasePath(), "Data" }).Concat(paths).ToArray();
        return Path.Combine(paths);
    }

    /// <summary>
    /// Gets the files in a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="pattern">The file matching pattern.</param>
    /// <returns>The list of files</returns>
    public List<string> GetFiles(string path, string pattern)
    {
        return Directory.GetFiles(path, pattern, new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).ToList();
    }

    /// <summary>
    /// Gets the invalid file name characters.
    /// </summary>
    /// <returns>The invalid file name characters</returns>
    public char[] GetInvalidFileNameChars()
    {
        return Path.GetInvalidFileNameChars();
    }

    /// <summary>
    /// Makes a file size human-readable
    /// </summary>
    /// <param name="size">The source file size</param>
    /// <returns>The human-readable file size</returns>
    /// <remarks>From stackoverflow.com/a/4975942/6776</remarks>
    public string HumanSize(long size)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (size == 0)
            return $"0 {suf[0]}";
        long bytes = Math.Abs(size);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return $"{Math.Sign(size) * num} {suf[place]}";
    }

    /// <summary>
    /// Checks if a path is a directory
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>true if the path is a directory ; otherwise, false</returns>
    public bool IsDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Joins paths
    /// </summary>
    /// <param name="paths">The paths parts to join.</param>
    /// <returns>The joined path</returns>
    public string PathJoin(params string[] paths)
    {
        return Path.Join(paths);
    }

    /// <summary>
    /// Reads all the lines in a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file content</returns>
    public string[] ReadAllLines(string path)
    {
        return File.ReadAllLines(path);
    }

    /// <summary>
    /// Reads a file using a stream.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="action">The action to execute on the file stream.</param>
    public async Task ReadFileStream(string path, Func<StreamReader, Task> action)
    {
        using var stream = File.OpenText(path);

        await action(stream);
    }

    /// <summary>
    /// Writes in a file using a stream.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="action">The action to execute on the stream writer.</param>
    public async Task WriteFileStream(string path, Func<StreamWriter, Task> action)
    {
        using var outStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var outStreamWriter = new StreamWriter(outStream);

        await action(outStreamWriter);
    }

    /// <summary>
    /// Lists the files inside a zip
    /// </summary>
    /// <param name="path">The path to the zip file</param>
    /// <param name="getSha1">Whether to get the SHA1 hash of the file</param>
    /// <returns>The zip file infos</returns>
    public IEnumerable<GameRomFile> GetZipFiles(string path, bool getSha1)
    {
        var result = new List<GameRomFile>();

        var fileName = FileName(path);

        using var zip = System.IO.Compression.ZipFile.OpenRead(path);

        // loop on entry, skipping folders (which are entries with size 0 and no name)
        foreach (var entry in zip.Entries.Where(e => e.Length > 0 && !string.IsNullOrEmpty(e.Name)))
        {
            result.Add(new GameRomFile
            {
                ZipFileName = fileName,
                Name = entry.Name,
                Path = Path.GetDirectoryName(entry.FullName),
                Size = (int)entry.Length,
                Crc = entry.Crc32.ToString("X4").PadLeft(8, '0').ToLower(),
                Sha1 = getSha1 ? GetZipFileSha1(entry) : null
            });
        }

        return result;
    }

    /// <summary>
    /// Replaces a file in a zip with another file
    /// </summary>
    /// <param name="sourceZip">The source zip to copy the file from</param>
    /// <param name="targetZip">The target zip to copy the file to</param>
    /// <param name="fileName">The file name to replace</param>
    /// <param name="sourceFilePath">The source file path, if any</param>
    public async Task ReplaceZipFile(string sourceZip, string targetZip, string fileName, string sourceFilePath = "")
    {
        if (!FileExists(sourceZip)) {
            return;
        }

        using var source = ZipFile.OpenRead(sourceZip);
        var mode = FileExists(targetZip) ? ZipArchiveMode.Update : ZipArchiveMode.Create;
        using var target = ZipFile.Open(targetZip, mode);

        // overwrite the entry
        var sourceEntry = source.GetEntry(string.IsNullOrEmpty(sourceFilePath) ? fileName : $"{sourceFilePath}/{fileName}");
        if (sourceEntry == null) {
            // maybe I should throw, but I have very bad exception management
            return;
        }

        var targetEntry = target.GetEntry(fileName);
        
        // recreate the entry
        targetEntry?.Delete();
        targetEntry = target.CreateEntry(fileName);

        // write content
        using var targetStream = targetEntry.Open();
        using var sourceStream = sourceEntry.Open();
        
        await sourceStream.CopyToAsync(targetStream);
    }

    /// <summary>
    /// Deletes a file in a zip
    /// </summary>
    /// <param name="zipFile">The path to the zip file</param>
    /// <param name="files">The list of files to delete</param>
    public void DeleteZipFile(string zipFile, IEnumerable<GameRomFile> files) {
        if (!FileExists(zipFile)) {
            return;
        }

        using var zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);

        foreach (var file in files) {
            var entry = zip.GetEntry(string.IsNullOrEmpty(file.Path) ? file.Name : $"{file.Path}/{file.Name}");
            entry?.Delete();
        }
    }

    private static string GetZipFileSha1(ZipArchiveEntry entry)
    {
        var sha1 = SHA1.Create();

        // see https://stackoverflow.com/questions/1993903
        byte[] hash = sha1.ComputeHash(entry.Open());
        StringBuilder formatted = new(2 * hash.Length);
        foreach (byte b in hash)
        {
            formatted.AppendFormat("{0:X2}", b);
        }

        return formatted.ToString().ToLower();
    }
}