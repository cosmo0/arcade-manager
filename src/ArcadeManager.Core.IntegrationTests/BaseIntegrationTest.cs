using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ImageMagick;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcadeManager.Core.IntegrationTests;

public abstract class BaseIntegrationTest
{
    protected readonly string basePath;
    protected readonly CsvFile csvFile;
    protected readonly DatFile datFile;
    protected readonly Downloader downloader;
    protected readonly IEnvironment env;
    protected readonly FileSystem fs;
    protected readonly ImageProcessor imageProcessor;
    protected readonly IniFile iniFile;
    protected readonly ILocalizer localizer;
    protected readonly MameOverlay mameOverlay;
    protected readonly IMessageHandler messageHandler;
    protected readonly RetroArchOverlay retroarchOverlay;
    protected readonly IWebClientFactory webClientFactory;

    protected BaseIntegrationTest()
    {
        basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        // the only property of environment that fs needs is GetBasePath
        env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetBasePath()).Returns(basePath);

        // localizer returns the code
        localizer = A.Fake<ILocalizer>();
        A.CallTo(() => localizer.CultureName(A<string>._)).Returns("test");
        A.CallTo(() => localizer.IsCurrentLocale(A<string>._)).Returns(true);
        A.CallTo(() => localizer.CurrentLocale()).Returns("test");
        A.CallTo(() => localizer.Locales).Returns(["test"]);
        A.CallTo(() => localizer[A<string>._]).ReturnsLazily((string code) => code);

        // message handler that we can inspect and mock
        messageHandler = A.Fake<IMessageHandler>();

        // init infra
        fs = new FileSystem(env);
        webClientFactory = new WebClientFactory();

        // init domain
        csvFile = new CsvFile();
        iniFile = new IniFile();
        datFile = new DatFile();
        imageProcessor = new ImageProcessor();
        retroarchOverlay = new RetroArchOverlay(fs);
        mameOverlay = new MameOverlay(fs);

        // init common services
        downloader = new Downloader(webClientFactory, fs, localizer);
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

        if (result.EndsWith(".png") || result.EndsWith(".jpg"))
        {
            var resultContent = await File.ReadAllBytesAsync(result, TestContext.Current.CancellationToken);
            var expectedContent = await File.ReadAllBytesAsync(expected, TestContext.Current.CancellationToken);

            AssertImagesAreIdentical(resultContent, expectedContent);
        }
        else
        {
            var resultContent = await File.ReadAllLinesAsync(result, TestContext.Current.CancellationToken);
            var expectedContent = await File.ReadAllLinesAsync(expected, TestContext.Current.CancellationToken);

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

    /// <summary>
    /// Gets the a file in the "data" test folder for integration tests.
    /// </summary>
    /// <param name="pathItems">The path items (missing the data\integration part).</param>
    /// <returns>
    /// The concatenated file name
    /// </returns>
    protected string GetTestPath(params string[] pathItems)
    {
        return Path.Combine([basePath, "Data", .. pathItems]);
    }
}