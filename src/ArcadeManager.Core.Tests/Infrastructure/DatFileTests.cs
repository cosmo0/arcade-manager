using System;
using System.Reflection;
using ArcadeManager.Infrastructure;
using FakeItEasy;
using FluentAssertions;

namespace ArcadeManager.Core.Tests.Infrastructure;

public class DatFileTests
{
    private readonly string filesPath;
    private readonly IEnvironment environment;
    private readonly DatFile sut;

    public DatFileTests()
    {
        string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        this.filesPath = Path.Combine(here, "Data", "Infrastructure");

        this.environment = A.Fake<IEnvironment>();

        // we'll inject the real filesystem service because faking the async stream reader is WAY too complicated
        this.sut = new DatFile(new FileSystem(this.environment));
    }

    [Fact]
    public async Task Datfile_is_read()
    {
        // arrange: file path
        var datPath = Path.Combine(filesPath, "datfile.xml");

        // act
        var result = await sut.GetRoms(datPath);

        // assert: list
        result.Should().HaveCount(4);

        // assert: game with clone
        result["1944"].Should().NotBeNull();
        result["1944"].RomFiles.Should().HaveCount(15);
        result["1944"].Clones.Should().HaveCount(1);
        result["1944"].CloneOf.Should().BeNull();
        result["1944"].Bios.Should().BeNull();

        // assert: clone
        result["1944j"].Should().NotBeNull();
        result["1944j"].CloneOf.Name.Should().Be("1944");
        result["1944j"].Bios.Should().BeNull();

        // assert: game with bios
        result["aof"].Should().NotBeNull();
        result["aof"].CloneOf.Should().BeNull();
        result["aof"].Bios.Name.Should().Be("neogeo");

        // assert: bios
        result["neogeo"].Should().NotBeNull();
        result["neogeo"].CloneOf.Should().BeNull();
        result["neogeo"].Bios.Should().BeNull();
        result["neogeo"].Clones.Should().BeNullOrEmpty();
    }
}
