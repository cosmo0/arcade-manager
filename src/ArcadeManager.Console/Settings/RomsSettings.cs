using System;
using System.ComponentModel;
using ArcadeManager.Actions;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Settings;

public class RomsSettings : CommandSettings
{
    [Description("The path to the CSV file")]
    [CommandOption("-c|--csv <CSV_FILE>")]
    public string? Csv { get; set; }

    [Description("The path to the romset folder")]
    [CommandOption("-r|--romset <ROMSET_FOLDER>")]
	public string? Romset { get; set; }

    [Description("The path to the selection folder")]
    [CommandOption("-s|--selection <SELECTION_FOLDER>")]
	public string? Selection { get; set; }

    [Description("Whether to overwrite existing files")]
    [CommandOption("-o|--overwrite")]
	public bool Overwrite { get; set; }

    public RomsAction ToAction()
    {
        return new Actions.RomsAction() {
            Main = Csv,
            Romset = Romset,
            Selection = Selection,
            Overwrite = Overwrite
        };
    }
}
