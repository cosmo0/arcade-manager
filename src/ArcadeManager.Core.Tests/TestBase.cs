using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests;

public abstract class TestBase
{
    protected readonly IFileSystem fs;
    protected readonly string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    protected readonly IMessageHandler messageHandler;
    protected readonly IFileSystem realfs;

    protected TestBase()
    {
        // some tests are much easier to write using the real fs layer
        this.fs = A.Fake<IFileSystem>();
        this.realfs = new FileSystem(A.Fake<IEnvironment>());

        this.messageHandler = A.Fake<IMessageHandler>();
    }

    /// <summary>
    /// Asserts that the provided files are identical.
    /// </summary>
    /// <param name="result">The path to the result file.</param>
    /// <param name="expected">The path to the expected content file.</param>
    protected static async Task AssertFilesAreIdentical(string result, string expected)
    {
        File.Exists(result).Should().BeTrue();
        File.Exists(expected).Should().BeTrue();

        var targetContent = await File.ReadAllTextAsync(result, TestContext.Current.CancellationToken);
        var expectedContent = await File.ReadAllTextAsync(expected, TestContext.Current.CancellationToken);

        targetContent.Should().BeEquivalentTo(expectedContent);
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