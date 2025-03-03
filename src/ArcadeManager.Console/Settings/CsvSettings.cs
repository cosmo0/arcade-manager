using System;
using System.ComponentModel;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Settings;

public class CsvSettings : CommandSettings
{
    [Description("The path to the main file")]
    [CommandOption("-m|--main <MAIN_FILE>")]
    public string? Main { get; set; }

    [Description("The path to the secondary file")]
    [CommandOption("-s|--secondary <SECONDARY_FILE>")]
    public string? Secondary { get; set; }

    [Description("The path to the target file")]
    [CommandOption("-t|--target <TARGET_FILE>")]
    public string? Target { get; set; }
}
