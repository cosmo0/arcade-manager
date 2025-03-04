using ArcadeManager.Console.Settings;
using ArcadeManager.Services;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvDeleteCommand(ICsv csv, IMessageHandler messageHandler) : AsyncCommand<CsvSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CsvSettings settings)
    {
        await csv.Remove(settings.Main, settings.Secondary, settings.Target, messageHandler);
        return 0;
    }
}
