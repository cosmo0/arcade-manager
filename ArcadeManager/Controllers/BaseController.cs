using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArcadeManager.Controllers; 

/// <summary>
/// Base controller class
/// </summary>
public abstract class BaseController : Controller {

	/// <summary>
	/// The logger
	/// </summary>
	protected readonly ILogger logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="BaseController"/> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	protected BaseController(ILogger logger) {
		this.logger = logger;
	}
}