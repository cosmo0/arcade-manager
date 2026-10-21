using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using System.IO;
using System.Reflection;

namespace ArcadeManager.Core.Tests;

public abstract class BaseUnitTests : BaseCommonTests
{
    protected readonly IFileSystem fs;
    protected readonly string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    protected readonly IMessageHandler messageHandler;
    protected readonly IFileSystem realfs;

    protected BaseUnitTests()
    {
        // some tests are much easier to write using the real fs layer
        this.fs = A.Fake<IFileSystem>();
        this.realfs = new FileSystem(A.Fake<IEnvironment>());

        this.messageHandler = A.Fake<IMessageHandler>();
    }

    /// <summary>
    /// Gets the a file in the "data" test folder.
    /// </summary>
    /// <param name="folder">The sub-folder name.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The concatenated file name</returns>
    protected string GetTestPath(string folder, string fileName)
    {
        return Path.Combine(here, "Data", folder, fileName);
    }
}