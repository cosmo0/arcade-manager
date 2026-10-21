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

        // message handler that outputs to the console and that we can inspect to check if it's been called
        messageHandler = A.Fake<IMessageHandler>();
        A.CallTo(() => messageHandler.ProgressInit(A<string>._)).Invokes((string label) => { Console.WriteLine($"progress init: {label}"); });
        A.CallTo(() => messageHandler.Progress(A<string>._)).Invokes((string label) => { Console.WriteLine($"progress: {label}"); });
        A.CallTo(() => messageHandler.Progress(A<string>._, A<int>._, A<int>._)).Invokes((string label, int total, int current) => { Console.WriteLine($"progress: {label} // {current}/{total}"); });
        A.CallTo(() => messageHandler.ProgressMessage(A<string>._)).Invokes((string label) => { Console.WriteLine($"progress message: {label}"); });
        A.CallTo(() => messageHandler.ProgressProcessed(A<GameRom>._)).Invokes((GameRom rom) => { Console.WriteLine($"progress processed rom: {rom.Name}"); });
        A.CallTo(() => messageHandler.ProgressDone(A<string>._, A<string>._)).Invokes((string label, string folder) => { Console.WriteLine($"progress done: {label} // {folder}"); });
        A.CallTo(() => messageHandler.ProgressError(A<Exception>._)).Invokes((Exception ex) => { Console.WriteLine($"progress error: {ex.Message}"); });

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