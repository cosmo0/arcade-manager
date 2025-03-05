using System;

namespace ArcadeManager.Core.Models.Roms;

public interface IGameRomFile
{
    string Path { get; }
    
    string Name { get; }
}
