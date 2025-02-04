using System;
using ArcadeManager.Actions;

namespace ArcadeManager.Services;

/// <summary>
/// Interface for DAT file checker
/// </summary>
public interface IDatChecker
{
    /// <summary>
    /// Checks a romset against a DAT file
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="messageHandler">The message handler</param>
    Task CheckDat(RomsActionCheckDat args, IMessageHandler messageHandler);
}
