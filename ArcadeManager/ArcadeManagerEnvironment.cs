using ArcadeManager.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ArcadeManager {

	/// <summary>
	/// Provides environment values relative to ArcadeManager
	/// </summary>
	public static class ArcadeManagerEnvironment {
		private readonly static Settings settings;
		private readonly static SettingsManager mgr;

		private static AppData _appData;
		private static string _basePath;
		private static string _platform;

		/// <summary>
        /// Initializes the ArcadeManager environment
        /// </summary>
		static ArcadeManagerEnvironment()
		{
			mgr = new SettingsManager(@"ArcadeManager\userSettings.json");
			settings = mgr.LoadSettings() ?? new Settings();
		}

		/// <summary>
		/// Gets the current AppData values
		/// </summary>
		public static AppData AppData {
			get {
				if (_appData != null) { return _appData; }

				string content = File.ReadAllText(Path.Join(BasePath, Path.Join("Data", "appdata.json")));
				_appData = Services.Serializer.Deserialize<AppData>(content);

				return _appData;
			}
		}

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
				else
                {
					throw new NotImplementedException("If you want to run Arcade Manager on something else than Linux, Mac or Windows, you'll have some coding to do!");
                }

				return _platform;
			}
		}

		/// <summary>
		/// Gets or sets the settings OS.
		/// </summary>
		public static string SettingsOs {
			get {
				return settings.Os ?? string.Empty;
			}
			set {
				settings.Os = value;

				mgr.SaveSettings(settings);
			}
		}

		/// <summary>
		/// Settings manager
		/// </summary>
		internal class SettingsManager {
			private readonly string _filePath;

			/// <summary>
			/// Initializes a new instance of the <see cref="SettingsManager"/> class.
			/// </summary>
			/// <param name="fileName">Name of the file.</param>
			public SettingsManager(string fileName) {
				_filePath = GetLocalFilePath(fileName);
			}

			/// <summary>
			/// Loads the settings.
			/// </summary>
			/// <returns>The settings</returns>
			public Settings LoadSettings() =>
				File.Exists(_filePath) ?
				Services.Serializer.Deserialize<Settings>(File.ReadAllText(_filePath)) :
				null;

			/// <summary>
			/// Saves the settings.
			/// </summary>
			/// <param name="settings">The settings.</param>
			public void SaveSettings(Settings settings) {
				string json = Services.Serializer.Serialize(settings);
				File.WriteAllText(_filePath, json);
			}

			/// <summary>
			/// Gets the local file path.
			/// </summary>
			/// <param name="fileName">Name of the file.</param>
			/// <returns>The local file path</returns>
			private static string GetLocalFilePath(string fileName) {
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				return Path.Combine(appData, fileName);
			}
		}
	}
}
