using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models.Bezels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Domain;

public class RetroArchOverlayTests : TestBase
{
    private static readonly Bounds testBounds = new() { X = 10, Y = 20, Width = 30, Height = 40 };
    private static readonly Bounds testResolution = new() { Width = 640, Height = 480 };
    private readonly RetroArchOverlay sut;

    public RetroArchOverlayTests() : base()
    {
        var fs = new FileSystem(A.Fake<IEnvironment>());

        sut = new(fs);
    }

    [Fact]
    public async Task CreateConfig_creates_config()
    {
        // arrange: dest file
        var dest = GetTestFile("Domain", "result_createconfig.cfg");
        CleanupFile(dest);

        // arrange: template
        var template = GetTestFile("Domain", "template.cfg");

        // act
        await sut.CreateConfig(template, "test", dest, testBounds, testResolution);

        // assert
        File.Exists(dest).Should().BeTrue();
        var result = await File.ReadAllTextAsync(dest, TestContext.Current.CancellationToken);
        var expected = await File.ReadAllTextAsync(GetTestFile("Domain", "expected_createconfig.cfg"), TestContext.Current.CancellationToken);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task FillTemplate_fills_template()
    {
        // arrange
        var cfg = GetTestFile("Domain", "filltemplate.cfg");

        // act
        await sut.FillTemplate(cfg, "test", testBounds, testResolution);

        // assert
        var result = await File.ReadAllTextAsync(cfg, TestContext.Current.CancellationToken);
        var expected = await File.ReadAllTextAsync(GetTestFile("Domain", "expected_filltemplate.cfg"), TestContext.Current.CancellationToken);
        result.Should().Be(expected);
    }

    [Fact]
    public void GetBoundsFromConfig_gets_bounds()
    {
        // arrange
        var content = File.ReadAllText(GetTestFile("Domain", "sf2.zip.cfg"));

        // act
        var result = sut.GetBoundsFromConfig(content);

        // assert
        result.X.Should().Be(569);
        result.Y.Should().Be(247);
        result.Width.Should().Be(772);
        result.Height.Should().Be(584);
    }

    [Fact]
    public void GetCfgData_gets_config_data()
    {
        // arrange
        var content = File.ReadAllText(GetTestFile("Domain", "sf2.zip.cfg"));

        // act
        var result = sut.GetCfgData(content, "video_shader");

        // assert
        result.Should().Be("zfast_crt_curve.glslp");
    }

    [Fact]
    public async Task GetRetroArchConfig_gets_config()
    {
        // arrange
        var rom = GetTestFile("Domain", "sf2.zip.cfg");
        var cfgFolder = Path.Join(here, "Data", "Domain");

        // act
        var result = await sut.GetRetroArchConfig(rom, cfgFolder, testResolution);

        // assert
        result.OverlayImageFileName.Should().Be("sf2.png");
        result.OverlayImagePath.Should().Be(Path.Join(cfgFolder, "sf2.png"));
        result.SourceScreenPosition.Should().BeEquivalentTo(new Bounds { X = 569, Y = 247, Width = 772, Height = 584 });
        result.SourceResolution.Should().BeEquivalentTo(new Bounds { Width = 1920, Height = 1080 });
    }

    [Fact]
    public void NormalizePath_normalizes_path()
    {
        // act
        var result = sut.NormalizePath(":\\test\\path");

        // assert
        result.Should().Be("/test/path");
    }

    [Fact]
    public async Task SetBounds_sets_bounds()
    {
        // arrange
        var f = GetTestFile("Domain", "setbounds.cfg");

        // act
        await sut.SetBounds(f, "sf2", testBounds, testResolution);

        // assert
        var result = await File.ReadAllTextAsync(f, TestContext.Current.CancellationToken);
        var expected = await File.ReadAllTextAsync(GetTestFile("Domain", "expected_setbounds.cfg"), TestContext.Current.CancellationToken);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("test = value", "test = changed")]
    [InlineData("init = other", "init = other\ntest = changed")]
    public void SetCfgData_changes_config_data(string input, string expected)
    {
        // act
        var result = sut.SetCfgData(input, "test", "changed");

        // assert
        result.Should().Be(expected);
    }
}