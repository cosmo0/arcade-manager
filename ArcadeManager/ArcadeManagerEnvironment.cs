using System.Runtime.InteropServices;

namespace ArcadeManager {

	/// <summary>
	/// Provides environment values relative to ArcadeManager
	/// </summary>
	public static class ArcadeManagerEnvironment {
		private static string _basePath;
		private static string _platform;

		/// <summary>
		/// Gets the base application path
		/// </summary>
		public static string BasePath {
			get {
				if (!string.IsNullOrEmpty(_basePath)) { return _basePath; }

				//_basePath = AppContext.BaseDirectory;

				// See stackoverflow.com/a/58307732/6776
				using var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
				_basePath = System.IO.Path.GetDirectoryName(processModule?.FileName);

				return _basePath;
			}
		}

		/// <summary>
		/// Gets the application platform (win32, darwin, linux)
		/// </summary>
		public static string Platform {
			get {
				if (!string.IsNullOrEmpty(_platform)) { return _platform; }

				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
					_platform = "darwin";
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
					_platform = "win32";
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
					_platform = "linux";
				}

				return _platform;
			}
		}
	}
}
