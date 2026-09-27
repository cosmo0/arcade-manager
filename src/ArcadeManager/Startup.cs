using ArcadeManager.Core;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Services;
using ArcadeManager.Core.Services.Interfaces;
using ArcadeManager.Services;
using ArcadeManager.Services.Interfaces;
using ElectronNET.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using System;

namespace ArcadeManager;

/// <summary>
/// Startup app
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Startup"/> class.
/// </remarks>
/// <param name="configuration">The configuration.</param>
public class Startup(IConfiguration configuration)
{
    private readonly Container container = new();

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public IConfiguration Configuration { get; } = configuration;

    /// <summary>
    /// Configures the application pipeline.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSimpleInjector(container);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        try
        {
            container.Verify();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred during injection: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = Localizer.GetSupportedCultures();

            options.DefaultRequestCulture = new RequestCulture("en", "en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        services.AddControllersWithViews();

        services.AddLogging();

        // bind SimpleInjector to .Net injection
        services.AddSimpleInjector(container, options =>
        {
            options.AddAspNetCore().AddControllerActivation();
            options.AddLocalization();
            options.AddLogging();
        });

        services.AddElectron();

        this.InitializeInjection(services);
    }

    /// <summary>
    /// Configures the dependency injection.
    /// </summary>
    private void InitializeInjection(IServiceCollection services)
    {
        try
        {
            // environment
            container.Register<IEnvironment, ArcadeManagerEnvironment>(Lifestyle.Singleton);

            // infrastructure
            container.Register<IWebClientFactory, WebClientFactory>(Lifestyle.Singleton);
            container.Register<IFileSystem, FileSystem>(Lifestyle.Singleton);
            container.Register<IDatFile, DatFile>(Lifestyle.Singleton);
            container.Register<IImageProcessor, ImageProcessor>(Lifestyle.Singleton);

            // core services
            container.Register<IDownloader, Downloader>(Lifestyle.Singleton);
            container.Register<ICsv, Csv>(Lifestyle.Singleton);
            container.Register<IOverlays, Overlays>(Lifestyle.Singleton);
            container.Register<IRoms, Roms>(Lifestyle.Singleton);
            container.Register<ILocalizer, Localizer>(Lifestyle.Singleton);
            container.Register<IDatChecker, DatChecker>(Lifestyle.Singleton);
            container.Register<Core.Services.Interfaces.IServiceProvider, Core.Services.ServiceProvider>(Lifestyle.Singleton);

            // app services
            container.Register<IWizard, Wizard>(Lifestyle.Singleton);
            container.Register<IUpdater, Updater>(Lifestyle.Singleton);

            // message handler (SimpleInjector returns the same singleton if it's the same implementation)
            container.Register<IMessageHandler>(() => { return Program.GetMessageHandler(container); });
            container.Register<IElectronMessageHandler>(() => { return Program.GetMessageHandler(container); });

            // view localization uses dotnet tooling
            services.AddSingleton(provider => container.GetInstance<ILocalizer>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred during injection: {ex.Message}");
            Environment.Exit(1);
        }
    }
}