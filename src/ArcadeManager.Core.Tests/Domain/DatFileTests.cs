using ArcadeManager.Core.Domain;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Tests.Domain;

public class DatFileTests : BaseUnitTests
{
    private readonly DatFile sut;

    public DatFileTests()
    {
        sut = new();
    }

    [Fact]
    public async Task GetRoms_lists_roms()
    {
        // arrange: file path
        var datPath = GetTestPath("Domain", "datfile.xml");
        using var stream = File.OpenText(datPath);

        // act
        var result = await sut.GetRoms(stream, "romset");

        // assert: list
        result.Should().HaveCount(4);

        // assert: game with clone
        result["1944"].Should().NotBeNull();
        result["1944"].RomFiles.Should().HaveCount(15);
        result["1944"].Clones.Should().HaveCount(1);
        result["1944"].Parent.Should().BeNull();
        result["1944"].Bios.Should().BeNull();

        // assert: clone
        result["1944j"].Should().NotBeNull();
        result["1944j"].Parent.Name.Should().Be("1944");
        result["1944j"].Bios.Should().BeNull();

        // assert: game with bios
        result["aof"].Should().NotBeNull();
        result["aof"].Parent.Should().BeNull();
        result["aof"].Bios.Name.Should().Be("neogeo");

        // assert: bios
        result["neogeo"].Should().NotBeNull();
        result["neogeo"].Parent.Should().BeNull();
        result["neogeo"].Bios.Should().BeNull();
        result["neogeo"].Clones.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ReadDatFile_reads_file()
    {
        // arrange: files
        var source = GetTestPath("Domain", "datfile.xml");
        using var streamReader = File.OpenText(source);

        // act
        var result = await sut.ReadDatFile(streamReader);

        // assert
        result.Entries.Should().NotBeNull().And.HaveCount(4);
        result.Entries[0].Name.Should().Be("1944");
        result.Entries[1].Name.Should().Be("1944j");
        result.Entries[2].Name.Should().Be("aof");
        result.Entries[3].Name.Should().Be("neogeo");
    }
}