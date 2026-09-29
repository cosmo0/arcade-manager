using ArcadeManager.Core.Models.Bezels;
using System;

namespace ArcadeManager.Core.Models.Actions.Overlays;

/// <summary>
/// Command line base option arguments
/// </summary>
public abstract class BaseAction
{
    private string targetResolution = "1920x1080";
    
    /// <summary>
    /// Gets or sets the margins applied to the screen after conversion.
    /// </summary>
    public int Margin { get; set; } = 0;

    /// <summary>
    /// Gets or sets a path for debug purpose
    /// </summary>
    public string OutputDebug { get; set; }

    /// <summary>
    /// Gets or sets the target overlay resolution
    /// </summary>
    public string TargetResolution
    {
        get
        {
            return targetResolution;
        }
        set
        {
            var splitRes = value.Split('x', '*', ':', '/');
            if (splitRes.Length < 2 || !int.TryParse(splitRes[0], out int _) || !int.TryParse(splitRes[1], out int _))
            {
                throw new ArgumentOutOfRangeException(nameof(TargetResolution), $"Unable to parse target resolution ({TargetResolution})");
            }

            targetResolution = value;
        }
    }

    /// <summary>
    /// Gets the target resolution bounds
    /// </summary>
    public Bounds TargetResolutionBounds
    {
        get
        {
            var splitRes = TargetResolution.Split('x', '*', ':', '/');

            return new Bounds
            {
                X = 0,
                Y = 0,
                Width = int.Parse(splitRes[0]),
                Height = int.Parse(splitRes[1])
            };
        }
    }

    /// <summary>
    /// Gets or sets the number of threads on which to run the conversion
    /// </summary>
    public int Threads { get; set; } = 1;
}