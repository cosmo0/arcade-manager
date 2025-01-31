using ArcadeManager.Actions;
using ArcadeManager.Infrastructure;
using ArcadeManager.Services;
using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Threading.Tasks;

namespace ArcadeManager;

/// <summary>
/// Class for messages handling
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageHandler"/> class.
/// </remarks>
/// <param name="csvService">The CSV service.</param>
/// <param name="downloaderService">The downloader service.</param>
/// <param name="overlaysService">The overlays service.</param>
/// <param name="romsService">The roms service.</param>
/// <param name="updaterService">The updater service.</param>
/// <param name="fs">The file system service</param>
/// <param name="environment">The environment accessor</param>
public partial class ElectronMessageHandler(ICsv csvService, IDownloader downloaderService, IOverlays overlaysService, IRoms romsService, IUpdater updaterService, IFileSystem fs, IEnvironment environment) : IElectronMessageHandler {
    private BrowserWindow window;

    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    public bool MustCancel { get; set; }

    /// <summary>
    /// Sends an "init" progress message
    /// </summary>
    /// <param name="label">The label.</param>
    public void Init(string label) {
        MustCancel = false;
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

        MustCancel = false;
    }

    /// <summary>
    /// Sends an "error" progress message
    /// </summary>
    /// <param name="ex">The exception.</param>
    public void Error(Exception ex) {
        Electron.IpcMain.Send(window, "progress", new Progress { label = $"An error has occurred: {ex.Message}", end = true });
        
        MustCancel = false;
    }

