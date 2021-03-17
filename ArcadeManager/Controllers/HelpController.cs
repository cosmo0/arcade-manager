using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers {

	/// <summary>
	/// Controller for the roms pages
	/// </summary>
	public class RomsController : BaseController {

		/// <summary>
		/// Initializes a new instance of the <see cref="RomsController" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public RomsController(ILogger<RomsController> logger) : base(logger) {
		}

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
		/// Keep only listed files
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Keep() => View();

		/// <summary>
		/// Index view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Index() => View();
	}
}
