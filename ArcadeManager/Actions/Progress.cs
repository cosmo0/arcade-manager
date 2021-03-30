namespace ArcadeManager.Actions {

	/// <summary>
	/// An action progression
	/// </summary>
	public class Progress {

		/// <summary>
		/// Gets or sets a value indicating whether the operation can be cancelled
		/// </summary>
		public bool canCancel { get; set; } = false;

		/// <summary>
		/// Gets or sets a value indicating whether the operation has been cancelled
		/// </summary>
		public bool cancelled { get; set; } = false;

		/// <summary>
		/// Gets or sets the current item
		/// </summary>
		public int current { get; set; } = 0;

		/// <summary>
		/// Gets or sets a value indicating whether the progress is ending
		/// </summary>
		public bool end { get; set; } = false;

		/// <summary>
		/// Gets or sets the results folder
		/// </summary>
		public string folder { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the progress is initializing
		/// </summary>
		public bool init { get; set; } = false;

		/// <summary>
		/// Gets or sets the label to display
		/// </summary>
		public string label { get; set; }

		/// <summary>
		/// Gets or sets the total number of items to process
		/// </summary>
		public int total { get; set; } = 0;
	}
}
