using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the overlays page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OverlaysController" /> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class OverlaysController(ILogger<OverlaysController> logger) : BaseController(logger) {

    /// <summary>
    /// Index view
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Index() => View();
}