    /// <summary>
    /// Initializes the message handling for the specified window
    /// </summary>
    /// <param name="window">The window.</param>
    public async Task Handle(BrowserWindow window) {
        this.window = window;

        if (HybridSupport.IsElectronActive) {
            // Cancel actions
            await Electron.IpcMain.On("cancel", (args) => { MustCancel = true; });

            // Navigation
            await Electron.IpcMain.On("open-blank", async (args) => await OpenNewWindow(args));
            await Electron.IpcMain.On("open-folder", async (args) => await OpenFolder(args));

            // Get AppData
            await Electron.IpcMain.On("get-appdata", (args) => GetAppData());

            // Get/Set OS
            await Electron.IpcMain.On("get-os", (args) => GetOs());
            await Electron.IpcMain.On("change-os", ChangeOs);

            // Browse events
            await Electron.IpcMain.On("select-directory", async (args) => await BrowseFolder(args));
            await Electron.IpcMain.On("new-file", async (args) => await NewFile(args));
            await Electron.IpcMain.On("select-file", async (args) => await SelectFile(args));

            // filesystem events
            await Electron.IpcMain.On("fs-exists", FsExists);
            await Electron.IpcMain.On("copy-file", CopyFile);
            await Electron.IpcMain.On("local-getlist", LocalFilesGetList);

            // Roms actions
            await Electron.IpcMain.On("roms-check", async (args) => await RomsCheck(args));
            await Electron.IpcMain.On("roms-add", async (args) => await RomsAdd(args));
            await Electron.IpcMain.On("roms-addfromwizard", async (args) => await RomsAddFromWizard(args));
            await Electron.IpcMain.On("roms-delete", async (args) => await RomsDelete(args));
            await Electron.IpcMain.On("roms-keep", async (args) => await RomsKeep(args));
            await Electron.IpcMain.On("roms-checkdat", async (args) => await RomsCheckDat(args));

            // download actions
            await Electron.IpcMain.On("download-getlist", async (args) => await GithubFilesGetList(args));
            await Electron.IpcMain.On("download-file", async (args) => await DownloadFile(args));

            // CSV actions
            await Electron.IpcMain.On("csv-convertdat", async (args) => await CsvConvertDat(args));
            await Electron.IpcMain.On("csv-convertini", async (args) => await CsvConvertIni(args));
            await Electron.IpcMain.On("csv-listfiles", async (args) => await CsvListFiles(args));
            await Electron.IpcMain.On("csv-merge", async (args) => await CsvMerge(args));
            await Electron.IpcMain.On("csv-remove", async (args) => await CsvRemove(args));
            await Electron.IpcMain.On("csv-keep", async (args) => await CsvKeep(args));

            // overlays action
            await Electron.IpcMain.On("overlays-download", async (args) => await OverlaysDownload(args));

            // check for update
            await Electron.IpcMain.On("update-check", async (_) => await UpdateCheck());
            await Electron.IpcMain.On("update-ignore", UpdateIgnore);
        }
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
    private async Task BrowseFolder(object currentPath) {
        var options = new OpenDialogOptions {
            Properties = [ OpenDialogProperty.openDirectory ],
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
    private async Task CsvConvertDat(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ConvertDat(data.main, data.target, this);
    }

    /// <summary>
    /// Converts a INI file
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task CsvConvertIni(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ConvertIni(data.main, data.target, this);
    }

    /// <summary>
    /// Keeps only listed entries in a CSV file
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task CsvKeep(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Keep(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Lists the files in a folder to CSV
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task CsvListFiles(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.ListFiles(data.main, data.target, this);
    }

    /// <summary>
    /// Merges two CSV files
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task CsvMerge(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Merge(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Removes entries from a CSV file
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task CsvRemove(object args) {
        var data = ConvertArgs<CsvAction>(args);
        MustCancel = false;

        await csvService.Remove(data.main, data.secondary, data.target, this);
    }

    /// <summary>
    /// Downloads the specified file.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task DownloadFile(object args) {
        var data = ConvertArgs<DownloadAction>(args);
        MustCancel = false;

        await downloaderService.DownloadFile(data.repository, data.path, data.localfile);

        Electron.IpcMain.Send(window, "download-file-reply", true);
    }

    /// <summary>
    /// Checks if a path exists
    /// </summary>
    private void FsExists(object args) {
        var path = args as string;

        Electron.IpcMain.Send(window, "fs-exists-reply", fs.Exists(path));
    }

    /// <summary>
    /// Gets the application data settings
    /// </summary>
    private void GetAppData() {
        // serialize here to handle a transmission problem
        Electron.IpcMain.Send(window, "get-appdata-reply", Serializer.Serialize(ArcadeManagerEnvironment.AppData));
    }

    /// <summary>
    /// Gets the selected OS
    /// </summary>
    private void GetOs() {
        // serialize here to handle a transmission problem
        Electron.IpcMain.Send(window, "get-os-reply", Serializer.Serialize(ArcadeManagerEnvironment.SettingsOs));
    }

    /// <summary>
    /// Gets a files list from Github.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task GithubFilesGetList(object args) {
        var data = ConvertArgs<DownloadAction>(args);
        MustCancel = false;

        Electron.IpcMain.Send(window, "download-getlist-reply", await downloaderService.GetList(data));
    }

    /// <summary>
    /// Gets a files list from a local folder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private void LocalFilesGetList(object args) {
        var data = ConvertArgs<DownloadAction>(args);
        MustCancel = false;

        Electron.IpcMain.Send(window, "local-getlist-reply", downloaderService.GetLocalList(data));
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="path">The default path</param>
    private async Task NewFile(object path) {
        var options = new SaveDialogOptions {
            DefaultPath = path as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        };

        string file = await Electron.Dialog.ShowSaveDialogAsync(window, options);
        Electron.IpcMain.Send(window, "new-file-reply", file);
    }

    /// <summary>
    /// Copies a local file
    /// </summary>
    /// <param name="args">The arguments</param>
    private void CopyFile(object args) {
        var data = ConvertArgs<FileAction>(args);
        
        fs.FileCopy(data.source, data.target, data.overwrite);

        Electron.IpcMain.Send(window, "copy-file-reply", true);
    }

    /// <summary>
    /// Opens the explorer to the specified folder
    /// </summary>
    /// <param name="folder">The folder to open</param>
    private async Task OpenFolder(object folder) {
        if (folder != null && fs.Exists(folder.ToString())) {
            var path = folder.ToString();
            if (!fs.IsDirectory(path)) {
                path = fs.DirectoryName(path);
            }

            await Electron.Shell.OpenPathAsync(path);
        }
        else {
            Console.WriteLine("Unable to open the folder");
        }
    }

    /// <summary>
    /// Opens a new browser window to the specified URL
    /// </summary>
    /// <param name="url">The URL to open</param>
    private async Task OpenNewWindow(object url) {
        if (url != null) {
            Console.WriteLine("open blank link to: " + url.ToString());
            await Electron.Shell.OpenExternalAsync(url.ToString());
        }
        else {
            Console.WriteLine("Unable to open a blank link: no URL provided");
        }
    }

    /// <summary>
    /// Downloads overlays
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task OverlaysDownload(object args) {
        var data = ConvertArgs<OverlaysAction>(args);
        MustCancel = false;

        await overlaysService.Download(data, this);
    }

    /// <summary>
    /// Checks the existence of roms
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task RomsCheck(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        string[] missing = await romsService.Check(data, this);
        // why is this method the ONLY one where the result is not wrapped into an array?
        Electron.IpcMain.Send(window, "roms-check-reply", [missing]);
    }

    /// <summary>
    /// Copies roms from a folder to another
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task RomsAdd(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Add(data, this);
    }

    /// <summary>
    /// Copies roms from a folder to another, from the wizard
    /// </summary>
    /// <param name="args">The arguments.</param>
    private async Task RomsAddFromWizard(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.AddFromWizard(data, this);
    }

    /// <summary>
    /// Deletes rom from a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task RomsDelete(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Delete(data, this);
    }

    /// <summary>
    /// Keeps roms in a folder
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task RomsKeep(object args) {
        var data = ConvertArgs<RomsAction>(args);
        MustCancel = false;

        await romsService.Keep(data, this);
    }

    /// <summary>
    /// Checks a romset against a DAT file
    /// </summary>
    /// <param name="args">The arguments</param>
    private async Task RomsCheckDat(object args) {
        var data = ConvertArgs<RomsActionCheckDat>(args);
        MustCancel = false;

        await romsService.CheckDat(data, this);
    }

    /// <summary>
    /// Selects a file
    /// </summary>
    /// <param name="path">The default path</param>
    private async Task SelectFile(object path) {
        var options = new OpenDialogOptions {
            Properties = [OpenDialogProperty.openFile],
            DefaultPath = path as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            Filters = [new FileFilter { Extensions = [".csv"] }]
        };

        string[] files = await Electron.Dialog.ShowOpenDialogAsync(window, options);
        Electron.IpcMain.Send(window, "select-file-reply", files);
    }

    /// <summary>
    /// Checks if an update is available
    /// </summary>
    private async Task UpdateCheck() {
        var current = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
        environment.SettingsIgnoredVersionAdd(args as string);
    }
}