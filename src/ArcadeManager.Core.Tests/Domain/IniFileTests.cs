using ArcadeManager.Core.Domain;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Domain;

public class IniFileTests : BaseUnitTests
{
    [Fact]
    public async Task ReadIniFile_returns_data()
    {
        // arrange: sut
        var sut = new IniFile();

        // arrange: data
        var testFile = GetTestPath("Domain", "category.ini");
        using var stream = File.OpenText(testFile);

        // act
        var result = await sut.ReadIniFile(stream);

        // assert
        result.Keys.Should().BeEquivalentTo("EMPTY_SECTION", "SECTION A", "SECTION B");
        result["EMPTY_SECTION"].Should().HaveCount(0);
        result["SECTION A"].Should().HaveCount(3);
        result["SECTION B"].Should().HaveCount(2);
    }
}