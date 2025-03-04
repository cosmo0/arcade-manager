using ArcadeManager.Console.Settings;
using ArcadeManager.Services;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvConvertDatCommand(ICsv csv, IMessageHandler messageHandler) : AsyncCommand<CsvSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CsvSettings settings)
    {
        await csv.ConvertDat(settings.Main, settings.Target, messageHandler);
        return 0;
    }
}
