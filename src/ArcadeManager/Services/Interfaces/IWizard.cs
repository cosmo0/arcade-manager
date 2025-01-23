using System.Collections.Generic;

namespace ArcadeManager.Services;

/// <summary>
/// Interface for the wizard service
/// </summary>
public interface IWizard {

    /// <summary>
    /// Counts the games in the lists.
    /// </summary>
    /// <param name="emulator">The emulator.</param>
    /// <returns>The games in each list</returns>
    Dictionary<string, int> CountGamesInLists(string emulator);
}