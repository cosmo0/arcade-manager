using ArcadeManager.Core;
using ArcadeManager.Core.Models.Roms;

namespace ArcadeManager.Console;

public class ConsoleMessageHandler : IMessageHandler
{
    public int CurrentItem { get; set; }
    public int CurrentStep { get; set; }
    public bool MustCancel { get; set; }
    public int TotalItems { get; set; }
    public int TotalSteps { get; set; }

    public void Progress(string label)
    {
        System.Console.WriteLine(label);
    }

    public void Progress(string label, int total, int current)
    {
        System.Console.WriteLine($"{current}/{total} - {label}");
    }

    public void ProgressDone(string label, string folder)
    {
        System.Console.WriteLine(label);

        if (!string.IsNullOrEmpty(folder))
        {
            System.Console.WriteLine($"Target folder: {folder}");
        }
    }

    public void ProgressError(Exception ex)
    {
        System.Console.WriteLine(ex.Message);
        System.Console.WriteLine(ex.StackTrace);
    }

    public void ProgressInit(string label)
    {
        System.Console.WriteLine($"=== Initializing - {label}");
    }

    public void ProgressMessage(string message)
    {
        System.Console.WriteLine(message);
    }

    public void ProgressProcessed(GameRom game)
    {
        System.Console.WriteLine($"Processed: {game.Name}");
    }
}