using System;

namespace ArcadeManager.Core.Tests;

public class IgnoreLocallyFactAttribute : FactAttribute
{
    public IgnoreLocallyFactAttribute()
    {
        if (!IsCI)
        {
            Skip = "Ignore test except on CI build; set the environment variable 'CI' to run it locally";
        }
    }

    private static bool IsCI => Environment.GetEnvironmentVariable("CI") != null;
}