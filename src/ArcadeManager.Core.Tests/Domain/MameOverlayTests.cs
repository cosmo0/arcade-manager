using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models.Bezels;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Domain;

public class MameOverlayTests : TestBase
{
    private readonly MameOverlay sut;

    public MameOverlayTests() : base()
    {
        sut = new(realfs);
    }

    [Fact]
    public async Task ExtractFiles_reads_zip()
    {
        // arrange
        var testFile = GetTestPath("Domain", "1943.zip");

        // act
        var (lay, cfg, bezel) = await sut.ReadZip("1943", testFile, null, true);

        // assert
        AssertMameReadFiles(lay, cfg, bezel);
    }

    [Fact]
    public void MameGetBezel_is_read_with_cfg()
    {
        // arrange: lay file
        var lay = GetTestLayFile();

        // arrange: cfg file
        var cfg = new MameCfgFile
        {
            SystemConfig = new()
            {
                Name = "1943",
                VideoConfig = new()
                {
                    VideoScreen = new()
                    {
                        Index = 0,
                        HOffset = -0.016000,
                        HStretch = 0.624000,
                        VOffset = 0.006000,
                        VStretch = 0.632000
                    }
                }
            }
        };

        // act
        var result = sut.GetBezel(lay, cfg, true);

        // assert
        result.BezelFileName.Should().Be("bezel.png");
        result.Offset.Should().BeEquivalentTo(new Offset { HOffset = -0.016, HStretch = 0.624, VOffset = 0.006, VStretch = 0.632 });
        result.SourceResolution.Should().BeEquivalentTo(new Bounds { Width = 1920, Height = 1080 });
        result.SourceScreenPosition.Should().BeEquivalentTo(new Bounds { X = 518, Y = 0, Width = 920, Height = 1080 });
    }

    [Fact]
    public void MameGetBezel_is_read_without_cfg()
    {
        // arrange: lay file
        var lay = GetTestLayFile();

        // act
        var result = sut.GetBezel(lay, null, true);

        // assert
        result.BezelFileName.Should().Be("bezel.png");
        result.Offset.Should().BeNull();
        result.SourceResolution.Should().BeEquivalentTo(new Bounds { Width = 1920, Height = 1080 });
        result.SourceScreenPosition.Should().BeEquivalentTo(new Bounds { Height = 1080, Width = 920, X = 518, Y = 0 });
    }

    [Fact]
    public async Task MameReadFiles_reads_folder()
    {
        // arrange
        var testFolder = GetTestPath("Domain", "1943");

        // act
        var (lay, cfg, bezel) = await sut.ReadFolder("1943", testFolder, null, true);

        // assert
        AssertMameReadFiles(lay, cfg, bezel);
    }

    private static void AssertMameReadFiles(MameLayFile lay, MameCfgFile cfg, byte[] bezel)
    {
        lay.Should().NotBeNull();

        lay.Elements.Should().HaveCount(6);
        var l = lay.Elements[0];
        l.Name.Should().Be("bezel");
        l.Images.Should().HaveCount(1);
        l.Images[0].File.Should().Be("1943_bezel.png");

        lay.Views.Should().HaveCount(4);
        var v = lay.Views[0];
        v.Name.Should().Be("Cab Artwork ");
        v.Backdrops.Should().HaveCount(1);
        v.Bezels.Should().HaveCount(1);
        v.Overlays.Should().HaveCount(1);
        v.Screens.Should().HaveCount(1);

        cfg.Should().BeNull();

        bezel.Should().NotBeNull().And.HaveCount(2538476);
    }

    private static MameLayFile GetTestLayFile()
    {
        return new MameLayFile
        {
            Elements = [
                new() { Name = "bezel_element", Images = [ new() { File = "bezel.png" }] } ,
                new() { Name = "backdrop_element", Images = [ new() { File = "backdrop.png" }] },
                new() { Name = "overlay_element", Images = [ new() { File = "overlay.png" }] },
                new() { Name = "screen_element", Images = [ new() { File = "screen.png" }] },
            ],
            Views = [
                new() {
                    Name = "Test view",
                    Bezels = [ new() { ElementName = "bezel_element", Bounds = new() { X = 0, Y = 0, Width = 1920, Height = 1080 } }],
                    Backdrops = [ new() { ElementName = "backdrop_element", Bounds = new() { X = 518, Y = 0, Width = 884, Height = 1080 } }],
                    Overlays = [new() { ElementName = "overlay_element", Bounds = new() { X = 554, Y = 0, Width = 812, Height = 1080 } }],
                    Screens = [new() { Bounds = new() { X = 555, Y = 0, Width = 810, Height = 1080 } }]
                }
            ]
        };
    }
}