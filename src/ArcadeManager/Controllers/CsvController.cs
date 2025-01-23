using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers; 

/// <summary>
/// Controller for the help pages
/// </summary>
public class CsvController : BaseController {

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvController" /> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	public CsvController(ILogger<CsvController> logger) : base(logger) {
	}

	/// <summary>
	/// Add files to a list
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Add() => View();

	/// <summary>
	/// Convert a DAT file
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult ConvertDat() => View();

	/// <summary>
	/// Convert a INI file
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult ConvertIni() => View();

	/// <summary>
	/// Delete entries
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Delete() => View();

	/// <summary>
	/// Download a pre-built list
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult Download() => View();

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

	/// <summary>
	/// List files
	/// </summary>
	/// <returns>The view</returns>
	public IActionResult ListFiles() => View();
}