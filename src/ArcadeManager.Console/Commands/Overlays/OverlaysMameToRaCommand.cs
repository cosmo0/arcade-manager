using System;
using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class OverlaysMameToRaCommand(IOverlays overlays, IMessageHandler messageHandler) : AsyncCommand<OverlaysMameToRaSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OverlaysMameToRaSettings settings, CancellationToken cancellationToken)
    {
        await overlays.ConvertMameToRa(settings.ToAction(), messageHandler);
        return 0;
    }
}
