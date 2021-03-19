using System.Text.Json;

namespace ArcadeManager.Services {

	/// <summary>
	/// Serializer provider
	/// </summary>
	public static class Serializer {
		private static readonly JsonSerializerOptions options;

		/// <summary>
		/// Initializes the <see cref="Serializer"/> class.
		/// </summary>
		static Serializer() {
			options = new() {
				PropertyNamingPolicy = new LowerCaseNamingPolicy(),
				PropertyNameCaseInsensitive = true
			};
		}

		/// <summary>
		/// Deserializes the provided input
		/// </summary>
		/// <typeparam name="T">The type to deserialize into</typeparam>
		/// <param name="input">The input string</param>
		/// <returns>The deserialized object</returns>
		public static T Deserialize<T>(string input) {
			return JsonSerializer.Deserialize<T>(input, options);
		}

		/// <summary>
		/// Serializes the provided object
		/// </summary>
		/// <typeparam name="T">The type of the object</typeparam>
		/// <param name="input">The object to serialize</param>
		/// <returns>The serialized object</returns>
		public static string Serialize<T>(T input) {
			return JsonSerializer.Serialize(input, options);
		}

		/// <summary>
		/// Lowercase naming policy
		/// </summary>
		public class LowerCaseNamingPolicy : JsonNamingPolicy {

			public override string ConvertName(string name) =>
				name?.ToLower();
		}
	}
}
