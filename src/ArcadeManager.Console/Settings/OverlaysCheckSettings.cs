using ArcadeManager.Core.Models.Actions.Overlays;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArcadeManager.Console.Settings;

public class OverlaysCheckSettings : BaseOverlaysSettings
{
    [Description("Whether to automatically fix configsfiles")]
    [CommandOption("-f|--autofix")]
    [DefaultValue(false)]
    public bool AutoFix { get; set; }

    [Description("An error margin to apply during the check of the screen position (0 is strict; default 10)")]
    [CommandOption("--error-margin")]
    [DefaultValue(10)]
    public int ErrorMargin { get; set; } = 10;

    [Description("The expected overlay path in the rom config file (leave empty if you don't know)")]
    [CommandOption("--expected-path <EXPECTED>")]
    public string? InputOverlayConfigPathInRomConfig { get; set; }

    [Description("The overlays configurations folder")]
    [CommandOption("--overlays <FOLDER>", true)]
    public string? OverlaysConfigFolder { get; set; }

    [Description("The roms configuration folder")]
    [CommandOption("--roms <FOLDER>", true)]
    public string? RomsConfigFolder { get; set; }

    public CheckAction ToAction()
    {
        return new CheckAction()
        {
            AutoFix = AutoFix,
            ErrorMargin = ErrorMargin,
            InputOverlayConfigPathInRomConfig = InputOverlayConfigPathInRomConfig,
            Margin = Margin,
            OutputDebug = OutputDebug,
            OverlaysConfigFolder = OverlaysConfigFolder,
            RomsConfigFolder = RomsConfigFolder,
            TargetResolution = TargetResolution
        };
    }
}