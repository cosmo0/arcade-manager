using ArcadeManager.Core.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Domain;

public class CsvFileTests : TestBase
{
    private readonly Core.Domain.CsvFile sut;

    public CsvFileTests() : base()
    {
        sut = new();
    }

    [Fact]
    public async Task ReadFile_with_headers_returns_data()
    {
        // arrange
        using var stream = File.OpenText(GetTestPath("Domain", "csvfile-readfile.csv"));

        // act
        var result = await sut.ReadFile(stream, true);

        // assert
        result.Headers.Should().HaveCount(2);
        result.Games.Should().HaveCount(3);
        result.Games.ElementAt(0).Name.Should().Be("game1");
        result.Games.ElementAt(0).Values.Should().HaveCount(2);
        result.Games.ElementAt(1).Name.Should().Be("game2");
        result.Games.ElementAt(1).Values.Should().HaveCount(2);
        result.Games.ElementAt(2).Name.Should().Be("game3");
        result.Games.ElementAt(2).Values.Should().HaveCount(2);
    }

    [Fact]
    public async Task ReadFile_without_headers_returns_data_without_headers()
    {
        // arrange
        using var stream = File.OpenText(GetTestPath("Domain", "csvfile-readfile-noheaders.csv"));

        // act
        var result = await sut.ReadFile(stream, true);

        // assert
        result.Headers.Should().HaveCount(0);
        result.Games.Should().HaveCount(3);
        result.Games.ElementAt(0).Name.Should().Be("game1");
        result.Games.ElementAt(0).Values.Should().HaveCount(0);
        result.Games.ElementAt(1).Name.Should().Be("game2");
        result.Games.ElementAt(1).Values.Should().HaveCount(0);
        result.Games.ElementAt(2).Name.Should().Be("game3");
        result.Games.ElementAt(2).Values.Should().HaveCount(0);
    }

    [Fact]
    public async Task WriteFile_outputs_file()
    {
        // arrange: outfile
        var outFile = GetTestPath("Domain", "out.csv");

        // arrange: data
        var data = new CsvGamesList();
        data.Add("game1", new() { { "valuea", "a" } });
        data.Add("game2", new() { { "valuea", "b" } });

        // arrange: stream
        var outStream = new StreamWriter(new FileStream(outFile, FileMode.Create, FileAccess.Write));

        // act
        await sut.WriteFile(outStream, data);
        await outStream.DisposeAsync(); // force close the output stream

        var result = await File.ReadAllLinesAsync(outFile, TestContext.Current.CancellationToken);

        // assert
        result.Should().HaveCount(data.Games.Count + 1);
    }
}