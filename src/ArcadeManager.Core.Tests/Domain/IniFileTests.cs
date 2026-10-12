using ArcadeManager.Core.Domain;
using AwesomeAssertions;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeManager.Core.Tests.Domain;

public class IniFileTests : TestBase
{
    [Fact]
    public async Task ReadIniFile_returns_data()
    {
        // arrange: sut
        var sut = new IniFile(fs);

        // arrange: data
        var testFile = GetTestFile("Domain", "category.ini");
        using var stream = File.OpenText(testFile);
        A.CallTo(() => fs.GetReadFileStream(A<string>._)).Returns(stream);

        // act
        var result = await sut.ReadIniFile(testFile);

        // assert
        result.Keys.Should().BeEquivalentTo("EMPTY_SECTION", "SECTION A", "SECTION B");
        result["EMPTY_SECTION"].Should().HaveCount(0);
        result["SECTION A"].Should().HaveCount(3);
        result["SECTION B"].Should().HaveCount(2);
    }
}