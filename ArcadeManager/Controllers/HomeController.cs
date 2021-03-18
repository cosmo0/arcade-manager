using ArcadeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ArcadeManager.Controllers {

	/// <summary>
	/// Controller for the home pages
	/// </summary>
	public class HomeController : BaseController {

		/// <summary>
		/// Initializes a new instance of the HomeController class
		/// </summary>
		/// <param name="logger"></param>
		public HomeController(ILogger<HomeController> logger) : base(logger) {
		}

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
		public IActionResult Index() => View();

		/// <summary>
		/// OS selection view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Os() => View();
	}
}
