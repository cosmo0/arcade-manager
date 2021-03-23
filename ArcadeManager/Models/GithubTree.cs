using System.Collections.Generic;

namespace ArcadeManager.Models {

	/// <summary>
	/// Represents a list of entries in a Github folder
	/// </summary>
	public class GithubTree {

		/// <summary>
		/// Gets or sets the SHA hash of the folder.
		/// </summary>
		public string sha { get; set; }

		/// <summary>
		/// Gets or sets the list of entries.
		/// </summary>
		public List<Entry> tree { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="GithubTree"/> is truncated.
		/// </summary>
		public bool truncated { get; set; }

		/// <summary>
		/// Gets or sets the URL of the folder.
		/// </summary>
		public string url { get; set; }

		/// <summary>
		/// Represents a file
		/// </summary>
		public class Entry {

			/// <summary>
			/// Gets or sets the entry mode.
			/// </summary>
			public string mode { get; set; }

			/// <summary>
			/// Gets or sets the path.
			/// </summary>
			public string path { get; set; }

			/// <summary>
			/// Gets or sets the entry SHA hash.
			/// </summary>
			public string sha { get; set; }

			/// <summary>
			/// Gets or sets the entry size.
			/// </summary>
			public int size { get; set; }

			/// <summary>
			/// Gets or sets the entry type.
			/// </summary>
			public string type { get; set; }

			/// <summary>
			/// Gets or sets the URL.
			/// </summary>
			public string url { get; set; }
		}
	}
}
