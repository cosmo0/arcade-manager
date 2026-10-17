using ArcadeManager.Core.Domain;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Services;

public class CsvTests : TestBase
{
    private readonly Core.Services.Csv sut;

    public CsvTests() : base()
    {
        // rather than mocking a lot of complex methods, let's simply use the actual files (that are already tested elsewhere)
        sut = new(realfs, new CsvFile(), new IniFile());
    }

    [Fact]
    public async Task ConvertDat_converts_dat()
    {
        // arrange
        var main = GetTestPath("Services", "datfile.xml");
        var target = GetTestPath("Services", "datfile_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("Services", "datfile_expected.csv");

        // act
        await sut.ConvertDat(main, target, messageHandler);

        // assert
        File.Exists(target).Should().BeTrue();
        var targetContent = await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken);
        var expectedContent = await File.ReadAllTextAsync(expected, TestContext.Current.CancellationToken);
        targetContent.Should().BeEquivalentTo(expectedContent);
    }

    [Fact]
    public async Task ConvertIni_converts_ini()
    {
        // arrange
        var main = GetTestPath("Services", "category.ini");
        var target = GetTestPath("Services", "ini_result");
        CleanupFolder(target, true);
        var expected = GetTestPath("Services", "ini_expected");

        // act
        await sut.ConvertIni(main, target, messageHandler);

        // assert
        foreach (var f in Directory.GetFileSystemEntries(target))
        {
            var fileName = Path.GetFileName(f);
            var expectedFile = Path.Combine(expected, fileName);
            await AssertFilesAreIdentical(f, expectedFile);
        }
    }

    [Fact]
    public async Task Keep_keeps_only_listed_entries()
    {
        // arrange
        var main = GetTestPath("Services", "set.csv");
        var secondary = GetTestPath("Services", "secondary.csv");
        var target = GetTestPath("Services", "keep_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("Services", "keep_expected.csv");

        // act
        await sut.Keep(main, secondary, target, messageHandler);

        // assert
        await AssertFilesAreIdentical(target, expected);
    }

    [Fact]
    public async Task ListFiles_lists_files_in_folder()
    {
        // arrange
        var main = GetTestPath("Services", "files");
        var target = GetTestPath("Services", "list_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("Services", "list_expected.csv");

        // act
        await sut.ListFiles(main, target, messageHandler);

        // assert
        await AssertFilesAreIdentical(target, expected);
    }

    [Fact]
    public async Task Merge_merges_two_lists()
    {
        // arrange
        var main = GetTestPath("Services", "set.csv");
        var secondary = GetTestPath("Services", "secondary.csv");
        var target = GetTestPath("Services", "merge_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("Services", "merge_expected.csv");

        // act
        await sut.Merge(main, secondary, target, messageHandler);

        // assert
        await AssertFilesAreIdentical(target, expected);
    }

    [Fact]
    public async Task Remove_removes_entries()
    {
        // arrange
        var main = GetTestPath("Services", "set.csv");
        var secondary = GetTestPath("Services", "secondary.csv");
        var target = GetTestPath("Services", "remove_result.csv");
        CleanupFile(target);
        var expected = GetTestPath("Services", "remove_expected.csv");

        // act
        await sut.Remove(main, secondary, target, messageHandler);

        // assert
        await AssertFilesAreIdentical(target, expected);
    }
}