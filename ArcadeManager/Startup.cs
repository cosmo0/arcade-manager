using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace ArcadeManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            if (HybridSupport.IsElectronActive)
            {
                ElectronBootstrap();
            }
        }

        /// <summary>
        /// Initializes the Electron app
        /// </summary>
        public async void ElectronBootstrap()
        {
            BuildAppMenu();

            var mainWindow = await CreateMainWindow();

            // re-create main window if last window has been closed
            Electron.App.On("activate", obj => {
                var hasWindows = (bool)obj;

                if (!hasWindows)
                {
                    mainWindow = Task.Run(CreateMainWindow).Result;
                }
                else
                {
                    mainWindow?.Show();
                }
            });

            // initializes RPC message handling
            Behavior.MessageHandler.InitMessageHandling(mainWindow);
        }

        /// <summary>
        /// Creates the main browser window
        /// </summary>
        /// <returns>The main browser window</returns>
        private async Task<BrowserWindow> CreateMainWindow()
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 1280,
                Height = 800,
                Show = true,
                Resizable = true
            });

            await browserWindow.WebContents.Session.ClearCacheAsync();

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle("Arcade Manager");

            return browserWindow;
        }

        /// <summary>
        /// Builds the application menus
        /// </summary>
        private void BuildAppMenu()
        {
            var menu = new MenuItem[]
            {
                // App name menu
                new MenuItem
                {
                    Label = "Prefix",
                    Submenu = new MenuItem[]
                    {
                        new MenuItem { Role = MenuRole.about },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem { Role = MenuRole.hide },
                        new MenuItem { Role = MenuRole.hideothers },
                        new MenuItem { Type = MenuType.separator },
                        new MenuItem { Role = MenuRole.quit }
                    }
                },

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

                // View
                new MenuItem {
                    Label = "View",
                    Type = MenuType.submenu,
                    Submenu = new MenuItem[] {
                        new MenuItem
                        {
                            Label = "Reload",
                            Accelerator = "CmdOrCtrl+R",
                            Click = () =>
                            {
                                // on reload, start fresh and close any old
                                // open secondary windows
                                Electron.WindowManager.BrowserWindows.ToList().ForEach(browserWindow => {
                                    if(browserWindow.Id != 1)
                                    {
                                        browserWindow.Close();
                                    }
                                    else
                                    {
                                        browserWindow.Reload();
                                    }
                                });
                            }
                        },
                        new MenuItem
                        {
                            Label = "Open Developer Tools",
                            Accelerator = "CmdOrCtrl+I",
                            Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                        }
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
    }
}
