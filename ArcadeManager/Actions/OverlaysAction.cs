namespace ArcadeManager.Actions {

	/// <summary>
	/// An overlay action
	/// </summary>
	public class OverlaysAction {

		/// <summary>
		/// Gets or sets the path to the configs folder
		/// </summary>
		public string configFolder { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the files should be overwritten
		/// </summary>
		public bool overwrite { get; set; }

		/// <summary>
		/// Gets or sets the selected pack name
		/// </summary>
		public string pack { get; set; }

		/// <summary>
		/// Gets or sets the target size ratio
		/// </summary>
		public float ratio { get; set; }

		/// <summary>
		/// Gets or sets the path to the roms folders
		/// </summary>
		public string[] romFolders { get; set; }
	}
}