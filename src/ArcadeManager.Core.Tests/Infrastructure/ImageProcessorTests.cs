using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models.Bezels;
using ImageMagick;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Infrastructure;

public class ImageProcessorTests : TestBase
{
    private readonly string outputFolder;
    private readonly Bounds resolution = new() { Width = 1920, Height = 1080 };
    private readonly string sourceImage;
    private readonly ImageProcessor sut;

    private readonly string testFolder = "Infrastructure";

    public ImageProcessorTests() : base()
    {
        // general fakes
        A.CallTo(() => fs.PathJoin(A<string[]>._)).ReturnsLazily((string[] paths) => Path.Join(paths));
        A.CallTo(() => fs.FileCopy(A<string>._, A<string>._, A<bool>._)).Invokes((string source, string dest, bool ow) => File.Copy(source, dest, ow));

        // system under test
        sut = new(fs);

        // globals
        sourceImage = GetTestPath(testFolder, "1943.png");
        outputFolder = GetTestPath(testFolder, "output");

        // reset output folder
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
    }

    [Fact]
    public async Task Debug_is_drawn()
    {
        // arrange: screen position
        Bounds screenPosition = new() { X = 674, Y = 184, Width = 575, Height = 734 };

        // arrange: expected result
        var expected = await File.ReadAllBytesAsync(GetTestPath(testFolder, "1943_expected_debug.png"), TestContext.Current.CancellationToken);

        // arrange: cleanup
        var outputFile = Path.Combine(outputFolder, "1943.png");
        CleanupFile(outputFile);

        // act
        await sut.DebugDraw("1943", outputFolder, sourceImage, screenPosition, resolution);
        var result = await File.ReadAllBytesAsync(outputFile, TestContext.Current.CancellationToken);

        // assert
        AssertImagesAreIdentical(result, expected);
    }

    [Fact]
    public async Task Image_bytes_is_resized()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(sourceImage, TestContext.Current.CancellationToken);
        var expected = await File.ReadAllBytesAsync(GetTestPath(testFolder, "1943_expected_resize.png"), TestContext.Current.CancellationToken);

        // act
        var result = await sut.Resize(content, 1280, 720);

        // assert
        AssertImagesAreIdentical(result, expected);
    }

    [Fact]
    public async Task Image_file_is_resized()
    {
        // arrange
        var temp = Path.Combine(outputFolder, "resize.png");
        CleanupFile(temp);
        var expected = await File.ReadAllBytesAsync(GetTestPath(testFolder, "1943_expected_resize.png"), TestContext.Current.CancellationToken);
        File.Copy(sourceImage, temp);

        // act
        await sut.Resize(temp, 1280, 720);
        var result = await File.ReadAllBytesAsync(temp, TestContext.Current.CancellationToken);

        // assert
        AssertImagesAreIdentical(result, expected);
    }

    [Fact]
    public async Task Screen_is_found_when_transparent_area()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(sourceImage, TestContext.Current.CancellationToken);

        // act
        var result = sut.FindScreen(content, 0);

        // assert
        result.Should().BeEquivalentTo(new Bounds() { X = 674, Y = 184, Width = 575, Height = 734 });
    }

    [Fact]
    public async Task Screen_is_not_found_when_no_transparent_area()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(GetTestPath(testFolder, "nontransparent.png"), TestContext.Current.CancellationToken);

        // act
        var result = sut.FindScreen(content, 0);

        // assert
        result.Should().BeEquivalentTo(new Bounds() { X = 0, Y = 0, Width = 0, Height = 0 });
    }

    [Fact]
    public void Size_is_returned()
    {
        // act
        var result = sut.GetSize(sourceImage);

        // assert
        result.Should().BeEquivalentTo(resolution);
    }

    private static void AssertImagesAreIdentical(byte[] result, byte[] expected)
    {
        using MagickImage resultImage = new(result);
        using MagickImage expectedImage = new(expected);

        var compareResult = resultImage.Compare(expectedImage, ErrorMetric.MeanAbsolute, Channels.RGBA);

        compareResult.Should().Be(0);
    }
}