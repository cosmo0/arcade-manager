using System;

namespace ArcadeManager.Actions;

public class RomsActionCheckDat
{
    public string romset { get; set; }
    
    public string datfilePre { get; set; }
    
    public string datfileCustom { get; set; }
    
    public string action { get; set; }
    
    public string romsetType { get; set; }
    
    public string fixFolder { get; set; }
    
    public bool actionReportAll { get; set; }
    
    public string speed { get; set; }
    
    public string csvfilter { get; set; }
    
    public bool otherBios { get; set; }
    
    public bool otherDevices { get; set; }
    
    public string otherFolder { get; set; }
    
}
