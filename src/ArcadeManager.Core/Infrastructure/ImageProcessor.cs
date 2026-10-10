using ArcadeManager.Core.Infrastructure.Interfaces;
using ArcadeManager.Core.Models.Bezels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.ExceptionServices;

namespace ArcadeManager.Core.Infrastructure;

public class ImageProcessor(IFileSystem fs) : IImageProcessor
{
    /// <summary>
    /// Draws a debug rectangle on the overlay image
    /// </summary>
    /// <param name="game">The game name</param>
    /// <param name="debugFolder">The path to the debug folder</param>
    /// <param name="sourceImagePath">The path to the image</param>
    /// <param name="position">The position of the screen</param>
    public void DebugDraw(string game, string debugFolder, string sourceImagePath, Bounds position, Bounds resolution)
    {
        if (string.IsNullOrEmpty(debugFolder))
        {
            return;
        }

        var debugImage = fs.PathJoin(debugFolder, $"{game}.png");
        fs.FileCopy(sourceImagePath, debugImage, true);
        DrawDebugRect(debugImage, position, resolution);
    }

    /// <summary>
    /// Finds the screen position in the specified bezel, based on transparency.
    /// </summary>
    /// <param name="bezel">The bezel file.</param>
    /// <param name="margin">The margins to apply.</param>
    /// <returns>The screen position</returns>
    public Bounds FindScreen(byte[] bezel, int margin)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(bezel);

        // scan around vertical center to find the screen (it's probably more or less
        // centered horizontally)
        var centerX = image.Width / 2;
        var (topCenter, bottomCenter) = GetTransparentPixelsInColumn(image, centerX);
        var (topCenterPlus, bottomCenterPlus) = GetTransparentPixelsInColumn(image, centerX + 50);
        var (topCenterMinus, bottomCenterMinus) = GetTransparentPixelsInColumn(image, centerX - 50);

        var top = Math.Min(topCenter, Math.Min(topCenterPlus, topCenterMinus));
        var bottom = Math.Max(bottomCenter, Math.Max(bottomCenterMinus, bottomCenterPlus));
        var height = bottom - top;

        // screen is found, now scan horizontally based on these positions (it's often not
        // centered vertically)
        var centerY = top + (height / 2);
        var (leftCenter, rightCenter) = GetTransparentPixelsInRow(image, centerY);
        var (leftCenterPlus, rightCenterPlus) = GetTransparentPixelsInRow(image, centerY + 50);
        var (leftCenterMinus, rightCenterMinus) = GetTransparentPixelsInRow(image, centerY - 50);

        var left = Math.Min(leftCenter, Math.Min(leftCenterMinus, leftCenterPlus));
        var right = Math.Max(rightCenter, Math.Max(rightCenterMinus, rightCenterPlus));
        var width = right - left;

        // apply margins
        if (margin > 0)
        {
            top -= margin;
            left -= margin;
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
        using Image image = Image.Load(imagePath);

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
    public void Resize(string imagePath, int width, int height)
    {
        using Image image = Image.Load(imagePath);

        if (image.Width > width || image.Height > height)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size { Width = width, Height = height },
                Position = AnchorPositionMode.Center,
                Mode = ResizeMode.Crop
            }));
        }

        image.Save(imagePath);
    }

    /// <summary>
    /// Resizes an image to the specified dimension, cropping it if necessary
    /// </summary>
    /// <param name="image">The image data</param>
    /// <param name="width">The target width</param>
    /// <param name="height">The target height</param>
    public byte[] Resize(byte[] bezel, int width, int height)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(bezel);

        if (image.Width > width || image.Height > height)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size { Width = width, Height = height },
                Position = AnchorPositionMode.Center,
                Mode = ResizeMode.Crop
            }));
        }

        using var ms = new MemoryStream();

        image.Save(ms, new PngEncoder { ColorType = PngColorType.RgbWithAlpha });
        return ms.ToArray();
    }

    /// <summary>
    /// Draws a red rectangle on the specified image at the specified position
    /// </summary>
    /// <param name="imagePath">The path to the image</param>
    /// <param name="position">The position at which to draw the rectangle</param>
    /// <param name="resolution">The target image resolution.</param>
    private static void DrawDebugRect(string imagePath, Bounds position, Bounds resolution)
    {
        using Image image = Image.Load(imagePath);

        // resize if necessary (RA would do it automatically)
        if (resolution != null && (image.Width != (int)resolution.Width || image.Height != (int)resolution.Height))
        {
            image.Mutate(x => x.Resize((int)resolution.Width, (int)resolution.Height));
        }

        // draw screen position
        var pen = Pens.Solid(Color.Red, 5);
        var rect = new Rectangle((int)position.X, (int)position.Y, (int)position.Width, (int)position.Height);
        image.Mutate(x => x.Draw(pen, rect));

        image.Save(imagePath);
    }

    /// <summary>
    /// Gets the first and last transparent pixels in the specified column.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="column">The column to scan.</param>
    /// <returns>The first and last transparent pixels</returns>
    private static (int first, int last) GetTransparentPixelsInColumn(Image<Rgba32> image, int column)
    {
        int first = 0;
        int last = 0;

        for (int y = 0; y < image.Height; y++)
        {
            // first transparent pixel
            if (first == 0 && image[column, y].A < 255)
            {
                first = y;
            }

            // last transparent pixel
            if (image[column, y].A < 255)
            {
                last = y;
            }

            (first, last, bool shouldBreak) = IsLastTransparentPixel(image[column, y].A, first, last);
            if (shouldBreak) { break; }
        }

        return (first, last);
    }

    /// <summary>
    /// Gets the first and last transparent pixels in the specified row.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="row">The row to scan.</param>
    /// <returns>The first and last transparent pixels</returns>
    private static (int first, int last) GetTransparentPixelsInRow(Image<Rgba32> image, int row)
    {
        int first = 0;
        int last = 0;

        image.ProcessPixelRows(accessor =>
        {
            Span<Rgba32> pixelRowSpan = accessor.GetRowSpan(row);
            for (int x = 0; x < image.Width; x++)
            {
                // first transparent pixel
                if (first == 0 && pixelRowSpan[x].A < 255)
                {
                    first = x;
                }

                // last transparent pixel
                if (pixelRowSpan[x].A < 255)
                {
                    last = x;
                }

                (first, last, bool shouldBreak) = IsLastTransparentPixel(pixelRowSpan[x].A, first, last);
                if (shouldBreak) { break; }
            }
        });

        return (first, last);
    }

    private static (int first, int last, bool shouldBreak) IsLastTransparentPixel(byte alpha, int first, int last)
    {
        bool shouldBreak = false;

        // last transparent pixel has been found
        if (alpha == 255 && last > 0)
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
}