using ArcadeManager.Actions;
using ArcadeManager.Console.Settings;
using ArcadeManager.Services;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsCheckDatCommand(IDatChecker checker, IMessageHandler messageHandler) : AsyncCommand<RomsCheckDatSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RomsCheckDatSettings settings)
    {
        await checker.CheckDat(settings.ToAction(), messageHandler);
        return 0;
    }
}
