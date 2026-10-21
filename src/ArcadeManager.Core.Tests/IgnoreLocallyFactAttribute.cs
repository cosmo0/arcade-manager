using System;
using System.Runtime.CompilerServices;

namespace ArcadeManager.Core.Tests;

public class IgnoreLocallyFactAttribute : FactAttribute
{
    public IgnoreLocallyFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!IsCI)
        {
            Skip = "Ignore test except on CI build; set the environment variable 'CI' to run it locally";
        }
    }

    private static bool IsCI => Environment.GetEnvironmentVariable("CI") != null;
}