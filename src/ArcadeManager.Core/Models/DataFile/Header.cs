using System.Xml.Serialization;

/// <summary>
/// Conversion of DAT files using json2csharp.com/xml-to-csharp
/// </summary>
namespace ArcadeManager.Core.Models.DataFile;

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