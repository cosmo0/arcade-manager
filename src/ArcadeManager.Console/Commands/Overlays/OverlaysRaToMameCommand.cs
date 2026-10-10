using System;
using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class OverlaysRaToMameCommand(IOverlays overlays, IMessageHandler messageHandler) : AsyncCommand<OverlaysRaToMameSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OverlaysRaToMameSettings settings, CancellationToken cancellationToken)
    {
        await overlays.ConvertRaToMame(settings.ToAction(), messageHandler);
        return 0;
    }
}