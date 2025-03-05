using ArcadeManager.Core.Services.Interfaces;
using ArcadeManager.Models;
using ElectronNET.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the home pages
/// </summary>
/// <remarks>
/// Initializes a new instance of the HomeController class
/// </remarks>
/// <param name="logger"></param>
public class HomeController(ILogger<HomeController> logger, ILocalizer localizer) : BaseController(logger) {

    /// <summary>
    /// Displays the error
    /// </summary>
    /// <returns>The error view</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Index view (default app view)
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Index(string lang) {
        localizer.ChangeCulture(lang);

        return View();
    }

    /// <summary>
    /// OS selection view
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Os() => View();
}