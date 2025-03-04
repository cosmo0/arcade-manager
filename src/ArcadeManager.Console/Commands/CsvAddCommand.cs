using System;
using ArcadeManager.Console.Settings;
using ArcadeManager.Services;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CsvAddCommand(ICsv csv, IMessageHandler messageHandler) : AsyncCommand<CsvSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CsvSettings settings)
    {
        await csv.Merge(settings.Main, settings.Secondary, settings.Target, messageHandler);
        return 0;
    }
}
