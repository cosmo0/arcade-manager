using ArcadeManager.Core.Models.Actions;
using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Models.Bezels;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace ArcadeManager.Console.Settings;

public class OverlaysMameToRaSettings : BaseOverlaysSettings
{
    [Description("The output folder for the overlays configs")]
    [CommandOption("--overlays <FOLDER>", true)]
    public string? OutputOverlays { get; set; }

    [Description("The output folder for the roms configs")]
    [CommandOption("--roms <FOLDER>", true)]
    public string? OutputRoms { get; set; }

    [Description("Whether to overwrite existing files")]
    [CommandOption("--overwrite")]
    [DefaultValue(false)]
    public bool Overwrite { get; set; }

    [Description("Whether to scan the image for screen coordinates rather than converting configuration values (slower but more accurate)")]
    [CommandOption("--scan")]
    [DefaultValue(false)]
    public bool ScanBezelForScreenCoordinates { get; set; }

    [Description("The source folder of the MAME bezels (ex: c:\\mame\\artwork")]
    [CommandOption("--source <FOLDER>", true)]
    public string? Source { get; set; }

    [Description("The source folder of the MAME configs (ex: c:\\mame\\cfg ; you can leave empty but it may result in misplaced screens)")]
    [CommandOption("--source-cfg <FOLDER>")]
    public string? SourceConfigs { get; set; }

    public MameToRaAction ToAction()
    {
        return new MameToRaAction()
        {
            OutputOverlays = OutputOverlays,
            OutputRoms = OutputRoms,
            ScanBezelForScreenCoordinates = ScanBezelForScreenCoordinates,
            Source = Source,
            SourceConfigs = SourceConfigs,
            Margin = Margin,
            OutputDebug = OutputDebug,
            Overwrite = Overwrite,
            TargetResolution = TargetResolution
        };
    }
}