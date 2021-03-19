namespace ArcadeManager.Actions {

	/// <summary>
	/// Progression
	/// </summary>
	public class Progress {

		/// <summary>
		/// Whether the operation can be cancelled
		/// </summary>
		public bool canCancel { get; set; } = false;

		/// <summary>
		/// Whether the operation has been cancelled
		/// </summary>
		public bool cancelled { get; set; } = false;

		/// <summary>
		/// The current item
		/// </summary>
		public int current { get; set; } = 0;

		/// <summary>
		/// Whether the progress is ending
		/// </summary>
		public bool end { get; set; } = false;

		/// <summary>
		/// The results folder
		/// </summary>
		public string folder { get; set; }

		/// <summary>
		/// Whether the progress is initializing
		/// </summary>
		public bool init { get; set; } = false;

		/// <summary>
		/// The label to display
		/// </summary>
		public string label { get; set; }

		/// <summary>
		/// The total number of items to process
		/// </summary>
		public int total { get; set; } = 0;
	}
}
