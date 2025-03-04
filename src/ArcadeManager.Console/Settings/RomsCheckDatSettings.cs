using System;
using System.ComponentModel;
using ArcadeManager.Actions;
using Spectre.Console.Cli;

namespace ArcadeManager.Console.Settings;

public class RomsCheckDatSettings : CommandSettings
{
    [Description("The path to the romset folder")]
    [CommandOption("--romset <ROMSET>")]
    public string? Romset { get; set; }
    
    [Description("The path to the DAT file")]
    [CommandOption("--dat <DATFILE>")]
    public string? DatFile { get; set; }
    
    [Description("Whether to change the type of the romset to non-merged; otherwise it just checks it")]
    [CommandOption("--changetype")]
    public bool ChangeType { get; set; }
    
    [Description("The path to the target folder")]
    [CommandOption("--target <ROMSET>")]
    public string? TargetFolder { get; set; }
    
    [Description("Whether to report all missing files; otherwise it just checks the ones that exist in the folder")]
    [CommandOption("--reportall")]
    public bool ReportAll { get; set; }
    
    [Description("Whether to check the SHA1 of the files; otherwise it just checks the CRC32")]
    [CommandOption("--checksha1")]
    public bool CheckSha1 { get; set; }
    
    [Description("(optional) The path to a CSV filter file")]
    [CommandOption("--csvfilter <CSVFILTER>")]
    public string? CsvFilter { get; set; }
    
    [Description("The path to another folder to rebuild the romset (for example, a recent romset to rebuild an older one)")]
    [CommandOption("--otherfolder <OTHERFOLDER>")]
    public string? OtherFolder { get; set; }

    public RomsActionCheckDat ToAction()
    {
        return new() {
            Romset = this.Romset,
            DatFile = "custom",
            DatFilePath = this.DatFile,
            ChangeType = this.ChangeType,
            TargetFolder = this.TargetFolder,
            OtherFolder = this.OtherFolder,
            CsvFilter = this.CsvFilter,
            ReportAll = this.ReportAll,
            Speed = this.CheckSha1 ? "slow" : "fast"
        };
    }
}
