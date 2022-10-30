namespace ArcadeManager.Actions {

	/// <summary>
	/// Represents a CSV action message
	/// </summary>
	public class CsvAction {

		/// <summary>
		/// Gets or sets the path to the main file.
		/// </summary>
		public string main { get; set; }

		/// <summary>
		/// Gets or sets the path to the secondary file.
		/// </summary>
		public string secondary { get; set; }

		/// <summary>
		/// Gets or sets the path to the target file.
		/// </summary>
		public string target { get; set; }
	}
}