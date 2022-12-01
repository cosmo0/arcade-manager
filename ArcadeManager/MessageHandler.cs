using ArcadeManager.Actions;
using ArcadeManager.Services;
using ElectronNET.API;
using ElectronNET.API.Entities;
using System;

namespace ArcadeManager;

/// <summary>
/// Class for messages handling
/// </summary>
public partial class MessageHandler : IMessageHandler {
    private readonly ICsv csvService;
    private readonly IDownloader downloaderService;
    private readonly IOverlays overlaysService;
    private readonly IRoms romsService;
    private readonly IUpdater updaterService;
    private BrowserWindow window;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="csvService">The CSV service.</param>
    /// <param name="downloaderService">The downloader service.</param>
    /// <param name="overlaysService">The overlays service.</param>
    /// <param name="romsService">The roms service.</param>
    /// <param name="updaterService">The updater service.</param>
    public MessageHandler(ICsv csvService, IDownloader downloaderService, IOverlays overlaysService, IRoms romsService, IUpdater updaterService) {
        this.csvService = csvService;
        this.downloaderService = downloaderService;
        this.overlaysService = overlaysService;
        this.romsService = romsService;
        this.updaterService = updaterService;
    }

    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    public bool MustCancel { get; set; }

    /// <summary>
    /// Sends a "done" progress message
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="folder">The result folder, if any.</param>
    public void Done(string label, string folder) {
        // display result
        if (MustCancel) {
            Electron.IpcMain.Send(window, "progress", new Progress { label = $"Operation cancelled! - {label}", end = true, cancelled = true });
        }
        else {
            Electron.IpcMain.Send(window, "progress", new Progress { label = label, end = true, folder = folder });
        }
    }

    /// <summary>
    /// Sends an "error" progress message
    /// </summary>
    /// <param name="ex">The exception.</param>
    public void Error(Exception ex) {
        Electron.IpcMain.Send(window, "progress", new Progress { label = $"An error has occurred: {ex.Message}", end = true });
    }

    /// <summary>
    /// Initializes the message handling for the specified window
    /// </summary>
    /// <param name="window">The window.</param>
    public void Handle(BrowserWindow window) {
        this.window = window;

        if (HybridSupport.IsElectronActive) {
            // Cancel actions
            Electron.IpcMain.On("cancel", (args) => { MustCancel = true; });

            // Navigation
            Electron.IpcMain.On("open-blank", OpenNewWindow);
            Electron.IpcMain.On("open-popup", OpenPopup);
            Electron.IpcMain.On("open-folder", OpenFolder);

            // Get AppData
            Electron.IpcMain.On("get-appdata", GetAppData);

            // Get/Set OS
            Electron.IpcMain.On("get-os", GetOs);
            Electron.IpcMain.On("change-os", ChangeOs);

            // Browse events
            Electron.IpcMain.On("select-directory", BrowseFolder);
            Electron.IpcMain.On("new-file", NewFile);
            Electron.IpcMain.On("select-file", SelectFile);

            // filesystem events
            Electron.IpcMain.On("fs-exists", FsExists);

            // Roms actions
            Electron.IpcMain.On("roms-add", RomsAdd);
            Electron.IpcMain.On("roms-addfromwizard", RomsAddFromWizard);
            Electron.IpcMain.On("roms-delete", RomsDelete);
            Electron.IpcMain.On("roms-keep", RomsKeep);

            // download actions
            Electron.IpcMain.On("download-getlist", GithubFilesGetList);
            Electron.IpcMain.On("download-file", DownloadFile);

            // CSV actions
            Electron.IpcMain.On("csv-convertdat", CsvConvertDat);
            Electron.IpcMain.On("csv-convertini", CsvConvertIni);
            Electron.IpcMain.On("csv-listfiles", CsvListFiles);
            Electron.IpcMain.On("csv-merge", CsvMerge);
            Electron.IpcMain.On("csv-remove", CsvRemove);
            Electron.IpcMain.On("csv-keep", CsvKeep);

            // overlays action
            Electron.IpcMain.On("overlays-download", OverlaysDownload);

            // check for update
            Electron.IpcMain.On("update-check", UpdateCheck);
            Electron.IpcMain.On("update-ignore", UpdateIgnore);
        }
    }

