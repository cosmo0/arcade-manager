using System;
using ArcadeManager.Core.Models.Roms;

namespace ArcadeManager.Core;

public interface IMessageHandler
{    
    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    bool MustCancel { get; set; }

    /// <summary>
    /// Gets or sets the total number of items to process
    /// </summary>
    int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the total number of steps
    /// </summary>
    int TotalSteps { get; set; }

    /// <summary>
    /// Gets or sets the current item index
    /// </summary>
    int CurrentItem { get; set; }

    /// <summary>
    /// Gets or sets the current step index
    /// </summary>
    int CurrentStep { get; set; }

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
    /// <param name="label">The label to display</param>
    void Progress(string label);

    /// <summary>
    /// Sends a progression message
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="current">The current item number.</param>
    void Progress(string label, int total, int current);

    /// <summary>
    /// Sends a game processed message
    /// </summary>
    /// <param name="game">The processed game</param>
    void Processed(GameRom game);
}
