using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers {

	/// <summary>
	/// Controller for the help pages
	/// </summary>
	public class HelpController : BaseController {

		/// <summary>
		/// Initializes a new instance of the <see cref="HelpController" /> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public HelpController(ILogger<HelpController> logger) : base(logger) {
		}

		/// <summary>
		/// Basics view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Basics() => View();

		/// <summary>
		/// Custom CSV view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult CustomCsv() => View();

		/// <summary>
		/// DAT files view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult DatFiles() => View();

		/// <summary>
		/// Emulators view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Emulators() => View();

		/// <summary>
		/// Index view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Index() => View();

		/// <summary>
		/// Install and configure view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Install() => View();

		/// <summary>
		/// Known systems view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult KnownSystems() => View();

		/// <summary>
		/// Romsets view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Romsets() => View();

		/// <summary>
		/// Shares view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Shares() => View();

		/// <summary>
		/// Tips view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult Tips() => View();

		/// <summary>
		/// What view
		/// </summary>
		/// <returns>The view</returns>
		public IActionResult What() => View();
	}
}
