using System;
using System.IO.Compression;

namespace ArcadeManager.Core.Models.Zip;

public class ZipFile : IDisposable
{
    private readonly ZipArchive archive;

    private bool disposedValue;

    private ZipFile(string path, ZipFileMode mode)
    {
        this.FilePath = path;
        this.Mode = mode;

        if (mode == ZipFileMode.Read) {
            archive = System.IO.Compression.ZipFile.OpenRead(path);
        } else {
            archive = System.IO.Compression.ZipFile.Open(path, mode == ZipFileMode.Create ? ZipArchiveMode.Create : ZipArchiveMode.Update);
        }
    }

    public string FilePath { get; set; }

    public ZipFileMode Mode { get; set; }

    public IEnumerable<ZipEntry> Entries => archive.Entries.Select(e => new ZipEntry(e));

    public static ZipFile Open(string path, ZipFileMode mode)
    {
        return new ZipFile(path, mode);
    }

    public ZipEntry CreateEntry(string name)
    {
        return new(archive.CreateEntry(name));
    }

    public ZipEntry GetEntry(string name)
    {
        return new(archive.GetEntry(name));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                archive.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}