using System;
using ArcadeManager.Core;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Services;
using ArcadeManager.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace ArcadeManager.Console;

// copied from github.com/spectreconsole/examples/blob/main/examples/Cli/Injection/Infrastructure/TypeRegistrar.cs
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection builder;

    public TypeRegistrar()
    {
        builder = new ServiceCollection();

        // environment
        builder.AddSingleton<IEnvironment, ConsoleEnvironment>();

        // infrastructure
        builder.AddSingleton<IWebClientFactory, WebClientFactory>();
        builder.AddSingleton<IFileSystem, FileSystem>();
        builder.AddSingleton<IDatFile, DatFile>();

        // services
        builder.AddSingleton<IDownloader, Downloader>();
        builder.AddSingleton<ICsv, Csv>();
        builder.AddSingleton<IOverlays, Overlays>();
        builder.AddSingleton<IRoms, Roms>();
        builder.AddSingleton<IUpdater, Updater>();
        builder.AddSingleton<ILocalizer, Localizer>();
        builder.AddSingleton<IWizard, Wizard>();
        builder.AddSingleton<IDatChecker, DatChecker>();
        builder.AddSingleton<Core.Services.Interfaces.IServiceProvider, Core.Services.ServiceProvider>();

        builder.AddSingleton<IMessageHandler, ConsoleMessageHandler>();
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        builder.AddSingleton(service, (provider) => factory());
    }
}
