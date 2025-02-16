namespace ArcadeManager.Actions; 

/// <summary>
/// An action progression
/// </summary>
public class Progress {

	/// <summary>
	/// Gets or sets a value indicating whether the operation has been cancelled
	/// </summary>
	public bool Cancelled { get; set; } = false;

	/// <summary>
	/// Gets or sets the current item
	/// </summary>
	public int Current { get; set; } = 0;

	/// <summary>
	/// Gets or sets a value indicating whether the progress is ending
	/// </summary>
	public bool End { get; set; } = false;

	/// <summary>
	/// Gets or sets the results folder
	/// </summary>
	public string Folder { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the progress is initializing
	/// </summary>
	public bool Init { get; set; } = false;

	/// <summary>
	/// Gets or sets the label to display
	/// </summary>
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the total number of items to process
	/// </summary>
	public int Total { get; set; } = 0;
}