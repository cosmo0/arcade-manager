using System;

namespace ArcadeManager.Models.Roms;

public interface IGameRomFile
{
    string Path { get; }
    
    string Name { get; }
}
