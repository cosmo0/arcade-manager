using ArcadeManager.Core.Models.Actions;
using ArcadeManager.Core.Services;
using System;
using System.Threading.Tasks;

namespace ArcadeManager.Core.IntegrationTests;

public class RomsIntegrationTests : BaseIntegrationTest
{
    private readonly Roms sut;

    public RomsIntegrationTests()
    {
        sut = new(fs, csvFile);
    }

    [Fact]
    public async Task Add_adds_roms()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "add.csv"),
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("roms", "add_output")
        };

        CleanupFolder(options.Selection, true);

        // act
        await sut.Add(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("roms", "add_output"), GetTestPath("roms", "add_expected"));
    }

    [Fact]
    public async Task AddFromWizard_adds_roms()
    {
        // arrange
        var options = new RomsAction
        {
            Main = "fbneo/top50,top80s,top90s",
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("roms", "addwizard_output")
        };

        // act
        await sut.AddFromWizard(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("roms", "addwizard_output"), GetTestPath("roms", "addwizard_expected"));
    }

    [Fact]
    public async Task Check_lists_missing_files()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "check.csv"),
            Romset = GetTestPath("romset")
        };

        // act
        var result = await sut.Check(options, messageHandler);

        // assert
        result.Should().BeEquivalentTo("a", "b", "c");
    }

    [Fact]
    public async Task Delete_removes_files()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "delete.csv"),
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("roms", "delete_output")
        };

        CleanupFolder(options.Selection, true);
        CopyFolder(GetTestPath("romset"), options.Selection);

        // act
        await sut.Delete(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("roms", "delete_output"), GetTestPath("roms", "delete_expected"));
    }

    [Fact]
    public async Task DeleteCheck_lists_files()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "check-delete.csv"),
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("romset")
        };

        // act
        var result = await sut.DeleteCheck(options, messageHandler);

        // assert
        result.Should().BeEquivalentTo("1942.zip", "astdelux.zip", "atetris.zip", "baddudes.zip");
    }

    [Fact]
    public async Task Keep_keeps_files()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "keep.csv"),
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("roms", "keep_output")
        };

        CleanupFolder(options.Selection, true);
        CopyFolder(GetTestPath("romset"), options.Selection);

        // act
        await sut.Keep(options, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFoldersAreIdentical(GetTestPath("roms", "keep_output"), GetTestPath("roms", "keep_expected"));
    }

    [Fact]
    public async Task KeepCheck_lists_files_to_be_deleted()
    {
        // arrange
        var options = new RomsAction
        {
            Main = GetTestPath("roms", "check-keep.csv"),
            Romset = GetTestPath("romset"),
            Selection = GetTestPath("romset")
        };

        // act
        var result = await sut.KeepCheck(options, messageHandler);

        // assert
        result.Should().BeEquivalentTo("1942.zip", "astdelux.zip", "atetris.zip", "baddudes.zip");
    }
}