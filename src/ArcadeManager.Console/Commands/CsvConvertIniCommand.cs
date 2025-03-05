using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvConvertIniCommand(ICsv csv, IMessageHandler messageHandler) : AsyncCommand<CsvSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CsvSettings settings)
    {
        await csv.ConvertIni(settings.Main, settings.Target, messageHandler);
        return 0;
    }
}
