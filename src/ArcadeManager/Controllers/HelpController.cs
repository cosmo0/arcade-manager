using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the help pages
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HelpController"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class HelpController(ILogger<HelpController> logger) : BaseController(logger)
{
    public IActionResult Basics() => View();

    public IActionResult CustomCsv() => View();

    public IActionResult DatFiles() => View();

    public IActionResult Emulators() => View();

    public IActionResult Index() => View();

    public IActionResult Install() => View();

    public IActionResult KnownSystems() => View();

    public IActionResult OverlaysGeneral() => View();

    public IActionResult OverlaysMame() => View();

    public IActionResult OverlaysRa() => View();

    public IActionResult Romsets() => View();

    public IActionResult Shares() => View();

    public IActionResult Tips() => View();

    public IActionResult What() => View();
}