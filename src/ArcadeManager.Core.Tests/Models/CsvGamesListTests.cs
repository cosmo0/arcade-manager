using ArcadeManager.Core.Models;
using AwesomeAssertions;

namespace ArcadeManager.Core.Tests.Models;

public class CsvGamesListTests : TestBase
{
    [Fact]
    public void CopyFrom_copies_values_when_not_already_exist()
    {
        // arrange
        var list = new CsvGamesList();
        var otherList = new CsvGamesList();

        var sut = new CsvGamesList.GameEntry(list, "test", new Dictionary<string, string>() { { "a", "avalue" } });
        list.Add(sut);

        var additional = new CsvGamesList.GameEntry(otherList, "other", new Dictionary<string, string>() { { "b", "bvalue" } });
        otherList.Add(additional);

        // act
        sut.CopyFrom(additional);

        // assert
        sut.Values.Should().HaveCount(2);
        sut.Values["b"].Should().Be("bvalue");
    }

    [Fact]
    public void CopyFrom_does_not_copy_values_when_already_exist()
    {
        // arrange
        var list = new CsvGamesList();
        var otherList = new CsvGamesList();

        var sut = new CsvGamesList.GameEntry(list, "test", new Dictionary<string, string>() { { "a", "avalue" } });
        list.Add(sut);

        var additional = new CsvGamesList.GameEntry(otherList, "other", new Dictionary<string, string>() { { "a", "bvalue" } });
        otherList.Add(additional);

        // act
        sut.CopyFrom(additional);

        // assert
        sut.Values.Should().HaveCount(1);
        sut.Values["a"].Should().Be("avalue");
    }
}