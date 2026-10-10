using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class CheckDatCommand(IDatChecker checker, IMessageHandler messageHandler) : AsyncCommand<RomsCheckDatSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RomsCheckDatSettings settings, CancellationToken cancellationToken)
    {
        await checker.CheckDat(settings.ToAction(), messageHandler);
        return 0;
    }
}
