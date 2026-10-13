using ArcadeManager.Core.Infrastructure.Interfaces;
using FakeItEasy;
using System.IO;
using System.Reflection;

namespace ArcadeManager.Core.Tests;

public class TestBase
{
    protected readonly IFileSystem fs;
    protected readonly string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    protected readonly IMessageHandler messageHandler;

    public TestBase()
    {
        this.fs = A.Fake<IFileSystem>();
        this.messageHandler = A.Fake<IMessageHandler>();
    }

    /// <summary>
    /// Gets the a file in the "data" test folder.
    /// </summary>
    /// <param name="folder">The sub-folder name.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The concatenated file name</returns>
    protected string GetTestFile(string folder, string fileName)
    {
        return Path.Combine(here, "Data", folder, fileName);
    }
}