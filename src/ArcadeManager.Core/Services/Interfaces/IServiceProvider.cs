using System;

namespace ArcadeManager.Services;

/// <summary>
/// Interface for the service provider
/// </summary>
public interface IServiceProvider
{
    /// <summary>
    /// Gets the CSV service
    /// </summary>
    ICsv Csv { get; }

    /// <summary>
    /// Gets the DAT checker service
    /// </summary>
    IDatChecker DatChecker { get; }

    /// <summary>
    /// Gets the downloader service
    /// </summary>
    IDownloader Downloader { get; }

    /// <summary>
    /// Gets the localizer service
    /// </summary>
    ILocalizer Localizer { get; }

    /// <summary>
    /// Gets the overlays service
    /// </summary>
    IOverlays Overlays { get; }

    /// <summary>
    /// Gets the roms service
    /// </summary>
    IRoms Roms { get; }

    /// <summary>
    /// Gets the updater service
    /// </summary>
    IUpdater Updater { get; }

    /// <summary>
    /// Gets the wizard service
    /// </summary>
    IWizard Wizard { get; }
}
