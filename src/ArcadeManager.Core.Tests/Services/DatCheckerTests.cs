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
using FluentAssertions.Extensions;

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

    [Fact]
    public async Task Rom_is_rebuilt_crc()
    {
        // arrange: arguments
        var args = new RomsActionCheckDat { Speed = "fast" };

        // arrange: found files matching the game files in the romset
        var foundFiles = new ReadOnlyGameRomFileList([
            new ReadOnlyGameRomFile("rom1.zip", "romset", "file1.a", "", 123, "abc", ""),
            new ReadOnlyGameRomFile("rom1.zip", "romset", "file2.a", "", 123, "def", ""),
            new ReadOnlyGameRomFile("rom2.zip", "romset", "file3.a", "", 123, "ghi", "")
        ]);

        // arrange: expected files in the game
        var gameFiles = new GameRomFilesList() {
            new GameRomFile { Name = "file1.a", Size = 123, Crc = "abc" },
            new GameRomFile { Name = "file2.a", Size = 123, Crc = "def" },
            new GameRomFile { Name = "file3.a", Size = 123, Crc = "ghi" }
        };

        // arrange: assume file replace works
        A.CallTo(() => fs.ReplaceZipFile(A<ZipFile>._, A<ZipFile>._, A<IGameRomFile>._, A<IGameRomFile>._))
            .Returns(true);

        // act
        await sut.RebuildGame("rom.zip", foundFiles, gameFiles, args);

        // assert
        A.CallTo(() => fs.ReplaceZipFile(A<ZipFile>._, A<ZipFile>._, A<IGameRomFile>._, A<IGameRomFile>._))
            .MustHaveHappened(3, Times.Exactly);
    }
    
    [Fact]
    public async Task Rom_is_rebuilt_sha1()
    {
        // arrange
        var args = new RomsActionCheckDat { Speed = "slow" };

        // arrange: found files in the romset
        var foundFiles = new ReadOnlyGameRomFileList([
            new ReadOnlyGameRomFile("rom1.zip", "romset", "file1.a", "", 123, "abc", "1"),
            new ReadOnlyGameRomFile("rom1.zip", "romset", "file2.a", "", 123, "def", "2"),
            new ReadOnlyGameRomFile("rom2.zip", "romset", "file3.a", "", 123, "ghi", "3")
        ]);

        // arrange: expected files in the game
        var gameFiles = new GameRomFilesList() {
            new GameRomFile { Name = "file1.a", Size = 123, Crc = "abc", Sha1 = "1" },
            new GameRomFile { Name = "file2.a", Size = 123, Crc = "def", Sha1 = "2" },
            new GameRomFile { Name = "file3.a", Size = 123, Crc = "ghi", Sha1 = "3" }
        };

        // arrange: assume file replace works
        A.CallTo(() => fs.ReplaceZipFile(A<ZipFile>._, A<ZipFile>._, A<IGameRomFile>._, A<IGameRomFile>._))
            .Returns(true);

        // act
        await sut.RebuildGame("rom.zip", foundFiles, gameFiles, args);

        // assert
        A.CallTo(() => fs.ReplaceZipFile(A<ZipFile>._, A<ZipFile>._, A<IGameRomFile>._, A<IGameRomFile>._))
            .MustHaveHappened(3, Times.Exactly);
    }
}
