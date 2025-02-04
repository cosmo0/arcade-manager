using System;
using System.Reflection;
using ArcadeManager.Infrastructure;
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
        var copyTo = Path.Combine(filesPath, "test1-copy.zip");
        var copyFrom = Path.Combine(filesPath, "test2.zip");

        // arrange: cleanup
        if (File.Exists(copyTo)) {
            File.Delete(copyTo);
        }

        // arrange: copy target file
        File.Copy(original, copyTo);

        // act
        await sut.ReplaceZipFile(copyFrom, copyTo, "test1.txt");

        // assert: read the new zip content
        using var zip = System.IO.Compression.ZipFile.OpenRead(copyTo);
        var file = zip.GetEntry("test1.txt");
        file.Should().NotBeNull();
        
        var data = new StreamReader(file!.Open()).ReadToEnd();
        data.Should().Be("test1 modified");
    }
}
