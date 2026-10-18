using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Bezels;
using ImageMagick;
using ImageMagick.Drawing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArcadeManager.Core.Domain;

public class ImageProcessor : IImageProcessor
{
    private static readonly ushort maxTransparentAlpha = ushort.MaxValue;

    /// <summary>
    /// Draws a debug rectangle on the overlay image
    /// </summary>
    /// <param name="debugImage">The path to the debug image</param>
    /// <param name="sourceImagePath">The path to the image</param>
    /// <param name="position">The position of the screen</param>
    public async Task DebugDraw(string debugImage, string sourceImagePath, Bounds position, Bounds resolution)
    {
        using MagickImage image = new(sourceImagePath);

        // resize if necessary (RA would do it automatically)
        if (resolution != null && (image.Width != (int)resolution.Width || image.Height != (int)resolution.Height))
        {
            image.Resize((uint)resolution.Width, (uint)resolution.Height);
        }

        // draw screen position
        new Drawables()
            .StrokeColor(new MagickColor(MagickColors.Red))
            .StrokeWidth(5)
            .FillColor(new MagickColor(MagickColors.Transparent))
            .Rectangle(position.X, position.Y, position.X + position.Width, position.Y + position.Height)
            .Draw(image);

        await image.WriteAsync(debugImage, MagickFormat.Png32);
    }

    /// <summary>
    /// Finds the screen position in the specified bezel, based on transparency.
    /// </summary>
    /// <param name="bezel">The bezel file.</param>
    /// <param name="margin">The margins to apply.</param>
    /// <returns>The screen position</returns>
    public Bounds FindScreen(byte[] bezel, int margin)
    {
        using MagickImage image = new(bezel);

        IPixelCollection<ushort> pixels = image.GetPixels();

        // scan around vertical center to find the screen (it's probably more or less
        // centered horizontally)
        int centerX = (int)(image.Width / 2);
        var (topCenter, bottomCenter) = GetTransparentPixelsInColumn(pixels, image.Height, centerX);
        var (topCenterPlus, bottomCenterPlus) = GetTransparentPixelsInColumn(pixels, image.Height, centerX + 50);
        var (topCenterMinus, bottomCenterMinus) = GetTransparentPixelsInColumn(pixels, image.Height, centerX - 50);

        int top = Math.Min(topCenter, Math.Min(topCenterPlus, topCenterMinus));
        int bottom = Math.Max(bottomCenter, Math.Max(bottomCenterMinus, bottomCenterPlus));
        int height = bottom - top;

        if (top == 0 && bottom == 0)
        {
            // no transparent area found
            // TODO: throw an exception and handle it in the UI
            return new() { X = 0, Y = 0, Width = 0, Height = 0 };
        }

        // screen is found, now scan horizontally based on these positions (it's often not
        // centered vertically)
        int centerY = top + (height / 2);
        var (leftCenter, rightCenter) = GetTransparentPixelsInRow(pixels, image.Width, centerY);
        var (leftCenterPlus, rightCenterPlus) = GetTransparentPixelsInRow(pixels, image.Width, centerY + 50);
        var (leftCenterMinus, rightCenterMinus) = GetTransparentPixelsInRow(pixels, image.Width, centerY - 50);

        int left = Math.Min(leftCenter, Math.Min(leftCenterMinus, leftCenterPlus));
        int right = Math.Max(rightCenter, Math.Max(rightCenterMinus, rightCenterPlus));
        int width = right - left;

        // apply margins
        if (margin > 0)
        {
            top -= top - margin > 0 ? margin : top;
            left -= left - margin > 0 ? margin : left;
            width += margin * 2;
            height += margin * 2;
        }

        return new Bounds
        {
            X = left,
            Y = top,
            Width = width,
            Height = height
        };
    }

    /// <summary>
    /// Gets the image size.
    /// </summary>
    /// <param name="imagePath">The image path.</param>
    /// <returns>The image size</returns>
    public Bounds GetSize(string imagePath)
    {
        using MagickImage image = new(imagePath);

        return new Bounds
        {
            X = 0,
            Y = 0,
            Width = image.Width,
            Height = image.Height
        };
    }

