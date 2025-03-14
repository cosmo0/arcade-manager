using System.Diagnostics;

namespace ArcadeManager.Core.Models.Roms;

/// <summary>
/// A game rom file (rom files inside the zip)
/// </summary>
[DebuggerDisplay("{Path}\\{Name} ({Crc})")]
public class ReadOnlyGameRomFile(
    string zipFileName,
    string zipFileFolder,
    string name,
    string path,
    long size,
    string crc,
    string sha1) : IGameRomFile
{
    public string ZipFileName => zipFileName;

    public string ZipFileFolder => zipFileFolder;

    public string ZipFilePath => System.IO.Path.Join(ZipFileFolder, ZipFileName);

    public string Name => name;

    public string Path => path;

    public long Size => size;

    public string Crc => crc;

    public string Sha1 => sha1;

    public GameRomFile ToGameRomFile() {
        return new GameRomFile {
            Name = Name,
            Path = Path,
            Crc = Crc,
            Sha1 = Sha1,
            Size = Size,
            ErrorReason = ErrorReason.None
        };
    }
}
