using System;
using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvAddCommand : Command<CsvSettings>
{
    public override int Execute(CommandContext context, CsvSettings settings)
    {
        System.Console.WriteLine("CsvAddCommand");
        return 0;
    }
}
