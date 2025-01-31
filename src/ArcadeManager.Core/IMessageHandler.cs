using System;
using ArcadeManager.Models;

namespace ArcadeManager;

public interface IMessageHandler
{    
    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    bool MustCancel { get; set; }

    /// <summary>
    /// Sends a "done" progress message
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="folder">The result folder, if any.</param>
    void Done(string label, string folder);

    /// <summary>
    /// Sends an "error" progress message
    /// </summary>
    /// <param name="ex">The exception.</param>
    void Error(Exception ex);

    /// <summary>
    /// Sends an "init" progress message
    /// </summary>
    /// <param name="label">The label.</param>
    void Init(string label);

    /// <summary>
    /// Sends a progression message
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="current">The current item number.</param>
    void Progress(string label, int total, int current);

    /// <summary>
    /// Sets the list of successfully processed games
    /// </summary>
    /// <param name="games">The processed games</param>
    void SetProcessed(IEnumerable<GameRom> games);

    /// <summary>
    /// Sets the list of failed games to process
    /// </summary>
    /// <param name="errors">The errors</param>
    void SetErrors(IEnumerable<GameError> errors);
}
