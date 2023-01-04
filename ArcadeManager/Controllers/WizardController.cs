using ArcadeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the wizard pages
/// </summary>
public class WizardController : BaseController {
    private readonly Services.IWizard wizardService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WizardController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WizardController(ILogger<WizardController> logger, Services.IWizard wizardService) : base(logger) {
        this.wizardService = wizardService;
    }

    /// <summary>
    /// Emulator view: choose the emulator
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>THe view</returns>
    public IActionResult Emulator(Wizard model) => View(model);

    /// <summary>
    /// Index view: selection of actions (install roms/install overlays)
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Index() => View();

    /// <summary>
    /// List selection view: selection of games list
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The view</returns>
    public IActionResult ListSelection(Wizard model) {
        // get number of games in each csv file
        model.GameNumbers = this.wizardService.CountGamesInLists(model.Emulator);

        return View(model);
    }

    /// <summary>
    /// Missing FBNeo view
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult MissingFbneo() => View();

    /// <summary>
    /// Path view: selection of the source and target paths
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The view</returns>
    public IActionResult Paths(Wizard model) => View(model);

    /// <summary>
    /// Postback for the emulator
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>Redirects to the next page</returns>
    [HttpPost]
    public IActionResult PostEmulator(Wizard model) {
        return RedirectToAction("ListSelection", model);
    }

    /// <summary>
    /// Postback for index
    /// </summary>
    /// <param name="roms">The roms.</param>
    /// <param name="overlays">The overlays.</param>
    /// <returns>Redirects to the next page</returns>
    [HttpPost]
    public IActionResult PostIndex(string roms, string overlays) {
        var model = new Wizard {
            DoRoms = !string.IsNullOrEmpty(roms),
            DoOverlays = !string.IsNullOrEmpty(overlays)
        };

        if (model.DoRoms) {
            return RedirectToAction("Emulator", model);
        }
        else {
            return RedirectToAction("Overlays", model);
        }
    }

    /// <summary>
    /// Postback for the list selection view
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>Redirects to the next page</returns>
    [HttpPost]
    public IActionResult PostListSelection(string[] list, Wizard model) {
        model.Lists = list;
        return RedirectToAction("Paths", model);
    }
}