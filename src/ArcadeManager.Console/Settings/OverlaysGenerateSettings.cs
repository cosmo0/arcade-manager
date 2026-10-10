using ArcadeManager.Core.Models.Actions;
using ArcadeManager.Core.Models.Actions.Overlays;
using ArcadeManager.Core.Models.Bezels;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace ArcadeManager.Console.Settings;

public class OverlaysGenerateSettings : BaseOverlaysSettings
{
    [Description("The images folder from which to create overlays from")]
    [CommandOption("-i|--images <FOLDER>", true)]
    public string ImagesFolder { get; set; }

    [Description("Whether to overwrite existing files")]
    [CommandOption("-o|--overwrite")]
    [DefaultValue(false)]
    public bool Overwrite { get; set; } = false;

    [Description("To roms folder in which to output the overlays configs")]
    [CommandOption("-r|--roms <FOLDER>", true)]
    public string RomsFolder { get; set; }

    public GenerateAction ToAction()
    {
        return new GenerateAction()
        {
            ImagesFolder = ImagesFolder,
            Margin = Margin,
            OutputDebug = OutputDebug,
            Overwrite = Overwrite,
            RomsFolder = RomsFolder,
            TargetResolution = TargetResolution
        };
    }
}