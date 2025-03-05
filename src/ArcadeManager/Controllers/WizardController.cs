using ArcadeManager.Core.Services.Interfaces;
using ArcadeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the wizard pages
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WizardController"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class WizardController(ILogger<WizardController> logger, IWizard wizardService) : BaseController(logger) {

    /// <summary>
    /// Emulator view: choose the emulator
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>THe view</returns>
    public IActionResult Emulator(WizardViewModel model) => ModelState.IsValid ? View(model) : null;

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
    public IActionResult ListSelection(WizardViewModel model) {
        if (!ModelState.IsValid) { return null; }

        // get number of games in each csv file
        model.GameNumbers = wizardService.CountGamesInLists(model.Emulator);

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
    public IActionResult Paths(WizardViewModel model) => ModelState.IsValid ? View(model) : null;

    /// <summary>
    /// Postback for the emulator
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>Redirects to the next page</returns>
    [HttpPost]
    public IActionResult PostEmulator(WizardViewModel model) {
        if (!ModelState.IsValid) { return null; }

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
        var model = new WizardViewModel {
            DoRoms = !string.IsNullOrEmpty(roms),
            DoOverlays = !string.IsNullOrEmpty(overlays)
        };

        if (model.DoRoms) {
            return RedirectToAction("Emulator", model);
        }
        else {
            return RedirectToAction("Index", "Overlays", model);
        }
    }

    /// <summary>
    /// Postback for the list selection view
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>Redirects to the next page</returns>
    [HttpPost]
    public IActionResult PostListSelection(string[] list, WizardViewModel model) {
        if (!ModelState.IsValid) { return null; }
        
        model.Lists = list;
        return RedirectToAction("Paths", model);
    }
}