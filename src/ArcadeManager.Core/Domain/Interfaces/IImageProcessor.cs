using ArcadeManager.Core.Models.Bezels;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Infrastructure.Interfaces;

public interface IImageProcessor
{
    /// <summary>
    /// Draws a debug rectangle on the overlay image
    /// </summary>
    /// <param name="debugImage">The path to the debug image</param>
    /// <param name="sourceImagePath">The path to the image</param>
    /// <param name="position">The position of the screen</param>
    Task DebugDraw(string debugImage, string sourceImagePath, Bounds position, Bounds resolution);

    /// <summary>
    /// Finds the screen position in the specified bezel, based on transparency.
    /// </summary>
    /// <param name="bezel">The bezel file.</param>
    /// <param name="margin">The margins to apply.</param>
    /// <returns>The screen position</returns>
    Bounds FindScreen(byte[] bezel, int margin);

    /// <summary>
    /// Gets the image size.
    /// </summary>
    /// <param name="imagePath">The image path.</param>
    /// <returns>The image size</returns>
    Bounds GetSize(string imagePath);

    /// <summary>
    /// Gets the image size.
    /// </summary>
    /// <param name="imageContent">The image content.</param>
    /// <returns>
    /// The image size
    /// </returns>
    Bounds GetSize(byte[] imageContent);

    /// <summary>
    /// Resizes an image to the specified dimension, cropping it if necessary
    /// </summary>
    /// <param name="imagePath">The path to the image</param>
    /// <param name="width">The target width</param>
    /// <param name="height">The target height</param>
    Task Resize(string imagePath, int width, int height);

    /// <summary>
    /// Resizes an image to the specified dimension, cropping it if necessary
    /// </summary>
    /// <param name="image">The image data</param>
    /// <param name="width">The target width</param>
    /// <param name="height">The target height</param>
    Task<byte[]> Resize(byte[] bezel, int width, int height);
}