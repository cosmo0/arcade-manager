namespace ArcadeManager.Actions {

	/// <summary>
	/// Rom adding action
	/// </summary>
	public class RomsAction {

		/// <summary>
		/// Gets or sets the path to the main file
		/// </summary>
		public string main { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to overwrite the files
		/// </summary>
		public bool overwrite { get; set; }

		/// <summary>
		/// Gets or sets the path to the full romset
		/// </summary>
		public string romset { get; set; }

		/// <summary>
		/// Gets or sets the path to the roms selection
		/// </summary>
		public string selection { get; set; }
	}
}
