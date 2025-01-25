using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers;

/// <summary>
/// Base controller class
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseController"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public abstract class BaseController(ILogger logger) : Controller {

	/// <summary>
	/// The logger
	/// </summary>
	protected readonly ILogger logger = logger;
}