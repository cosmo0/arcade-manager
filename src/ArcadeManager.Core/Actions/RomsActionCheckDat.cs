using System;

namespace ArcadeManager.Actions;

/// <summary>
/// Rom check action
/// </summary>
public class RomsActionCheckDat
{
    /// <summary>
    /// Gets or sets the path to the romset
    /// </summary>
    public string romset { get; set; }
    
    /// <summary>
    /// Gets or sets the pre-built DAT file identifier
    /// </summary>
    public string datfilePre { get; set; }
    
    /// <summary>
    /// Gets or sets a path to a custom DAT file
    /// </summary>
    public string datfileCustom { get; set; }
    
    /// <summary>
    /// Gets or sets the action to execute
    /// </summary>
    public string action { get; set; }
    
    /// <summary>
    /// Gets or sets the romset target type
    /// </summary>
    public string romsetType { get; set; }
    
    /// <summary>
    /// Gets or sets the target folder for fixes
    /// </summary>
    public string fixFolder { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to report all missing files; otherwise, it only reports error on roms existing on the disk
    /// </summary>
    public bool actionReportAll { get; set; }
    
    /// <summary>
    /// Gets or sets the check speed
    /// </summary>
    public string speed { get; set; }

    /// <summary>
    /// Gets a value indicating whether the processing speed is slow
    /// </summary>
    public bool isslow => speed != "fast";
    
    /// <summary>
    /// Gets or sets the path to a CSV filter
    /// </summary>
    public string csvfilter { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to check for missing BIOS files
    /// </summary>
    public bool otherBios { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to check for "devices" files
    /// </summary>
    public bool otherDevices { get; set; }
    
    /// <summary>
    /// Gets or sets the path to another folder to fix/rebuild the romset with
    /// </summary>
    public string otherFolder { get; set; }
}
