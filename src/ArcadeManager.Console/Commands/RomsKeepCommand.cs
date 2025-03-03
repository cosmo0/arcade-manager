using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsKeepCommand : Command<RomsSettings>
{
    public override int Execute(CommandContext context, RomsSettings settings)
    {
        System.Console.WriteLine("RomsKeepCommand");
        return 0;
    }
}
