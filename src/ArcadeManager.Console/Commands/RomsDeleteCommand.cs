using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsDeleteCommand : Command<RomsSettings>
{
    public override int Execute(CommandContext context, RomsSettings settings)
    {
        System.Console.WriteLine("RomsDeleteCommand");
        return 0;
    }
}
