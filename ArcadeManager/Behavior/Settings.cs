using System;
using System.IO;
using System.Text.Json;

namespace ArcadeManager.Behavior {

	/// <summary>
	/// The application settings
	/// </summary>
	public class Settings {
		private readonly static SettingsManager mgr;
		private readonly static Settings settings;

		/// <summary>
		/// Initializes the <see cref="Settings"/> class.
		/// </summary>
		static Settings() {
			mgr = new SettingsManager(@"ArcadeManager\userSettings.json");

			settings = mgr.LoadSettings() ?? new Settings();
		}

		/// <summary>
		/// Gets or sets the OS
		/// </summary>
		public static string Os {
			get {
				return settings.OsInstance;
			}
			set {
				settings.OsInstance = value;
				SaveSettings();
			}
		}

		/// <summary>
		/// Gets or sets the os as instance property.
		/// </summary>
		public string OsInstance { get; set; }

		/// <summary>
		/// Saves the settings.
		/// </summary>
		private static void SaveSettings() {
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
			JsonSerializer.Deserialize<Settings>(File.ReadAllText(_filePath)) :
			null;

		/// <summary>
		/// Saves the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void SaveSettings(Settings settings) {
			string json = JsonSerializer.Serialize(settings);
			File.WriteAllText(_filePath, json);
		}

		/// <summary>
		/// Gets the local file path.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>The local file path</returns>
		private string GetLocalFilePath(string fileName) {
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			return Path.Combine(appData, fileName);
		}
	}
}
