using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers; 

/// <summary>
/// Controller for the overlays page
/// </summary>
public class OverlaysController : BaseController {

	/// <summary>
	/// Initializes a new instance of the <see cref="OverlaysController" /> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	public OverlaysController(ILogger<OverlaysController> logger) : base(logger) {
	}

	/// <summary>
	/// Index view
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Index() => View();
}