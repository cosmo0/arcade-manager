using System;
using System.IO;
using System.Text.Json;

namespace ArcadeManager.Behavior
{
    /// <summary>
    /// The application settings
    /// </summary>
    public class Settings
    {
        private readonly static SettingsManager mgr;
        private readonly static Settings settings;

        static Settings()
        {
            mgr = new SettingsManager("ArcadeManager.json");

            settings = mgr.LoadSettings() ?? new Settings();
        }

        public Settings()
        {
        }

        public string OsInstance { get; set; }

        /// <summary>
        /// Gets or sets the OS
        /// </summary>
        public static string Os
        {
            get
            {
                return settings.OsInstance;
            }
            set
            {
                settings.OsInstance = value;
                SaveSettings();
            }
        }

        private static void SaveSettings()
        {
            mgr.SaveSettings(settings);
        }
    }

    /// <summary>
    /// Settings manager
    /// </summary>
    internal class SettingsManager
    {
        private readonly string _filePath;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Console.WriteLine($"Settings file: {appData}");
            return Path.Combine(appData, fileName);
        }

        public Settings LoadSettings() =>
            File.Exists(_filePath) ?
            JsonSerializer.Deserialize<Settings>(File.ReadAllText(_filePath)) :
            null;

        public void SaveSettings(Settings settings)
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}
