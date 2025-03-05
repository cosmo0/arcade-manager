namespace ArcadeManager.Models.Roms;

/// <summary>
/// A reason of an error on a game or file
/// </summary>
public enum ErrorReason
{
    None,
    MissingFile,
    BadHash
}
