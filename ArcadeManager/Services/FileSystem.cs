using System;
using System.IO;

namespace ArcadeManager.Services;

/// <summary>
/// File system utilities
/// </summary>
public static class FileSystem {

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
    public static int DirectoryCopy(string sourceDirName, string destDirName, bool overwrite, bool copySubDirs) {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new(sourceDirName);

        if (!dir.Exists) {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName)) {
            Directory.CreateDirectory(destDirName);
        }

        int nbCopied = 0;

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files) {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, overwrite);
            nbCopied++;
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs) {
            foreach (DirectoryInfo subdir in dirs) {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                nbCopied += DirectoryCopy(subdir.FullName, tempPath, overwrite, copySubDirs);
            }
        }

        return nbCopied;
    }

    /// <summary>
    /// Computes a directory size
    /// </summary>
    /// <param name="directory">The path to the directory</param>
    /// <returns>The directory size</returns>
    /// <remarks>From stackoverflow.com/a/468131/6776</remarks>
    public static long DirectorySize(string directory) {
        var d = new DirectoryInfo(directory);

        long size = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis) {
            size += fi.Length;
        }

        // Add subdirectory sizes.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis) {
            size += DirectorySize(di.FullName);
        }

        return size;
    }

    /// <summary>
    /// Makes sure that a folder exists
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    public static void EnsureDirectory(string targetFolder) {
        if (!Directory.Exists(targetFolder)) {
            Directory.CreateDirectory(targetFolder);
        }
    }

    /// <summary>
    /// Checks if a path exists
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>Whether the path exists</returns>
    public static bool Exists(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }

        return IsDirectory(path)
            ? Directory.Exists(path)
            : File.Exists(path);
    }

    /// <summary>
    /// Makes a file size human-readable
    /// </summary>
    /// <param name="size">The source file size</param>
    /// <returns>The human-readable file size</returns>
    /// <remarks>From stackoverflow.com/a/4975942/6776</remarks>
    public static string HumanSize(long size) {
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
    public static bool IsDirectory(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return false;
        }

        try {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
        catch (Exception) {
            return false;
        }
    }
}