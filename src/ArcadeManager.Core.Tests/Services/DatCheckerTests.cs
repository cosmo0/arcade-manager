using System;
using Xunit;
using FakeItEasy;
using FluentAssertions;
using ArcadeManager.Services;
using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using ArcadeManager.Actions;
using System.IO.Compression;

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
        game.RomFiles.Add(new GameRomFile {
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
            new GameRomFile {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipArchive>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

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
            new GameRomFile {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            },
            new GameRomFile {
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
            new GameRomFile {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipArchive>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

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
            new GameRomFile {
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
            new GameRomFile {
                Name = "test.1",
                Crc = "def",
                Size = 1234
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("test.zip");
        A.CallTo(() => fs.FileExists("test.zip")).Returns(true);
        A.CallTo(() => fs.GetZipFiles(A<ZipArchive>._, A<string>._, A<string>._, A<bool>._)).Returns(zipFiles);

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
    public async Task Rom_is_fixed()
    {
        // arrange: data
        var game = new GameRom {
            Name = "test"
        };
        game.RomFiles.Add(new GameRomFile {
            Name = "test.1",
            Crc = "abcd",
            Size = 1234,
            ErrorReason = ErrorReason.MissingFile
        });
        var args = new RomsActionCheckDat {
            ChangeType = true,
            Romset = "roms",
            TargetFolder = "fix"
        };
        GameRomList processed = [game];

        var fixFolder = new GameRomFilesList {
            new GameRomFile {
                Name = "test.1",
                Crc = "abcd",
                Size = 1234,
                ZipFileName = "test.zip",
                ZipFileFolder = "roms"
            }
        };

        // arrange: services
        A.CallTo(() => fs.PathJoin("roms", "test.zip")).Returns("roms/test.zip");
        A.CallTo(() => fs.PathJoin("fix", "test.zip")).Returns("fix/test.zip");
        A.CallTo(() => fs.FileExists("roms/test.zip")).Returns(true);
        A.CallTo(() => fs.FileExists("fix/test.zip")).Returns(true);

        // act (the game.RomFiles is re-cast so the list is cloned)
        await sut.FixGame(null, game, [..game.RomFiles], args, processed, fixFolder, messageHandler);

        // assert
        A.CallTo(() => fs.ReplaceZipFile(A<System.IO.Compression.ZipArchive>._, A<GameRomFile>._)).MustHaveHappened();
        game.RomFiles[0].ErrorReason.Should().Be(ErrorReason.None);
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
        A.CallTo(() => fs.DeleteZipFile(A<ZipArchive>._, A<GameRomFile>._)).MustHaveHappenedOnceExactly();

        zipFiles.Should().HaveCount(2);
    }
}
