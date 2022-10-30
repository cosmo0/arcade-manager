using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcadeManager.Models {

	/// <summary>
	/// Represents a list of entries in a Github folder
	/// </summary>
	public class GithubTree {

		/// <summary>
		/// Gets or sets the SHA hash of the folder.
		/// </summary>
		[JsonPropertyName("sha")]
		public string SHA { get; set; }

		/// <summary>
		/// Gets or sets the list of entries.
		/// </summary>
		[JsonPropertyName("tree")]
		public List<Entry> Tree { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="GithubTree"/> is truncated.
		/// </summary>
		[JsonPropertyName("truncated")]
		public bool Truncated { get; set; }

		/// <summary>
		/// Gets or sets the URL of the folder.
		/// </summary>
		[JsonPropertyName("url")]
		public string URL { get; set; }

		/// <summary>
		/// Represents a file
		/// </summary>
		public class Entry {

			/// <summary>
			/// Gets a value indicating whether this instance is a file.
			/// </summary>
			public bool IsFile {
				get {
					return this.Type == "file" || this.Type == "blob";
				}
			}

			/// <summary>
			/// Gets or sets the entry mode.
			/// </summary>
			[JsonPropertyName("mode")]
			public string Mode { get; set; }

			/// <summary>
			/// Gets or sets the path.
			/// </summary>
			[JsonPropertyName("path")]
			public string Path { get; set; }

			/// <summary>
			/// Gets or sets the entry SHA hash.
			/// </summary>
			[JsonPropertyName("sha")]
			public string SHA { get; set; }

			/// <summary>
			/// Gets or sets the entry size.
			/// </summary>
			[JsonPropertyName("size")]
			public int Size { get; set; }

			/// <summary>
			/// Gets or sets the entry type.
			/// </summary>
			[JsonPropertyName("type")]
			public string Type { get; set; }

			/// <summary>
			/// Gets or sets the URL.
			/// </summary>
			[JsonPropertyName("url")]
			public string URL { get; set; }
		}
	}
}