namespace ArcadeManager.Models {

	/// <summary>
	/// ViewModel for the errors
	/// </summary>
	public class ErrorViewModel {

		/// <summary>
		/// Gets or sets the request identifier.
		/// </summary>
		/// <value>
		/// The request identifier.
		/// </value>
		public string RequestId { get; set; }

		/// <summary>
		/// Gets a value indicating whether to show the request identifier.
		/// </summary>
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}
