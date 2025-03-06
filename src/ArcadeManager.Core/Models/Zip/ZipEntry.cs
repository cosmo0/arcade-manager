using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace ArcadeManager.Core.Models.Zip;

[DebuggerDisplay("{FullName} ({Crc})")]
public class ZipEntry(ZipArchiveEntry entry)
{
    public string Name => entry?.Name;

    public string FullName => entry?.FullName;

    public string Crc => entry?.Crc32.ToString("X4").PadLeft(8, '0').ToLower();

    public long Length => entry?.Length ?? 0;

    public void Delete()
    {
        entry?.Delete();
    }

    public Stream Open()
    {
        return entry?.Open();
    }

    public string GetSha1()
    {
        // see https://stackoverflow.com/questions/1993903
        var sha1 = SHA1.Create();
        byte[] hash = sha1.ComputeHash(entry.Open());
        StringBuilder formatted = new(2 * hash.Length);
        foreach (byte b in hash)
        {
            formatted.AppendFormat("{0:X2}", b);
        }

        return formatted.ToString().ToLower();
    }
}