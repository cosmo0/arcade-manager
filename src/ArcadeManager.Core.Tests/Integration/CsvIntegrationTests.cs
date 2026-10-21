using System;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Integration;

public class CsvIntegrationTests : BaseIntegrationTest
{
    private readonly Core.Services.Csv sut;

    public CsvIntegrationTests() : base()
    {
        sut = new(fs, csvFile, iniFile, datFile);
    }

    [IgnoreLocallyFact]
    public async Task ConvertDat_converts_dat()
    {
        // arrange
        var main = GetTestPath("csv", "convertdat.xml");
        var target = GetTestPath("csv", "convertdat_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("csv", "convertdat_expected.csv");

        // act
        await sut.ConvertDat(main, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        File.Exists(target).Should().BeTrue();
        await AssertFilesAreIdentical(target, expected);
    }

    [IgnoreLocallyFact]
    public async Task ConvertIni_converts_ini()
    {
        // arrange
        var main = GetTestPath("csv", "convertini.ini");
        var target = GetTestPath("csv", "convertini_result");
        CleanupFolder(target, true);
        var expected = GetTestPath("csv", "convertini_expected");

        // act
        await sut.ConvertIni(main, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        foreach (var f in Directory.GetFileSystemEntries(target))
        {
            var fileName = Path.GetFileName(f);
            var expectedFile = Path.Combine(expected, fileName);
            await AssertFilesAreIdentical(f, expectedFile);
        }
    }

    [IgnoreLocallyFact]
    public async Task Keep_keeps_only_listed_entries()
    {
        // arrange
        var main = GetTestPath("csv", "csv-set.csv");
        var secondary = GetTestPath("csv", "csv-secondary.csv");
        var target = GetTestPath("csv", "csv-keep_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("csv", "csv-keep_expected.csv");

        // act
        await sut.Keep(main, secondary, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFilesAreIdentical(target, expected);
    }

    [IgnoreLocallyFact]
    public async Task ListFiles_lists_files_in_folder()
    {
        // arrange
        var main = GetTestPath("romset_fbneo");
        var target = GetTestPath("csv", "list_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("csv", "list_expected.csv");

        // act
        await sut.ListFiles(main, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFilesAreIdentical(target, expected);
    }

    [IgnoreLocallyFact]
    public async Task Merge_merges_two_lists()
    {
        // arrange
        var main = GetTestPath("csv", "csv-set.csv");
        var secondary = GetTestPath("csv", "csv-secondary.csv");
        var target = GetTestPath("csv", "csv-merge_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("csv", "csv-merge_expected.csv");

        // act
        await sut.Merge(main, secondary, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFilesAreIdentical(target, expected);
    }

    [IgnoreLocallyFact]
    public async Task Remove_removes_entries()
    {
        // arrange
        var main = GetTestPath("csv", "csv-set.csv");
        var secondary = GetTestPath("csv", "csv-secondary.csv");
        var target = GetTestPath("csv", "csv-remove_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("csv", "csv-remove_expected.csv");

        // act
        await sut.Remove(main, secondary, target, messageHandler);

        // assert
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).MustNotHaveHappened();
        await AssertFilesAreIdentical(target, expected);
    }
}