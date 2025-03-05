using System;

namespace ArcadeManager.Core.Actions;

/// <summary>
/// Rom check action
/// </summary>
public class RomsActionCheckDat
{
    /// <summary>
    /// Gets or sets the path to the romset
    /// </summary>
    public string Romset { get; set; }
    
    /// <summary>
    /// Gets or sets the pre-built DAT file identifier
    /// </summary>
    public string DatFile { get; set; }
    
    /// <summary>
    /// Gets or sets a path to a custom DAT file
    /// </summary>
    public string DatFilePath { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to change the type of the romset; otherwise, it simply checks it
    /// </summary>
    public bool ChangeType { get; set; }
    
    /// <summary>
    /// Gets or sets the target folder for fixes
    /// </summary>
    public string TargetFolder { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to report all missing files; otherwise, it only reports error on roms existing on the disk
    /// </summary>
    public bool ReportAll { get; set; }
    
    /// <summary>
    /// Gets or sets the check speed
    /// </summary>
    public string Speed { get; set; }

    /// <summary>
    /// Gets a value indicating whether to check the SHA1 of the files
    /// </summary>
    public bool CheckSha1 => Speed != "fast";
    
    /// <summary>
    /// Gets or sets the path to a CSV filter
    /// </summary>
    public string CsvFilter { get; set; }
    
    /// <summary>
    /// Gets or sets the path to another folder to fix/rebuild the romset with
    /// </summary>
    public string OtherFolder { get; set; }
}
