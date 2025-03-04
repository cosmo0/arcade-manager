using ArcadeManager.Infrastructure;
using ArcadeManager.Models;
using ElectronNET.API;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ArcadeManager;

/// <summary>
/// Provides environment values relative to ArcadeManager
/// </summary>
public class ArcadeManagerEnvironment : IEnvironment {
    private static readonly SettingsManager mgr = new(@"ArcadeManager\userSettings.json");
    private static readonly Settings settings = mgr.LoadSettings() ?? new Settings();
    private static AppData _appData;
    private static string _basePath;
    private static string _platform;

    /// <summary>
    /// Gets the current AppData values
    /// </summary>
    public static AppData AppData {
        get {
            if (_appData != null) { return _appData; }

            string content = File.ReadAllText(Path.Join(BasePath, "Data", "appdata.json"));
            _appData = Serializer.Deserialize<AppData>(content);

            return _appData;
        }
    }

    /// <summary>
    /// Gets the application data
    /// </summary>
    /// <returns>The application data</returns>
    public AppData GetAppData() {
        return AppData;
    }

    /// <summary>
    /// Gets the base application path
    /// </summary>
    public static string BasePath {
        get {
            if (!string.IsNullOrEmpty(_basePath)) { return _basePath; }

            // See stackoverflow.com/a/58307732/6776
            using var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            _basePath = System.IO.Path.GetDirectoryName(processModule?.FileName);

            return _basePath;
        }
    }

    /// <summary>
    /// Gets the application base path
    /// </summary>
    /// <returns></returns>
    public string GetBasePath() {
        return BasePath;
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
            else {
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
    /// Gets the OS from the settings
    /// </summary>
    /// <returns>The OS</returns>
    public string GetSettingsOs() {
        return SettingsOs;
    }

    /// <summary>
    /// Gets the app version.
    /// </summary>
    /// <returns>The app version</returns>
    public static async Task<string> GetVersion() {
        return await Electron.App.GetVersionAsync();
    }

    /// <summary>
    /// Adds the specified version to the list of ignored versions
    /// </summary>
    /// <param name="version">The version.</param>
    public void SettingsIgnoredVersionAdd(string version) {
        if (string.IsNullOrWhiteSpace(version)) { return; }

        settings.IgnoredVersions.Add(version);

        mgr.SaveSettings(settings);
    }

    /// <summary>
    /// Gets a value indicating whether the specified version should be ignored
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>Whether the version should be ignored</returns>
    public bool SettingsIgnoredVersionHas(string version) {
        return settings.IgnoredVersions.Contains(version);
    }
}
