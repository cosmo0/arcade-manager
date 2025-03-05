using System;
using ArcadeManager.Core.Services.Interfaces;

namespace ArcadeManager.Core.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "S107:Constructor has 8 parameters, which is greater than the 7 authorized.", Justification = "The only place where this should be")]
public class ServiceProvider(
    ICsv csv,
    IDatChecker datChecker,
    IDownloader downloader,
    ILocalizer localizer,
    IOverlays overlays,
    IRoms roms,
    IUpdater updater,
    IWizard wizard) : Interfaces.IServiceProvider
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

    /// <summary>
    /// Gets the updater service
    /// </summary>
    public IUpdater Updater => updater;

    /// <summary>
    /// Gets the wizard service
    /// </summary>
    public IWizard Wizard => wizard;
}