    /// <summary>
    /// Gets the image size.
    /// </summary>
    /// <param name="imageContent">The image content.</param>
    /// <returns>
    /// The image size
    /// </returns>
    public Bounds GetSize(byte[] imageContent)
    {
        using MagickImage image = new(imageContent);

        return new Bounds
        {
            X = 0,
            Y = 0,
            Width = image.Width,
            Height = image.Height
        };
    }

    /// <summary>
    /// Resizes an image to the specified dimension, cropping it if necessary
    /// </summary>
    /// <param name="imagePath">The path to the image</param>
    /// <param name="width">The target width</param>
    /// <param name="height">The target height</param>
    public async Task Resize(string imagePath, int width, int height)
    {
        using MagickImage image = new(imagePath);

        ResizeImageInternal(image, width, height);

        await image.WriteAsync(imagePath, MagickFormat.Png32);
    }

    /// <summary>
    /// Resizes an image to the specified dimension, cropping it if necessary
    /// </summary>
    /// <param name="image">The image data</param>
    /// <param name="width">The target width</param>
    /// <param name="height">The target height</param>
    public async Task<byte[]> Resize(byte[] bezel, int width, int height)
    {
        using MagickImage image = new(bezel);

        ResizeImageInternal(image, width, height);

        using var ms = new MemoryStream();

        await image.WriteAsync(ms, MagickFormat.Png32);
        return ms.ToArray();
    }

    /// <summary>
    /// Gets the first and last transparent pixels in the specified column.
    /// </summary>
    /// <param name="pixels">The pixels.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="column">The column to scan.</param>
    /// <returns>
    /// The first and last transparent pixels
    /// </returns>
    private static (int first, int last) GetTransparentPixelsInColumn(IPixelCollection<ushort> pixels, uint height, int column)
    {
        int first = 0;
        int last = 0;

        for (int y = 0; y < height; y++)
        {
            ushort alpha = pixels.GetPixel(column, y).ToColor().A;

            // first transparent pixel
            if (first == 0 && alpha < maxTransparentAlpha)
            {
                first = y;
            }

            // last transparent pixel
            if (alpha < maxTransparentAlpha)
            {
                last = y;
            }

            (first, last, bool shouldBreak) = IsLastTransparentPixel(alpha, first, last);
            if (shouldBreak) { break; }
        }

        return (first, last);
    }

    /// <summary>
    /// Gets the first and last transparent pixels in the specified row.
    /// </summary>
    /// <param name="pixels">The pixels.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="row">The row to scan.</param>
    /// <returns>
    /// The first and last transparent pixels
    /// </returns>
    private static (int first, int last) GetTransparentPixelsInRow(IPixelCollection<ushort> pixels, uint width, int row)
    {
        int first = 0;
        int last = 0;

        for (int x = 0; x < width; x++)
        {
            ushort alpha = pixels.GetPixel(x, row).ToColor().A;

            // first transparent pixel
            if (first == 0 && alpha < maxTransparentAlpha)
            {
                first = x;
            }

            // last transparent pixel
            if (alpha < maxTransparentAlpha)
            {
                last = x;
            }

            (first, last, bool shouldBreak) = IsLastTransparentPixel(alpha, first, last);
            if (shouldBreak) { break; }
        }

        return (first, last);
    }

    private static (int first, int last, bool shouldBreak) IsLastTransparentPixel(ushort alpha, int first, int last)
    {
        bool shouldBreak = false;

        // last transparent pixel has been found
        if (alpha >= maxTransparentAlpha && last > 0)
        {
            // arbitrary margin of error in case a transparent pixel (or column) exists
            // somewhere in the bezel
            if (last - first < 100)
            {
                // it's an error = reset values
                first = 0;
                last = 0;
            }
            else
            {
                shouldBreak = true;
            }
        }

        return (first, last, shouldBreak);
    }

    private static void ResizeImageInternal(MagickImage image, int width, int height)
    {
        if (image.Width > width || image.Height > height)
        {
            var size = new MagickGeometry((uint)width, (uint)height)
            {
                IgnoreAspectRatio = true
            };

            image.Resize(size);
        }
    }
}