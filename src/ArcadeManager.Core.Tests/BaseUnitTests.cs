using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ImageMagick;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests;

public abstract class BaseUnitTests
{
    protected readonly IFileSystem fs;
    protected readonly string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    protected readonly IMessageHandler messageHandler;
    protected readonly IFileSystem realfs;

    protected BaseUnitTests()
    {
        // some tests are much easier to write using the real fs layer
        this.fs = A.Fake<IFileSystem>();
        this.realfs = new FileSystem(A.Fake<IEnvironment>());

        this.messageHandler = A.Fake<IMessageHandler>();
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
    /// Gets the a file in the "data" test folder.
    /// </summary>
    /// <param name="folder">The sub-folder name.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The concatenated file name</returns>
    protected string GetTestPath(string folder, string fileName)
    {
        return Path.Combine(here, "Data", folder, fileName);
    }
}