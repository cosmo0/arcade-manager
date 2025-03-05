using System;
using ArcadeManager.Models;
using ArcadeManager.Models.Roms;

namespace ArcadeManager.Console;

public class ConsoleMessageHandler : IMessageHandler
{
    public bool MustCancel { get; set; }
    public int TotalItems { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentItem { get; set; }
    public int CurrentStep { get; set; }

    public void Done(string label, string folder)
    {
        System.Console.WriteLine(label);

        if (!string.IsNullOrEmpty(folder)) {
            System.Console.WriteLine($"Target folder: {folder}");
        }
    }

    public void Error(Exception ex)
    {
        System.Console.WriteLine(ex.Message);
        System.Console.WriteLine(ex.StackTrace);
    }

    public void Init(string label)
    {
        System.Console.WriteLine(label);
    }

    public void Processed(GameRom game)
    {
        System.Console.WriteLine($"Processed: {game.Name}");
    }

    public void Progress(string label)
    {
        System.Console.WriteLine(label);
    }

    public void Progress(string label, int total, int current)
    {
        System.Console.WriteLine(label);
    }
}
