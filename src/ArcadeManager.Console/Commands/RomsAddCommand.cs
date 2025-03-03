using System;
using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsAddCommand : Command<RomsSettings>
{
    public override int Execute(CommandContext context, RomsSettings settings)
    {
        System.Console.WriteLine("RomsAddCommand");
        return 0;
    }
}
