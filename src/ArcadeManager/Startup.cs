using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ArcadeManager;

/// <summary>
/// Startup app
/// </summary>
public class Startup {
    private readonly Container container = new Container();

    private IWebHostEnvironment env;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the application pipeline.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        this.env = env;

        app.UseSimpleInjector(container);

        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        try {
            container.Verify();
        }
        catch (Exception ex) {
            Console.WriteLine($"An error has occurred during injection: {ex.Message}");
            Environment.Exit(1);
        }

        if (HybridSupport.IsElectronActive) {
            ElectronBootstrap().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services) {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options => {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("fr")
            };

            options.DefaultRequestCulture = new RequestCulture("en", "en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        services.AddControllersWithViews();

        services.AddLogging();

        // bind SimpleInjector to .Net injection
        services.AddSimpleInjector(container, options => {
            options.AddAspNetCore().AddControllerActivation();
            options.AddLocalization();
            options.AddLogging();
        });

        this.InitializeInjection(services);
    }

    /// <summary>
    /// Initializes the Electron app
    /// </summary>
    public async Task ElectronBootstrap() {
        BuildAppMenu();

        var mainWindow = await CreateMainWindow();

        // re-create main window if last window has been closed
        await Electron.App.On("activate", obj => {
            var hasWindows = (bool)obj;

            if (!hasWindows) {
                mainWindow = Task.Run(CreateMainWindow).Result;
            }
            else {
                mainWindow?.Show();
            }
        });

        // initializes RPC message handling
        var msgHandler = container.GetInstance<IElectronMessageHandler>();
        msgHandler?.Handle(mainWindow);
    }

    /// <summary>
    /// Builds the application menus
    /// </summary>
    private static void BuildAppMenu() {
        static MenuItem firstMenu() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return new MenuItem {
                    Label = "ArcadeManager",
                    Submenu = new MenuItem[]
                    {
                        new MenuItem { Role = MenuRole.about },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem
                        {
                            Label = "Open Developer Tools",
                            Accelerator = "CmdOrCtrl+I",
                            Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                        },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem { Role = MenuRole.hide },
                        new MenuItem { Role = MenuRole.hideothers },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem { Role = MenuRole.quit }
                    }
                };
            }
            else {
                return new MenuItem {
                    Label = "File",
                    Submenu = new MenuItem[]
                    {
                        new MenuItem { Role = MenuRole.about },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem
                        {
                            Label = "Open Developer Tools",
                            Accelerator = "CmdOrCtrl+I",
                            Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                        },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem { Role = MenuRole.quit }
                    }
                };
            }
        }

        var menu = new MenuItem[]
        {
			// App name/file menu
			firstMenu(),

			// Edit
			new MenuItem {
                Label = "Edit",
                Type = MenuType.submenu,
                Submenu = new MenuItem[] {
                    new MenuItem { Label = "Undo", Accelerator = "CmdOrCtrl+Z", Role = MenuRole.undo },
                    new MenuItem { Label = "Redo", Accelerator = "Shift+CmdOrCtrl+Z", Role = MenuRole.redo },
                    new MenuItem { Type = MenuType.separator },
                    new MenuItem { Label = "Cut", Accelerator = "CmdOrCtrl+X", Role = MenuRole.cut },
                    new MenuItem { Label = "Copy", Accelerator = "CmdOrCtrl+C", Role = MenuRole.copy },
                    new MenuItem { Label = "Paste", Accelerator = "CmdOrCtrl+V", Role = MenuRole.paste },
                }
            },

			// Window
			new MenuItem {
                Label = "Window",
                Role = MenuRole.window,
                Type = MenuType.submenu,
                Submenu = new MenuItem[] {
                    new MenuItem { Label = "Minimize", Accelerator = "CmdOrCtrl+M", Role = MenuRole.minimize },
                    new MenuItem { Label = "Close", Accelerator = "CmdOrCtrl+W", Role = MenuRole.close }
                }
            },

			// Help
			new MenuItem {
                Label = "Help",
                Role = MenuRole.help,
                Type = MenuType.submenu,
                Submenu = new MenuItem[] {
                    new MenuItem
                    {
                        Label = "Learn More",
                        Click = async () => await Electron.Shell.OpenExternalAsync("https://github.com/cosmo0/arcade-manager/")
                    }
                }
            }
        };

        Electron.Menu.SetApplicationMenu(menu);
    }

    /// <summary>
    /// Creates the main browser window
    /// </summary>
    /// <returns>The main browser window</returns>
    private async Task<BrowserWindow> CreateMainWindow() {
        var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions {
            Width = 1280,
            Height = 800,
            Show = true,
            Resizable = true
        });

        await browserWindow.WebContents.Session.ClearCacheAsync();

        browserWindow.OnReadyToShow += () => browserWindow.Show();
        browserWindow.SetTitle("Arcade Manager");

        if (this.env.IsDevelopment()) {
            browserWindow.WebContents.OpenDevTools();
        }

        return browserWindow;
    }

    /// <summary>
    /// Configures the dependency injection.
    /// </summary>
    private void InitializeInjection(IServiceCollection services) {
        try {
            // environment
            container.Register<IEnvironment, ArcadeManagerEnvironment>(Lifestyle.Singleton);

            // infrastructure
            container.Register<Infrastructure.IWebClientFactory, Infrastructure.WebClientFactory>(Lifestyle.Singleton);
            container.Register<Infrastructure.IFileSystem, Infrastructure.FileSystem>(Lifestyle.Singleton);
            container.Register<Infrastructure.IDatFile, Infrastructure.DatFile>(Lifestyle.Singleton);

            // services
            container.Register<Services.IDownloader, Services.Downloader>(Lifestyle.Singleton);
            container.Register<Services.ICsv, Services.Csv>(Lifestyle.Singleton);
            container.Register<Services.IOverlays, Services.Overlays>(Lifestyle.Singleton);
            container.Register<Services.IRoms, Services.Roms>(Lifestyle.Singleton);
            container.Register<Services.IUpdater, Services.Updater>(Lifestyle.Singleton);
            container.Register<Services.ILocalizer, Services.Localizer>(Lifestyle.Singleton);
            container.Register<Services.IWizard, Services.Wizard>(Lifestyle.Singleton);
            container.Register<Services.IDatChecker, Services.DatChecker>(Lifestyle.Singleton);

            // message handler (SimpleInjector returns the same singleton if it's the same implementation)
            container.Register<IMessageHandler, ElectronMessageHandler>(Lifestyle.Singleton);
            container.Register<IElectronMessageHandler, ElectronMessageHandler>(Lifestyle.Singleton);

            // view localization uses dotnet tooling
            services.AddSingleton(provider => container.GetInstance<Services.ILocalizer>());
        }
        catch (Exception ex) {
            Console.WriteLine($"An error has occurred during injection: {ex.Message}");
            Environment.Exit(1);
        }
    }
}