using System.Collections.Generic;

namespace ArcadeManager.Models;

/// <summary>
/// The description of a downloadable CSV file
/// </summary>
public class CsvFile {

    /// <summary>
    /// Gets or sets the file description
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string filename { get; set; }

    /// <summary>
    /// Gets or sets the file types (ex: [ "set", "main" ])
    /// </summary>
    public string[] types { get; set; }
}

/// <summary>
/// A list of CSV files
/// </summary>
public class CsvFilesList {

    /// <summary>
    /// The CSV files
    /// </summary>
    public IEnumerable<CsvFile> files { get; set; }
}