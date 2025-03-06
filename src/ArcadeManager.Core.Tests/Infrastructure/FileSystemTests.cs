using System;
using System.Reflection;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models.Roms;
using FakeItEasy;
using FluentAssertions;

namespace ArcadeManager.Core.Tests.Infrastructure;

public class FileSystemTests
{
    private readonly string filesPath;
    private readonly IEnvironment environment;
    private readonly FileSystem sut;

    public FileSystemTests()
    {
        string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        filesPath = Path.Combine(here, "Data", "Infrastructure");

        this.environment = A.Fake<IEnvironment>();

        this.sut = new FileSystem(this.environment);
    }

    [Fact]
    public void Zip_files_are_listed_without_sha1()
    {
        // arrange
        var zip = Path.Combine(filesPath, "test1.zip");

        // act
        var result = sut.GetZipFiles(zip, false);

        // assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Zip_files_are_listed_with_sha1()
    {
        // arrange
        var zip = Path.Combine(filesPath, "test1.zip");

        // act
        var result = sut.GetZipFiles(zip, true);

        // assert
        result.Should().HaveCount(2);
        result.ElementAt(0).Sha1.Should().NotBeNullOrEmpty();
        result.ElementAt(1).Sha1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Zip_files_are_listed_with_folder()
    {
        // arrange
        var zip = Path.Combine(filesPath, "testfolder.zip");

        // act
        var result = sut.GetZipFiles(zip, false);

        // assert
        result.Should().HaveCount(3);
        result.Any(g => g.Name == "test1.txt").Should().BeTrue();
        result.First(g => g.Name == "test1.txt").Path.Should().BeNullOrEmpty();
        result.Any(g => g.Name == "test2.txt").Should().BeTrue();
        result.First(g => g.Name == "test2.txt").Path.Should().BeNullOrEmpty();
        result.Any(g => g.Name == "test3.txt").Should().BeTrue();
        result.First(g => g.Name == "test3.txt").Path.Should().Be("testfolder");
    }

    [Fact]
    public async Task Zip_files_are_replaced()
    {
        // arrange: paths
        var original = Path.Combine(filesPath, "test1.zip");
        var sourceZip = Path.Combine(filesPath, "test2.zip");
        var targetZip = Path.Combine(filesPath, "test1-copy.zip");

        // arrange: file data
        var sourceFile = new GameRomFile {
            Name = "test1.txt",
            Crc = "8ab2dce2"
        };

        // arrange: cleanup
        if (File.Exists(targetZip)) {
            File.Delete(targetZip);
        }

        // arrange: copy original to target file
        File.Copy(original, targetZip);

        // arrange: open the zip
        using var sourceZipFile = sut.OpenZipWrite(sourceZip);
        using var targetZipFile = sut.OpenZipWrite(targetZip);

        // act
        var replaced = await sut.ReplaceZipFile(sourceZipFile, targetZipFile, sourceFile);
        
        // act a second time to simulate what can happen in the rebuild process
        var replacedTwice = await sut.ReplaceZipFile(sourceZipFile, targetZipFile, sourceFile);

        // assert: file has been replaced
        replaced.Should().BeTrue();
        replacedTwice.Should().BeTrue();

        // assert: read the new zip content
        var fileInZip = targetZipFile.GetEntry("test1.txt");
        fileInZip.Should().NotBeNull();
        
        var data = await new StreamReader(fileInZip!.Open()).ReadToEndAsync();
        data.Should().Be("test1 modified");
    }

    [Fact]
    public void Zip_files_are_deleted()
    {
        // arrange
        var targetZip = Path.Combine(filesPath, "testdelete-copy.zip");
        File.Copy(Path.Combine(filesPath, "testdelete.zip"), targetZip, true);

        // arrange: files list
        GameRomFilesList files = [
            new() { Name = "test1.txt" },
            new() { Name = "test3.txt", Path = "testfolder" }
        ];

        // arrange: read zip
        using (var zip = sut.OpenZipWrite(targetZip)) {
            // act
            foreach (var file in files) {
                sut.DeleteZipFile(zip, file);
            }

            sut.DeleteZipFile(zip, new GameRomFile { Name = "testfolder/" });
        }

        // assert: re-read zip and check remaining files
        using var zipRead = sut.OpenZipRead(targetZip);
        zipRead.Entries.Should().HaveCount(2);
        zipRead.GetEntry("test2.txt").Should().NotBeNull();
        zipRead.GetEntry("test3.txt").Should().NotBeNull();
    }
}
