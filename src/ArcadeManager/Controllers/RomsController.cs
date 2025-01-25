using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Controller for the roms pages
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RomsController" /> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class RomsController(ILogger<RomsController> logger) : BaseController(logger) {

    /// <summary>
    /// Copy roms to a folder
    /// </summary>
    /// <returns>The view</returns>
    public IActionResult Add() => View();

	/// <summary>
	/// Delete roms from a folder
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Delete() => View();

	/// <summary>
	/// Index view
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Index() => View();

	/// <summary>
	/// Keep only listed files
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Keep() => View();
}