using ArcadeManager.Core.Models.Bezels;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ArcadeManager.Console.Settings;

public abstract class BaseOverlaysSettings : CommandSettings
{
    private string targetResolution = "1920x1080";

    [Description("A margin to apply to the position of the game screen in the overlay (<0 the screen will be cropped, >0 there will be a black border around)")]
    [CommandOption("-m|--margin")]
    [DefaultValue(0)]
    public int Margin { get; set; } = 0;

    [Description("A debug folder to generate the overlays images with a red square where the screen will be")]
    [CommandOption("--debug <FOLDER>")]
    public string OutputDebug { get; set; }

    [Description("The target resolution for the overlays, under the form '1920x1080' (default is 1080p)")]
    [CommandOption("--resolution <RESOLUTION>")]
    [DefaultValue("1920x1080")]
    public string TargetResolution
    {
        get
        {
            return targetResolution;
        }
        set
        {
            if (string.IsNullOrEmpty(value)) { return; }

            var splitRes = value.Split('x', '*', ':', '/');
            if (splitRes.Length < 2 || !int.TryParse(splitRes[0], out int _) || !int.TryParse(splitRes[1], out int _))
            {
                throw new ArgumentOutOfRangeException(nameof(TargetResolution), $"Unable to parse target resolution ({TargetResolution})");
            }

            targetResolution = value;
        }
    }
}