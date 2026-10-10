using System;
using ArcadeManager.Console.Settings;
using ArcadeManager.Core;
using ArcadeManager.Core.Services.Interfaces;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Commands;

public class OverlaysGenerateCommand(IOverlays overlays, IMessageHandler messageHandler) : AsyncCommand<OverlaysGenerateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OverlaysGenerateSettings settings, CancellationToken cancellationToken)
    {
        await overlays.Generate(settings.ToAction(), messageHandler);
        return 0;
    }
}
