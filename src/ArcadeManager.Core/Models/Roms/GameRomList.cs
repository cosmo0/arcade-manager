namespace ArcadeManager.Models.Roms;

/// <summary>
/// A list of game roms (zip)
/// </summary>
public class GameRomList : List<GameRom>
{
    /// <summary>
    /// Gets a game by its name
    /// </summary>
    /// <param name="name">The game name</param>
    /// <returns>The game, if found</returns>
    public GameRom this[string name]
    {
        get
        {
            return this.FirstOrDefault(g => g.Name == name);
        }
    }
}
