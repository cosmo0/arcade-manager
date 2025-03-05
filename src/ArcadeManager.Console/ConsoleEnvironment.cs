using ArcadeManager.Core;
using ArcadeManager.Core.Infrastructure;
using ArcadeManager.Core.Models;

namespace ArcadeManager.Console;

public class ConsoleEnvironment : IEnvironment
{
    private AppData? _appData;
    private string? _basePath;

    public AppData GetAppData()
    {
        if (_appData != null) { return _appData; }

        string content = File.ReadAllText(Path.Join(GetBasePath(), "Data", "appdata.json"));
        _appData = Serializer.Deserialize<AppData>(content);

        return _appData;
    }

    public string GetBasePath()
    {
        if (!string.IsNullOrEmpty(_basePath)) { return _basePath; }

        // See stackoverflow.com/a/58307732/6776
        using var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
        _basePath = System.IO.Path.GetDirectoryName(processModule?.FileName);

        return _basePath!;
    }

    public string GetSettingsOs()
    {
        // unused in console mode
        return string.Empty;
    }

    public void SettingsIgnoredVersionAdd(string version)
    {
        // do nothing as the console version doesn't check for update
    }

    public bool SettingsIgnoredVersionHas(string version)
    {
        // do nothing as the console version doesn't check for update
        return false;
    }
}
