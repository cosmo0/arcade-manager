using System;
using System.Reflection;
using System.Xml.Linq;
using ArcadeManager.Core.Models.Roms;
using FluentAssertions;

namespace ArcadeManager.Core.Tests.Models;

public class GameRomsTests
{
    private readonly XElement xml;
    
    public GameRomsTests()
    {
        string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string xmlPath = Path.Combine(here, "Data", "Models", "GameRom.xml");
        XDocument xmlData = XDocument.Load(xmlPath);
        xml = xmlData.Root!.Elements().First();
    }

    [Fact]
    public void GameRom_is_created_from_xml()
    {
        // arrange/act
        var game = GameRom.FromXml(xml, "romset");
    
        // assert: rom
        game.Name.Should().Be("test1");
        game.ParentName.Should().Be("test");
        game.BiosName.Should().Be("bios");

        // assert: files
        game.RomFiles.Should().HaveCount(18);
        var file = game.RomFiles["s92u-23a"];
        file.Should().NotBeNull();
        file.Size.Should().Be(524288);
        file.Crc.Should().Be("ac44415b");
        file.Sha1.Should().Be("218f8b1886eb72b8547127042b5ae47600e18944");
    }

    [Fact]
    public void GameRom_is_marked_as_error_missing_rom()
    {
        // arrange
        var game = GameRom.FromXml(xml, "romset");

        // act
        game.Error(ErrorReason.MissingFile, "Missing whole rom", null);
    
        // assert
        game.HasError.Should().BeTrue();
        game.RomFiles.HasError.Should().BeFalse();
    }

    [Fact]
    public void GameRom_is_marked_as_error_missing_bios()
    {
        // arrange
        var game = GameRom.FromXml(xml, "romset");

        // arrange: fake bios (should be filled with Infrastructure.DatFile.GetRoms)
        game.Bios = new() { Name = "bios.zip" };

        // act
        game.Error(ErrorReason.MissingFile, "Missing bios", "bios.zip");
    
        // assert
        game.HasError.Should().BeTrue();
        game.Bios.HasError.Should().BeTrue();
        game.RomFiles.HasError.Should().BeFalse();
    }

    [Fact]
    public void GameRom_is_marked_as_error_missing_file_in_zip()
    {
        // arrange
        var game = GameRom.FromXml(xml, "romset");

        // act
        game.Error(ErrorReason.MissingFile, "Missing file", "s92u-23a");
    
        // assert
        game.HasError.Should().BeTrue();
        game.RomFiles.HasError.Should().BeTrue();
        game.RomFiles["s92u-23a"].HasError.Should().BeTrue();
    }

    [Fact]
    public void GameRom_is_marked_as_error_bad_hash()
    {
        // arrange
        var game = GameRom.FromXml(xml, "romset");

        // act
        game.Error(ErrorReason.BadHash, "Bad hash", "s92u-23a");
    
        // assert
        game.HasError.Should().BeTrue();
        game.RomFiles.HasError.Should().BeTrue();
        game.RomFiles["s92u-23a"].HasError.Should().BeTrue();
    }

    [Fact]
    public void GameRomFile_is_removed()
    {
        // arrange
        var data = new GameRomFilesList {
            new() { Name = "a" },
            new() { Name = "b" },
            new() { Name = "a", Path = "sub" },
            new() { Name = "b", Path = "sub" }
        };
    
        // act
        data.RemoveFile("b");
        data.RemoveFile("a", "sub");
    
        // assert
        data.Should().HaveCount(2);
    }
}
