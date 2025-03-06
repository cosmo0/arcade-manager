using System;
using Xunit;
using FakeItEasy;
using FluentAssertions;
using ArcadeManager.Core.Services;
using ArcadeManager.Core.Services.Interfaces;
using ArcadeManager.Core.Models.Roms;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Actions;
using ArcadeManager.Core.Models.Zip;

namespace ArcadeManager.Core.Tests.Services;

public class DatCheckerTests
{
    private readonly ICsv csv;
    private readonly IFileSystem fs;
    private readonly IDatFile dat;
    private readonly IMessageHandler messageHandler;
    private readonly DatChecker sut;

    public DatCheckerTests()
    {
        this.csv = A.Fake<ICsv>();
        this.fs = A.Fake<IFileSystem>();
        this.dat = A.Fake<IDatFile>();
        this.messageHandler = A.Fake<IMessageHandler>();

        this.sut = new DatChecker(this.fs, this.csv, this.dat);
    }

    [Fact]
    public void Rom_is_checked()
    {
        // arrange: data
        var game = new GameRom {
            Name = "test"
        };
        game.RomFiles.Add(new GameRomFile() {
            Name = "test.1",
            Crc = "abcd",
            Size = 1234
        });
        var args = new RomsActionCheckDat {
            ChangeType = false,
            Romset = "roms"
        };
        var processed = new GameRomList();

        var zipFiles = new GameRomFilesList {
            new GameRomFile() {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipFile>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

        // act
        sut.CheckGame(game, args, processed, this.messageHandler);

        // assert
        processed.Should().NotBeEmpty();
        processed[0].Should().Be(game);
        game.HasError.Should().BeFalse();
    }

    [Fact]
    public void Rom_not_found_is_error()
    {
        // arrange: data
        var game = new GameRom {
            Name = "test"
        };
        var args = new RomsActionCheckDat {
            ChangeType = false,
            Romset = "roms",
            ReportAll = true
        };
        var processed = new GameRomList();

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(false);

        // act
        sut.CheckGame(game, args, processed, this.messageHandler);

        // assert
        processed.Should().NotBeEmpty();
        processed[0].Should().Be(game);
        game.HasError.Should().BeTrue();
    }

    [Fact]
    public void Rom_missing_romfile_is_error()
    {
        // arrange: data
        var game = new GameRom {
            Name = "test"
        };
        game.RomFiles.AddRange([
            new GameRomFile() {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            },
            new GameRomFile() {
                Name = "test.2",
                Crc = "def",
                Size = 456
            }
        ]);
        var args = new RomsActionCheckDat {
            ChangeType = false,
            Romset = "roms"
        };
        var processed = new GameRomList();

        var zipFiles = new GameRomFilesList {
            new GameRomFile() {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipFile>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

        // act
        sut.CheckGame(game, args, processed, this.messageHandler);

        // assert
        processed.Should().NotBeEmpty();
        processed[0].Should().Be(game);
        game.RomFiles.HasError.Should().BeTrue();
        game.RomFiles.Should().HaveCount(2);
        game.RomFiles["test.1"].HasError.Should().BeFalse();
        game.RomFiles["test.2"].HasError.Should().BeTrue();
        game.RomFiles["test.2"].ErrorReason.Should().Be(ErrorReason.MissingFile);
    }

    [Fact]
    public void Rom_bad_crc_is_error()
    {
        // arrange: data
        var game = new GameRom {
            Name = "test"
        };
        game.RomFiles.AddRange([
            new GameRomFile() {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            }
        ]);
        var args = new RomsActionCheckDat {
            ChangeType = false,
            Romset = "roms"
        };
        var processed = new GameRomList();

        var zipFiles = new GameRomFilesList {
            new GameRomFile() {
                Name = "test.1",
                Crc = "def",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipFile>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

        // act
        sut.CheckGame(game, args, processed, this.messageHandler);

        // assert
        processed.Should().NotBeEmpty();
        processed[0].Should().Be(game);
        game.RomFiles.HasError.Should().BeTrue();
        game.RomFiles["test.1"].HasError.Should().BeTrue();
        game.RomFiles["test.1"].ErrorReason.Should().Be(ErrorReason.BadHash);
    }

    [Fact]
    public void Cleanup_removes_excess_files()
    {
        // arrange: game
        var game = new GameRom {
            Name = "test"
        };
        game.RomFiles.Add(new() { Name = "a" });
        game.RomFiles.Add(new() { Name = "b" });

        // arrange: zip files
        var zipFiles = new GameRomFilesList {
            new() { Name = "a" },
            new() { Name = "b" },
            new() { Name = "c" }
        };
    
        // act
        sut.CleanupFilesOfGame(null, game, zipFiles);
    
        // assert: one file has been deleted
        A.CallTo(() => fs.DeleteZipFile(A<ZipFile>._, A<GameRomFile>._)).MustHaveHappenedOnceExactly();

        zipFiles.Should().HaveCount(2);
    }
}
