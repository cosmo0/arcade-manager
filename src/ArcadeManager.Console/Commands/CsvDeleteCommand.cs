using ArcadeManager.Console.Settings;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvDeleteCommand : Command<CsvSettings>
{
    public override int Execute(CommandContext context, CsvSettings settings)
    {
        System.Console.WriteLine("CsvDeleteCommand");
        return 0;
    }
}
