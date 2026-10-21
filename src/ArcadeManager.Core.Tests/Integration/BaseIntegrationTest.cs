using ArcadeManager.Core.Domain;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Roms;
using System;
using System.IO;
using System.Reflection;

namespace ArcadeManager.Core.Tests.Integration;

public abstract class BaseIntegrationTest : BaseCommonTests
{
    protected readonly string basePath;
    protected readonly CsvFile csvFile;
    protected readonly DatFile datFile;
    protected readonly Downloader downloader;
    protected readonly IEnvironment env;
    protected readonly FileSystem fs;
    protected readonly ImageProcessor imageProcessor;
    protected readonly IniFile iniFile;
    protected readonly ILocalizer localizer;
    protected readonly MameOverlay mameOverlay;
    protected readonly IMessageHandler messageHandler;
    protected readonly RetroArchOverlay retroarchOverlay;
    protected readonly IWebClientFactory webClientFactory;

    protected BaseIntegrationTest()
    {
        basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        // the only property of environment that fs needs is GetBasePath
        env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetBasePath()).Returns(basePath);

        // localizer returns the code
        localizer = A.Fake<ILocalizer>();
        A.CallTo(() => localizer.CultureName(A<string>._)).Returns("test");
        A.CallTo(() => localizer.IsCurrentLocale(A<string>._)).Returns(true);
        A.CallTo(() => localizer.CurrentLocale()).Returns("test");
        A.CallTo(() => localizer.Locales).Returns(["test"]);
        A.CallTo(() => localizer[A<string>._]).ReturnsLazily((string code) => code);

        // message handler that we can inspect and mock
        messageHandler = A.Fake<IMessageHandler>();

        // init infra
        fs = new FileSystem(env);
        webClientFactory = new WebClientFactory();

        // init domain
        csvFile = new CsvFile();
        iniFile = new IniFile();
        datFile = new DatFile();
        imageProcessor = new ImageProcessor();
        retroarchOverlay = new RetroArchOverlay(fs);
        mameOverlay = new MameOverlay(fs);

        // init common services
        downloader = new Downloader(webClientFactory, fs, localizer);
    }

    /// <summary>
    /// Gets the a file in the "data" test folder for integration tests.
    /// </summary>
    /// <param name="pathItems">The path items (missing the data\integration part).</param>
    /// <returns>
    /// The concatenated file name
    /// </returns>
    protected string GetTestPath(params string[] pathItems)
    {
        return Path.Combine([basePath, "Data", "Integration", .. pathItems]);
    }
}