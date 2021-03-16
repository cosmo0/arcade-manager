using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ArcadeManager.Models;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace ArcadeManager.Controllers
{
    /// <summary>
    /// Base controller class
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the BaseController class
        /// </summary>
        /// <param name="logger">The logger</param>
        public BaseController(ILogger logger)
        {
            this.logger = logger;

            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On("open-blank", async (args) =>
                {
                    await Electron.Shell.OpenExternalAsync(args.ToString());
                });
            }

        }
    }
}
