using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvKeepCommand : Command<CsvSettings>
{
    public override int Execute(CommandContext context, CsvSettings settings)
    {
        System.Console.WriteLine("CsvKeepCommand");
        return 0;
    }
}
