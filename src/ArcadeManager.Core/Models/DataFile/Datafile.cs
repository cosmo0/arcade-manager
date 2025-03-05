using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

/// <summary>
/// Conversion of DAT files using json2csharp.com/xml-to-csharp
/// </summary>
namespace ArcadeManager.Core.Models.DataFile;

[XmlRoot(ElementName = "datafile")]
public class Datafile {

    [XmlIgnore]
    public List<Entry> Entries {
        get {
            if (Game != null && Game.Count != 0) {
                return Game;
            }

            return Machine;
        }
    }

    [XmlElement(ElementName = "game")]
    public List<Entry> Game { get; set; }

    [XmlElement(ElementName = "header")]
    public Header Header { get; set; }

    [XmlElement(ElementName = "machine")]
    public List<Entry> Machine { get; set; }
}
