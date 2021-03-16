using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers
{
    /// <summary>
    /// Controller for the help pages
    /// </summary>
    public class HelpController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the Help controller
        /// </summary>
        /// <param name="logger">The logger</param>
        public HelpController(ILogger<HelpController> logger) : base(logger)
        {
        }

        public IActionResult Index() => View();

        public IActionResult Basics() => View();

        public IActionResult CustomCsv() => View();

        public IActionResult DatFiles() => View();

        public IActionResult Emulators() => View();

        public IActionResult Install() => View();

        public IActionResult KnownSystems() => View();

        public IActionResult Romsets() => View();

        public IActionResult Shares() => View();

        public IActionResult Tips() => View();

        public IActionResult What() => View();
    }
}
