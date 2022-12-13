using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace ArcadeManager.Infrastructure;

/// <summary>
/// Serializer provider
/// </summary>
public static class Serializer
{

    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes the provided input
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="input">The input string</param>
    /// <returns>The deserialized object</returns>
    public static T Deserialize<T>(string input)
    {
        return JsonSerializer.Deserialize<T>(input, options);
    }

    /// <summary>
    /// Deserializes an XML file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <param name="reader">The reader.</param>
    /// <returns>The deserialized file</returns>
    public static T DeserializeXml<T>(XmlReader reader)
    {
        XmlSerializer serializer = new(typeof(T));

        return (T)serializer.Deserialize(reader);
    }

    /// <summary>
    /// Serializes the provided object
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <param name="input">The object to serialize</param>
    /// <returns>The serialized object</returns>
    public static string Serialize<T>(T input)
    {
        return JsonSerializer.Serialize(input, options);
    }

    /// <summary>
    /// Lowercase naming policy
    /// </summary>
    public class LowerCaseNamingPolicy : JsonNamingPolicy
    {

        /// <summary>
        /// Converts the specified name to lower case
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public override string ConvertName(string name) =>
            name?.ToLower();
    }
}