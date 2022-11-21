using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

/// <summary>
/// Conversion of DAT files using json2csharp.com/xml-to-csharp
/// </summary>
namespace ArcadeManager.Models.DatFile;

[XmlRoot(ElementName = "datafile")]
public class Datafile {

    [XmlIgnore]
    public List<Entry> Entries {
        get {
            if (Game != null && Game.Any()) {
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

public class Entry {

    [XmlElement(ElementName = "category")]
    public string Category { get; set; }

    [XmlAttribute(AttributeName = "cloneof")]
    public string Cloneof { get; set; }

    [XmlElement(ElementName = "description")]
    public string Description { get; set; }

    [XmlAttribute(AttributeName = "isdevice")]
    public string Isdevice { get; set; }

    [XmlElement(ElementName = "manufacturer")]
    public string Manufacturer { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }

    [XmlAttribute(AttributeName = "romof")]
    public string Romof { get; set; }

    [XmlAttribute(AttributeName = "runnable")]
    public string Runnable { get; set; }

    [XmlAttribute(AttributeName = "sampleof")]
    public string Sampleof { get; set; }

    [XmlAttribute(AttributeName = "sourcefile")]
    public string Sourcefile { get; set; }

    [XmlText]
    public string Text { get; set; }

    [XmlElement(ElementName = "year")]
    public string Year { get; set; }
}

[XmlRoot(ElementName = "header")]
public class Header {

    [XmlElement(ElementName = "author")]
    public string Author { get; set; }

    [XmlElement(ElementName = "category")]
    public string Category { get; set; }

    [XmlElement(ElementName = "comment")]
    public string Comment { get; set; }

    [XmlElement(ElementName = "description")]
    public string Description { get; set; }

    [XmlElement(ElementName = "name")]
    public string Name { get; set; }

    [XmlElement(ElementName = "version")]
    public string Version { get; set; }
}