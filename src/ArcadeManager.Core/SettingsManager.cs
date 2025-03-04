using ArcadeManager.Infrastructure;
using ArcadeManager.Models;

namespace ArcadeManager;

/// <summary>
/// Settings manager
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SettingsManager"/> class.
/// </remarks>
/// <param name="fileName">Name of the file.</param>
public class SettingsManager(string fileName)
{
    private readonly string _filePath = GetLocalFilePath(fileName);

    /// <summary>
    /// Loads the settings.
    /// </summary>
    /// <returns>The settings</returns>
    public Settings LoadSettings() =>
        File.Exists(_filePath) ?
        Serializer.Deserialize<Settings>(File.ReadAllText(_filePath)) :
        null;

    /// <summary>
    /// Saves the settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public void SaveSettings(Settings settings)
    {
        string json = Serializer.Serialize(settings);

        var fi = new FileInfo(_filePath);
        if (!Directory.Exists(fi.DirectoryName))
        {
            Directory.CreateDirectory(fi.DirectoryName);
        }

        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Gets the local file path.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>The local file path</returns>
    private static string GetLocalFilePath(string fileName)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, fileName);
    }
}