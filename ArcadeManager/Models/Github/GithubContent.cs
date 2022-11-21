namespace ArcadeManager.Models;

/// <summary>
/// A Github content entry
/// </summary>
public class GithubContent {

    /// <summary>
    /// Gets or sets the links.
    /// </summary>
    public Links _links { get; set; }

    /// <summary>
    /// Gets or sets the download URL.
    /// </summary>
    public object download_url { get; set; }

    /// <summary>
    /// Gets or sets the Git URL.
    /// </summary>
    public string git_url { get; set; }

    /// <summary>
    /// Gets or sets the HTML URL.
    /// </summary>
    public string html_url { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public string path { get; set; }

    /// <summary>
    /// Gets or sets the SHA hash.
    /// </summary>
    public string sha { get; set; }

    /// <summary>
    /// Gets or sets the entry size.
    /// </summary>
    public int size { get; set; }

    /// <summary>
    /// Gets or sets the type of the entry.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string url { get; set; }

    /// <summary>
    /// The links of a folder
    /// </summary>
    public class Links {

        /// <summary>
        /// Gets or sets the git link.
        /// </summary>
        public string git { get; set; }

        /// <summary>
        /// Gets or sets the HTML link.
        /// </summary>
        public string html { get; set; }

        /// <summary>
        /// Gets or sets the self-referencing link.
        /// </summary>
        public string self { get; set; }
    }
}