using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcadeManager.Core.Models.Github;

/// <summary>
/// A Github release
/// </summary>
public class GithubRelease {

    /// <summary>
    /// Gets or sets the assets.
    /// </summary>
    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; }

    /// <summary>
    /// Gets or sets the release description.
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the current version.
    /// </summary>
    public string Current { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GithubRelease"/> is a draft.
    /// </summary>
    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    /// <summary>
    /// Gets or sets the release name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GithubRelease"/> is a prerelease.
    /// </summary>
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the publication date in local format
    /// </summary>
    public string PublishedAtLocal { get; set; }

    /// <summary>
    /// Gets or sets the name of the Git tag.
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    /// <summary>
    /// Gets or sets the release details URL.
    /// </summary>
    [JsonPropertyName("html_url")]
    public string URL { get; set; }

    /// <summary>
    /// A release asset (file)
    /// </summary>
    public class Asset {

        /// <summary>
        /// Gets or sets the download URL.
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        public string DownloadURL { get; set; }

        /// <summary>
        /// Gets or sets the size in a human-readable format.
        /// </summary>
        [JsonPropertyName("human_size")]
        public string HumanSize { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}