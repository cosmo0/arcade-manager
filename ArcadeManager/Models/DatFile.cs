using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Conversion of DAT files using json2csharp.com/xml-to-csharp
/// </summary>
namespace ArcadeManager.Models.DatFile {

	public abstract class BaseEntry {

		[XmlElement(ElementName = "description")]
		public string Description { get; set; }

		[XmlElement(ElementName = "manufacturer")]
		public string Manufacturer { get; set; }

		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "sourcefile")]
		public string Sourcefile { get; set; }

		[XmlText]
		public string Text { get; set; }

		[XmlElement(ElementName = "year")]
		public string Year { get; set; }
	}

	[XmlRoot(ElementName = "datafile")]
	public class Datafile {

		[XmlElement(ElementName = "game")]
		public List<Game> Game { get; set; }

		[XmlElement(ElementName = "header")]
		public Header Header { get; set; }

		[XmlElement(ElementName = "machine")]
		public List<Machine> Machine { get; set; }
	}

	[XmlRoot(ElementName = "game")]
	public class Game : BaseEntry {

		[XmlElement(ElementName = "category")]
		public string Category { get; set; }

		[XmlAttribute(AttributeName = "cloneof")]
		public string Cloneof { get; set; }

		[XmlAttribute(AttributeName = "romof")]
		public string Romof { get; set; }

		[XmlAttribute(AttributeName = "sampleof")]
		public string Sampleof { get; set; }
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

	[XmlRoot(ElementName = "machine")]
	public class Machine : BaseEntry {
		//[XmlElement(ElementName = "device_ref")]
		//public List<DeviceRef> DeviceRef { get; set; }

		[XmlAttribute(AttributeName = "isdevice")]
		public string Isdevice { get; set; }

		[XmlAttribute(AttributeName = "runnable")]
		public string Runnable { get; set; }
	}
}
