using System.Xml.Serialization;

/// <summary>
/// Conversion of DAT files using json2csharp.com/xml-to-csharp
/// </summary>
namespace ArcadeManager.Core.Models.DataFile;

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
