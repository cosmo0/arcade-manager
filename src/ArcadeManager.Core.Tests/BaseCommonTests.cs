using ImageMagick;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests;

public abstract class BaseCommonTests
{
    /// <summary>
    /// Asserts that the provided files are identical.
    /// </summary>
    /// <param name="result">The path to the result file.</param>
    /// <param name="expected">The path to the expected content file.</param>
    protected static async Task AssertFilesAreIdentical(string result, string expected)
    {
        File.Exists(result).Should().BeTrue();
        File.Exists(expected).Should().BeTrue();

        var resultContent = await File.ReadAllBytesAsync(result, TestContext.Current.CancellationToken);
        var expectedContent = await File.ReadAllBytesAsync(expected, TestContext.Current.CancellationToken);

        if (result.EndsWith(".png") || result.EndsWith(".jpg"))
        {
            AssertImagesAreIdentical(resultContent, expectedContent);
        }
        else
        {
            resultContent.Should().BeEquivalentTo(expectedContent);
        }
    }

    /// <summary>
    /// Asserts that two folders have identical content.
    /// </summary>
    /// <param name="result">The path to the result folder.</param>
    /// <param name="expected">The path to the expected folder content.</param>
    protected static async Task AssertFoldersAreIdentical(string result, string expected)
    {
        Directory.Exists(result).Should().BeTrue();
        Directory.Exists(expected).Should().BeTrue();

        var subItemsResult = Directory.GetFileSystemEntries(result, "*.*", SearchOption.AllDirectories);
        var subItemsExpected = Directory.GetFileSystemEntries(expected, "*.*", SearchOption.AllDirectories);

        // check if the listed items are equal
        subItemsResult.Select(r => r.Replace(result, string.Empty)).Should().BeEquivalentTo(subItemsExpected.Select(r => r.Replace(expected, string.Empty)));

        // check items contents
        foreach (var sir in subItemsResult)
        {
            var nameResult = sir.Replace(result, string.Empty);
            var matchingExpected = subItemsExpected.Single(e => e.EndsWith(nameResult));

            // the GetFileSystemEntries should have grabbed all sub-items
            if (!File.GetAttributes(sir).HasFlag(FileAttributes.Directory))
            {
                await AssertFilesAreIdentical(sir, matchingExpected);
            }
        }
    }

    /// <summary>
    /// Asserts that two images are identical.
    /// </summary>
    /// <param name="result">The path to the result image.</param>
    /// <param name="expected">The path to the expected image.</param>
    protected static void AssertImagesAreIdentical(byte[] result, byte[] expected)
    {
        using MagickImage resultImage = new(result);
        using MagickImage expectedImage = new(expected);

        var compareResult = resultImage.Compare(expectedImage, ErrorMetric.MeanAbsolute, Channels.RGBA);

        compareResult.Should().Be(0);
    }

    /// <summary>
    /// Deletes the specified file if it exists
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    protected static void CleanupFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// Cleanups the specified folder.
    /// </summary>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="recreate">if set to <c>true</c> recreate the folder after deletion.</param>
    protected static void CleanupFolder(string folder, bool recreate)
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        if (recreate)
        {
            Directory.CreateDirectory(folder);
        }
    }

    /// <summary>
    /// Copies the content of a folder in another.
    /// </summary>
    /// <param name="source">The source folder.</param>
    /// <param name="target">The target folder.</param>
    protected static void CopyFolder(string source, string target)
    {
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }

        foreach (var f in Directory.GetFiles(source))
        {
            File.Copy(f, Path.Join(target, Path.GetFileName(f)));
        }
    }
}