    /// <summary>
    /// Sends an "init" progress message
    /// </summary>
    /// <param name="label">The label.</param>
    public void Init(string label) {
        Electron.IpcMain.Send(window, "progress", new Progress { label = label, init = true, canCancel = true });
    }

    /// <summary>
    /// Sends a progression message
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="current">The current item number.</param>
    public void Progress(string label, int total, int current) {
        Electron.IpcMain.Send(window, "progress", new Progress { label = label, total = total, current = current });
    }

    /// <summary>
    /// Convert arguments to strongly-typed object
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <param name="args">The arguments</param>
    /// <returns>The object</returns>
    /// <exception cref="ArgumentException">Unable to convert arguments to JObject</exception>
    private static T ConvertArgs<T>(object args) {
        if (args == null) {
            return default;
        }

        if (args.GetType() != typeof(Newtonsoft.Json.Linq.JObject)) {
            throw new ArgumentException("Unable to convert arguments to JObject");
        }

        return ((Newtonsoft.Json.Linq.JObject)args).ToObject<T>();
    }

    /// <summary>
    /// Browse for a folder to select
    /// </summary>
    /// <param name="currentPath">The default path</param>
    /// <param name="window">The window reference</param>
    private async void BrowseFolder(object currentPath) {
        var options = new OpenDialogOptions {
            Properties = new OpenDialogProperty[] { OpenDialogProperty.openDirectory },
            DefaultPath = currentPath as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        };

        string[] files = await Electron.Dialog.ShowOpenDialogAsync(window, options);
        Electron.IpcMain.Send(window, "select-directory-reply", files);
    }

    /// <summary>
    /// Changes the selected OS
    /// </summary>
    /// <param name="obj">The value</param>
    private void ChangeOs(object obj) {
        if (obj != null) {
            // save the OS in settings
            ArcadeManagerEnvironment.SettingsOs = obj.ToString();
        }
    }

    /// <summary>
    /// Converts a DAT file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window.</param>
    private async void CsvConvertDat(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ConvertDat(data.main, data.target, this);
    }

    /// <summary>
    /// Converts a INI file
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="window">The window reference</param>
    private async void CsvConvertIni(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ConvertIni(data.main, data.target, this);
    }

