using System.Collections.Generic;

namespace ArcadeManager.Models {

	/// <summary>
	/// A game entry in a CSV file
	/// </summary>
	public class GameEntry {

		/// <summary>
		/// Gets or sets the game's name
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Gets or sets the additional game entry values.
		/// </summary>
		public IDictionary<string, string> values { get; set; } = new Dictionary<string, string>();
	}
}
