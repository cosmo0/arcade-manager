using ArcadeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

    public IActionResult ListSelection(Wizard model) => View(model);

    /// <summary>
    /// Postback to index -
    /// </summary>
    /// <param name="roms">The roms.</param>
    /// <param name="overlays">The overlays.</param>
    /// <returns>Redirects to an action</returns>
    [HttpPost]
    public IActionResult PostIndex(string roms, string overlays) {
        var model = new Wizard {
            DoRoms = !string.IsNullOrEmpty(roms),
            DoOverlays = !string.IsNullOrEmpty(overlays)
        };

        if (model.DoRoms) {
            return RedirectToAction("Roms", model);
        }
        else {
            return RedirectToAction("Overlays", model);
        }
    }

    [HttpPost]
    public IActionResult PostListSelection(Wizard model) {
        return RedirectToAction("Paths", model);
    }

    [HttpPost]
    public IActionResult PostRoms(Wizard model) {
        return RedirectToAction("System", model);
    }

    [HttpPost]
    public IActionResult PostSystem(Wizard model) {
        return RedirectToAction("ListSelection", model);
    }

    public IActionResult Roms(Wizard model) => View(model);

    public IActionResult System(Wizard model) => View(model);
}