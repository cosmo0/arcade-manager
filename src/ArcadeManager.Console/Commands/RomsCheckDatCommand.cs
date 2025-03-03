using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsCheckDatCommand : Command<RomsCheckDatSettings>
{
    public override int Execute(CommandContext context, RomsCheckDatSettings settings)
    {
        System.Console.WriteLine("RomsCheckDatCommand");
        return 0;
    }
}
