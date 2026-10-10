using ArcadeManager.Core.Models.Actions.Overlays;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ArcadeManager.Console.Settings;

public class OverlaysRaToMameSettings : BaseOverlaysSettings
{
    [Description("The path to the output bezels")]
    [CommandOption("--output <FOLDER>", true)]
    public string Output { get; set; }

    [Description("Whether to overwrite existing files")]
    [CommandOption("--overwrite")]
    [DefaultValue(false)]
    public bool Overwrite { get; set; }

    [Description("Whether to scan the image for screen coordinates rather than converting configuration values (slower but more accurate)")]
    [CommandOption("--scan")]
    [DefaultValue(false)]
    public bool ScanBezelForScreenCoordinates { get; set; }

    [Description("The path to the source configurations")]
    [CommandOption("--configs <FOLDER>", true)]
    public string SourceConfigs { get; set; }

    [Description("The path to the source roms")]
    [CommandOption("--roms <FOLDER>", true)]
    public string SourceRoms { get; set; }

    [Description("Whether to zip the output bezels")]
    [CommandOption("--zip")]
    [DefaultValue(false)]
    public bool Zip { get; set; }

    public RaToMameAction ToAction()
    {
        return new RaToMameAction()
        {
            Output = Output,
            ScanBezelForScreenCoordinates = ScanBezelForScreenCoordinates,
            SourceConfigs = SourceConfigs,
            SourceRoms = SourceRoms,
            Zip = Zip,
            Margin = Margin,
            OutputDebug = OutputDebug,
            Overwrite = Overwrite,
            TargetResolution = TargetResolution
        };
    }
}