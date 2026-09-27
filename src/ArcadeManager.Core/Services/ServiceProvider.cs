using System;
using ArcadeManager.Core.Services.Interfaces;

namespace ArcadeManager.Core.Services;

public class ServiceProvider(
    ICsv csv,
    IDatChecker datChecker,
    IDownloader downloader,
    ILocalizer localizer,
    IOverlays overlays,
    IRoms roms) : Interfaces.IServiceProvider
{
    /// <summary>
    /// Gets the CSV service
    /// </summary>
    public ICsv Csv => csv;

    /// <summary>
    /// Gets the DAT checker service
    /// </summary>
    public IDatChecker DatChecker => datChecker;

    /// <summary>
    /// Gets the downloader service
    /// </summary>
    public IDownloader Downloader => downloader;

    /// <summary>
    /// Gets the localizer service
    /// </summary>
    public ILocalizer Localizer => localizer;

    /// <summary>
    /// Gets the overlay service
    /// </summary>
    public IOverlays Overlays => overlays;

    /// <summary>
    /// Gets the roms service
    /// </summary>
    public IRoms Roms => roms;
}