namespace ArcadeManager.Actions {

	/// <summary>
	/// A download action request
	/// </summary>
	public class DownloadAction {

		/// <summary>
		/// Gets or sets the details.
		/// </summary>
		public string details { get; set; }

		/// <summary>
		/// Gets or sets the folder.
		/// </summary>
		public string folder { get; set; }

		/// <summary>
		/// Gets or sets the local file path.
		/// </summary>
		public string localfile { get; set; }

		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		public string path { get; set; }

		/// <summary>
		/// Gets or sets the repository (ex: cosmo0/arcade-manager-data)
		/// </summary>
		public string repository { get; set; }
	}
}
