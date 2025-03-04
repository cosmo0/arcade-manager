using ArcadeManager.Console.Settings;
using ArcadeManager.Services;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class RomsDeleteCommand(IRoms roms, IMessageHandler messageHandler) : AsyncCommand<RomsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RomsSettings settings)
    {
        await roms.Delete(settings.ToAction(), messageHandler);
        return 0;
    }
}
