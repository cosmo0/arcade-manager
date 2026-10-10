using System;
using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class OverlaysCheckCommand(IOverlays overlays, IMessageHandler messageHandler) : AsyncCommand<OverlaysCheckSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OverlaysCheckSettings settings, CancellationToken cancellationToken)
    {
        await overlays.Check(settings.ToAction(), messageHandler);
        return 0;
    }
}
