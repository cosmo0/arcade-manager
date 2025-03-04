using System;
using ArcadeManager;
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
        builder.AddSingleton<Infrastructure.IWebClientFactory, Infrastructure.WebClientFactory>();
        builder.AddSingleton<Infrastructure.IFileSystem, Infrastructure.FileSystem>();
        builder.AddSingleton<Infrastructure.IDatFile, Infrastructure.DatFile>();

        // services
        builder.AddSingleton<Services.IDownloader, Services.Downloader>();
        builder.AddSingleton<Services.ICsv, Services.Csv>();
        builder.AddSingleton<Services.IOverlays, Services.Overlays>();
        builder.AddSingleton<Services.IRoms, Services.Roms>();
        builder.AddSingleton<Services.IUpdater, Services.Updater>();
        builder.AddSingleton<Services.ILocalizer, Services.Localizer>();
        builder.AddSingleton<Services.IWizard, Services.Wizard>();
        builder.AddSingleton<Services.IDatChecker, Services.DatChecker>();
        builder.AddSingleton<Services.IServiceProvider, Services.ServiceProvider>();

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
