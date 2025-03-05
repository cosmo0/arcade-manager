using System;
using System.Runtime.Serialization;

namespace ArcadeManager.Core.Exceptions;

/// <summary>
/// Exception when a path (file or directory) has not been found
/// </summary>
/// <seealso cref="Exception"/>
[Serializable]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Serialization",
    "S3925:Update this implementation of 'ISerializable' to conform to the recommended serialization pattern",
    Justification = "Not serialized")]
public class PathNotFoundException : Exception {

    /// <summary>
    /// Initializes a new instance of the <see cref="PathNotFoundException"/> class.
    /// </summary>
    public PathNotFoundException() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PathNotFoundException(string message) : base(message) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference ( <see
    /// langword="Nothing"/> in Visual Basic) if no inner exception is specified.
    /// </param>
    public PathNotFoundException(string message, Exception innerException) : base(message, innerException) {
    }
}