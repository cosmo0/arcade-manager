using ArcadeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the wizard pages
/// </summary>
public class WizardController : BaseController {

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WizardController(ILogger<WizardController> logger) : base(logger) {
    }

    /// <summary>
    /// Index view: selection of actions
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Index() => View();

    /// <summary>
    /// Postback to index - 
    /// </summary>
    /// <param name="roms">The roms.</param>
    /// <param name="overlays">The overlays.</param>
    /// <returns>Redirects to an action</returns>
    [HttpPost]
    public IActionResult Index(string roms, string overlays) {
        var model = new Wizard {
            DoRoms = !string.IsNullOrEmpty(roms),
            DoOverlays = !string.IsNullOrEmpty(overlays)
        };

        if (model.DoRoms) {
            return RedirectToAction("Roms", model);
        } else {
            return RedirectToAction("Overlays", model);
        }
    }

    public IActionResult Roms(Wizard model) => View(model);
}