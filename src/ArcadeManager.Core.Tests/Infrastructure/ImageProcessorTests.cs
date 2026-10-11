using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Bezels;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ArcadeManager.Core.Tests.Infrastructure;

public class ImageProcessorTests
{
    private readonly IFileSystem fs;
    private readonly string here;
    private readonly string outputFolder;
    private readonly Bounds resolution = new() { Width = 1920, Height = 1080 };
    private readonly string sourceFolder;
    private readonly string sourceImage;
    private readonly ImageProcessor sut;

    public ImageProcessorTests()
    {
        fs = A.Fake<IFileSystem>();

        // general fakes
        A.CallTo(() => fs.PathJoin(A<string[]>._)).ReturnsLazily((string[] paths) => Path.Join(paths));
        A.CallTo(() => fs.FileCopy(A<string>._, A<string>._, A<bool>._)).Invokes((string source, string dest, bool ow) => File.Copy(source, dest, ow));

        // system under test
        sut = new(fs);

        // globals
        here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        sourceFolder = Path.Join(here, "Data", "Infrastructure");
        sourceImage = Path.Join(sourceFolder, "1943.png");
        outputFolder = Path.Join(sourceFolder, "output");

        // reset output folder
        if (Directory.Exists(outputFolder)) { Directory.Delete(outputFolder, true); }
        Directory.CreateDirectory(outputFolder);
    }

    [Fact]
    public async Task Debug_is_drawn()
    {
        // arrange: screen position
        Bounds screenPosition = new() { X = 674, Y = 184, Width = 575, Height = 734 };

        // arrange: expected result
        var expected = await File.ReadAllBytesAsync(Path.Join(sourceFolder, "expected", "1943_debug.png"));

        // act
        sut.DebugDraw("1943", outputFolder, sourceImage, screenPosition, resolution);
        var result = await File.ReadAllBytesAsync(Path.Join(outputFolder, "1943.png"));

        // assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Image_bytes_is_resized()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(sourceImage);
        var expected = await File.ReadAllBytesAsync(Path.Combine(sourceFolder, "expected", "1943_resize.png"));

        // act
        var result = sut.Resize(content, 1280, 720);

        // assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Image_file_is_resized()
    {
        // arrange
        var temp = Path.Combine(outputFolder, "resize.png");
        var expected = await File.ReadAllBytesAsync(Path.Combine(sourceFolder, "expected", "1943_resize.png"));
        File.Copy(sourceImage, temp);

        // act
        sut.Resize(temp, 1280, 720);
        var result = await File.ReadAllBytesAsync(temp);

        // assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Screen_is_found_when_transparent_area()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(sourceImage);

        // act
        var result = sut.FindScreen(content, 0);

        // assert
        result.Should().BeEquivalentTo(new Bounds() { X = 674, Y = 184, Width = 575, Height = 734 });
    }

    [Fact]
    public async Task Screen_is_not_found_when_no_transparent_area()
    {
        // arrange
        var content = await File.ReadAllBytesAsync(Path.Join(sourceFolder, "nontransparent.png"));

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
}