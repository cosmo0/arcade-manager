using System;
using ArcadeManager.Core.Models;

namespace ArcadeManager.Core;

/// <summary>
/// Interface for environment data injection
/// </summary>
public interface IEnvironment
{
    /// <summary>
    /// Gets the application data
    /// </summary>
    /// <returns>The application data</returns>
    AppData GetAppData();
    
    /// <summary>
    /// Gets the application base path
    /// </summary>
    /// <returns></returns>
    string GetBasePath();

    /// <summary>
    /// Gets the OS from the settings
    /// </summary>
    /// <returns>The OS</returns>
    string GetSettingsOs();

    /// <summary>
    /// Adds the specified version to the list of ignored versions
    /// </summary>
    /// <param name="version">The version.</param>
    void SettingsIgnoredVersionAdd(string version);

    /// <summary>
    /// Gets a value indicating whether the specified version should be ignored
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>Whether the version should be ignored</returns>
    bool SettingsIgnoredVersionHas(string version);
}
