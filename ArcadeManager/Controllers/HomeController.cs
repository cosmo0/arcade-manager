using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ArcadeManager.Models;

namespace ArcadeManager.Controllers
{
    /// <summary>
    /// Controller for the home pages
    /// </summary>
    public class HomeController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the HomeController class
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets the index view (main app view)
        /// </summary>
        /// <returns>The view</returns>
        public IActionResult Index() => View();

        /// <summary>
        /// Gets the OS selection view
        /// </summary>
        /// <returns>The view</returns>
        public IActionResult Os() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
