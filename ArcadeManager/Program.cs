using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ArcadeManager {

	/// <summary>
	/// Base program entry point
	/// </summary>
	public static class Program {

		/// <summary>
		/// Creates the host builder.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		/// <returns>The host builder</returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseElectron(args);
					webBuilder.UseStartup<Startup>();
				});

		/// <summary>
		/// Defines the entry point of the application.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args) {
			CreateHostBuilder(args).Build().Run();
		}
	}
}