    /// <summary>
    /// Keeps only listed entries in a CSV file
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window.</param>
    private async void CsvKeep(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Keep(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Lists the files in a folder to CSV
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window.</param>
    private async void CsvListFiles(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ListFiles(data.main, data.target, this);
    }

    /// <summary>
    /// Merges two CSV files
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window reference.</param>
    private async void CsvMerge(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Merge(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Removes entries from a CSV file
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window reference.</param>
    private async void CsvRemove(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Remove(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Downloads the specified file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window.</param>
    private async void DownloadFile(object args) {
        var data = ConvertArgs<DownloadAction>(args);
        MustCancel = false;

        await downloaderService.DownloadFile(data.repository, data.path, data.localfile);

        Electron.IpcMain.Send(window, "download-file-reply", true);
    }

    /// <summary>
    /// Checks if a path exists
    /// </summary>
    /// <param name="window"></param>
    private void FsExists(object args) {
        var path = args as string;

        Electron.IpcMain.Send(window, "fs-exists-reply", FileSystem.Exists(path));
    }

    /// <summary>
    /// Gets the application data settings
    /// </summary>
    /// <param name="window">The window reference</param>
    private void GetAppData(object _) {
        Electron.IpcMain.Send(window, "get-appdata-reply", ArcadeManagerEnvironment.AppData);
    }

    /// <summary>
    /// Gets the selected OS
    /// </summary>
    /// <param name="window">The window reference</param>
    private void GetOs(object _) {
        Electron.IpcMain.Send(window, "get-os-reply", ArcadeManagerEnvironment.SettingsOs);
    }

    /// <summary>
    /// Gets a files list from Github.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="window">The window.</param>
    private async void GithubFilesGetList(object args) {
        var data = ConvertArgs<DownloadAction>(args);
        MustCancel = false;

        Electron.IpcMain.Send(window, "download-getlist-reply", await downloaderService.GetList(data));
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="path">The default path</param>
    /// <param name="window">The window reference</param>
    private async void NewFile(object path) {
        var options = new SaveDialogOptions {
            DefaultPath = path as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        };

        string file = await Electron.Dialog.ShowSaveDialogAsync(window, options);
        Electron.IpcMain.Send(window, "new-file-reply", file);
    }

    /// <summary>
    /// Opens the explorer to the specified folder
    /// </summary>
    /// <param name="folder">The folder to open</param>
    private async void OpenFolder(object folder) {
        if (folder != null) {
            await Electron.Shell.OpenPathAsync(folder.ToString());
        }
        else {
            Console.WriteLine("Unable to open the folder");
        }
    }

    /// <summary>
    /// Opens a new browser window to the specified URL
    /// </summary>
    /// <param name="url">The URL to open</param>
    private async void OpenNewWindow(object url) {
        if (url != null) {
            Console.WriteLine("open blank link to: " + url.ToString());
            await Electron.Shell.OpenExternalAsync(url.ToString());
        }
        else {
            Console.WriteLine("Unable to open a blank link: no URL provided");
        }
    }

    /// <summary>
    /// Opens a popup to the specified URL
    /// </summary>
    /// <param name="url">The URL.</param>
    private async void OpenPopup(object url) {
        if (url != null) {
            Console.WriteLine("open popup link to: " + url.ToString());
            var options = new BrowserWindowOptions {
                Width = 1024,
                Height = 768
            };
            await Electron.WindowManager.CreateWindowAsync(options, url.ToString());
        }
        else {
            Console.WriteLine("Unable to open a blank link: no URL provided");
        }
    }

    /// <summary>
    /// Downloads overlays
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="window">The window reference</param>
    private async void OverlaysDownload(object args) {
        var data = ConvertArgs<OverlaysAction>(args);
        MustCancel = false;

        await overlaysService.Download(data, this);
    }

    /// <summary>
    /// Copies roms from a folder to another
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="window">The window reference</param>
    private async void RomsAdd(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Add(data, this);
    }

    /// <summary>
    /// Copies roms from a folder to another, from the wizard
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async void RomsAddFromWizard(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.AddFromWizard(data, this);
    }

    /// <summary>
    /// Deletes rom from a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="window">The window reference</param>
    private async void RomsDelete(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Delete(data, this);
    }

    /// <summary>
    /// Keeps roms in a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <param name="window">The window reference</param>
    private async void RomsKeep(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Keep(data, this);
    }

    /// <summary>
    /// Selects a file
    /// </summary>
    /// <param name="path">The default path</param>
    /// <param name="window">The window reference</param>
    private async void SelectFile(object path) {
        var options = new OpenDialogOptions {
            Properties = new OpenDialogProperty[] { OpenDialogProperty.openFile },
            DefaultPath = path as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            Filters = new FileFilter[] { new FileFilter { Extensions = new string[] { ".csv" } } }
        };

        string[] files = await Electron.Dialog.ShowOpenDialogAsync(window, options);
        Electron.IpcMain.Send(window, "select-file-reply", files);
    }

    /// <summary>
    /// Checks if an update is available
    /// </summary>
    private async void UpdateCheck(object _) {
        var current = await ArcadeManagerEnvironment.GetVersion();
        var update = await updaterService.CheckUpdate(current);
        if (update != null) {
            update.Current = current;
        }

        Electron.IpcMain.Send(window, "update-check-reply");
    }

    /// <summary>
    /// Ignores the specified version for future updates
    /// </summary>
    /// <param name="args">The arguments.</param>
    private void UpdateIgnore(object args) {
        ArcadeManagerEnvironment.SettingsIgnoredVersionAdd(args as string);
    }
}