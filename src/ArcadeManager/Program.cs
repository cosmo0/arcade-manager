using ArcadeManager.Core;
using ArcadeManager.Core.Exceptions;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Services;
using ArcadeManager.Core.Services.Interfaces;
using ArcadeManager.Services;
using ArcadeManager.Services.Interfaces;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ArcadeManager;

/// <summary>
/// Base program entry point
/// </summary>
public static class Program
{
    private static readonly Lock messageHandlerLock = new();
    private static IElectronMessageHandler messageHandler;

    /// <summary>
    /// Gets the message handler.
    /// </summary>
    /// <param name="container">The DI container.</param>
    /// <returns>The message handler</returns>
    public static IElectronMessageHandler GetMessageHandler(SimpleInjector.Container container)
    {
        if (container != null)
        {
            lock (messageHandlerLock)
            {
                messageHandler ??= new ElectronMessageHandler(
                        container.GetInstance<ICsv>(),
                        container.GetInstance<IDatChecker>(),
                        container.GetInstance<IDownloader>(),
                        container.GetInstance<IOverlays>(),
                        container.GetInstance<IRoms>(),
                        container.GetInstance<IUpdater>(),
                        container.GetInstance<IEnvironment>(),
                        container.GetInstance<IFileSystem>());
            }
        }

        if (container == null && messageHandler == null)
        {
            throw new StartupException("No DI container provided and message handler not initialized");
        }

        return messageHandler;
    }

    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static async Task Main(string[] args)
    {
        Localizer.EnsureLocale();

        var builder = Host.CreateDefaultBuilder(args);

        await builder
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseElectron(args, StartupElectron.CreateMainWindow);
                webBuilder.UseStartup<Startup>();
            })
            .Build()
            .RunAsync();
    }
}