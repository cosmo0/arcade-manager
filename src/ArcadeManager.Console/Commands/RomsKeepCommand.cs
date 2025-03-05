using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsKeepCommand(IRoms roms, IMessageHandler messageHandler) : AsyncCommand<RomsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RomsSettings settings)
    {
        await roms.Keep(settings.ToAction(), messageHandler);
        return 0;
    }
}
