using ArcadeManager.Actions;
using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Threading.Tasks;

namespace ArcadeManager {

	/// <summary>
	/// Class for messages handling
	/// </summary>
	public static class MessageHandler {

		/// <summary>
		/// Cancellation token
		/// </summary>
		public static bool MustCancel { get; set; }

		/// <summary>
		/// Initializes the global message handling
		/// </summary>
		public static void InitMessageHandling(BrowserWindow window) {
			if (HybridSupport.IsElectronActive) {
				// Cancel actions
				Electron.IpcMain.On("cancel", (args) => { MustCancel = true; });

				// Navigation
				Electron.IpcMain.On("open-blank", async (args) => { await OpenNewWindow(args as string); });
				Electron.IpcMain.On("open-folder", async (args) => { await OpenFolder(args as string); });

				// Get AppData
				Electron.IpcMain.On("get-appdata", (args) => { GetAppData(window); });

				// Get/Set OS
				Electron.IpcMain.On("get-os", (args) => { GetOs(window); });
				Electron.IpcMain.On("change-os", ChangeOs);

				// Browse events
				Electron.IpcMain.On("select-directory", async (args) => { await BrowseFolder(args, window); });
				Electron.IpcMain.On("new-file", async (args) => { await NewFile(args, window); });
				Electron.IpcMain.On("select-file", async (args) => { await SelectFile(args, window); });

				// filesystem events
				Electron.IpcMain.On("fs-exists", (args) => { FsExists(args, window); });

				// Roms actions
				Electron.IpcMain.On("roms-add", async (args) => { await RomsAdd(args, window); });
				Electron.IpcMain.On("roms-delete", async (args) => { await RomsDelete(args, window); });
				Electron.IpcMain.On("roms-keep", async (args) => { await RomsKeep(args, window); });

				// download actions
				Electron.IpcMain.On("download-getlist", async (args) => { await GithubFilesGetList(args, window); });
				Electron.IpcMain.On("download-file", async (args) => { await DownloadFile(args, window); });

				// CSV actions
				Electron.IpcMain.On("csv-convertdat", async (args) => { await CsvConvertDat(args, window); });
				Electron.IpcMain.On("csv-convertini", async (args) => { await CsvConvertIni(args, window); });
				Electron.IpcMain.On("csv-listfiles", async (args) => { await CsvListFiles(args, window); });
				Electron.IpcMain.On("csv-merge", async (args) => { await CsvMerge(args, window); });
				Electron.IpcMain.On("csv-remove", async (args) => { await CsvRemove(args, window); });
				Electron.IpcMain.On("csv-keep", async (args) => { await CsvKeep(args, window); });

				// overlays action
				Electron.IpcMain.On("overlays-download", async (args) => { await OverlaysDownload(args, window); });
			}
		}

		/// <summary>
		/// Browse for a folder to select
		/// </summary>
		/// <param name="currentPath">The default path</param>
		/// <param name="window">The window reference</param>
		private static async Task BrowseFolder(object currentPath, BrowserWindow window) {
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
		private static void ChangeOs(object obj) {
			if (obj != null) {
				// save the OS in settings
				ArcadeManagerEnvironment.SettingsOs = obj.ToString();
			}
		}

		/// <summary>
		/// Convert arguments to strongly-typed object
		/// </summary>
		/// <typeparam name="T">The type of the object</typeparam>
		/// <param name="args">The arguments</param>
		/// <returns>
		/// The object
		/// </returns>
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
		/// Converts a DAT file.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window.</param>
		private static async Task CsvConvertDat(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.ConvertDat(data.main, data.target, new Progressor(window));
		}

		/// <summary>
		/// Converts a INI file
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="window">The window reference</param>
		private static async Task CsvConvertIni(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.ConvertIni(data.main, data.target, new Progressor(window));
		}

		/// <summary>
		/// Keeps only listed entries in a CSV file
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window.</param>
		private static async Task CsvKeep(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.Keep(data.main, data.secondary, data.target, new Progressor(window));
		}

		/// <summary>
		/// Lists the files in a folder to CSV
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window.</param>
		private static async Task CsvListFiles(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.ListFiles(data.main, data.target, new Progressor(window));
		}

		/// <summary>
		/// Merges two CSV files
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window reference.</param>
		private static async Task CsvMerge(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.Merge(data.main, data.secondary, data.target, new Progressor(window));
		}

		/// <summary>
		/// Removes entries from a CSV file
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window reference.</param>
		private static async Task CsvRemove(object args, BrowserWindow window) {
			var data = ConvertArgs<CsvAction>(args);
			MustCancel = false;

			await Services.Csv.Remove(data.main, data.secondary, data.target, new Progressor(window));
		}

		/// <summary>
		/// Downloads the specified file.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window.</param>
		private static async Task DownloadFile(object args, BrowserWindow window) {
			var data = ConvertArgs<DownloadAction>(args);
			MustCancel = false;

			await Services.Downloader.DownloadFile(data.repository, data.path, data.localfile);

			Electron.IpcMain.Send(window, "download-file-reply", true);
		}

		/// <summary>
		/// Checks if a path exists
		/// </summary>
		/// <param name="window"></param>
		private static void FsExists(object args, BrowserWindow window) {
			var path = args as string;

			Electron.IpcMain.Send(window, "fs-exists-reply", Services.FileSystem.Exists(path));
		}

		/// <summary>
		/// Gets the application data settings
		/// </summary>
		/// <param name="window">The window reference</param>
		private static void GetAppData(BrowserWindow window) {
			Electron.IpcMain.Send(window, "get-appdata-reply", ArcadeManagerEnvironment.AppData);
		}

		/// <summary>
		/// Gets the selected OS
		/// </summary>
		/// <param name="window">The window reference</param>
		private static void GetOs(BrowserWindow window) {
			Electron.IpcMain.Send(window, "get-os-reply", ArcadeManagerEnvironment.SettingsOs);
		}

		/// <summary>
		/// Gets a files list from Github.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="window">The window.</param>
		private static async Task GithubFilesGetList(object args, BrowserWindow window) {
			var data = ConvertArgs<DownloadAction>(args);
			MustCancel = false;

			Electron.IpcMain.Send(window, "csv-getlist-reply", await Services.Downloader.GetList(data));
		}

		/// <summary>
		/// Create a new file
		/// </summary>
		/// <param name="path">The default path</param>
		/// <param name="window">The window reference</param>
		private static async Task NewFile(object path, BrowserWindow window) {
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
		private static async Task OpenFolder(object folder) {
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
		private static async Task OpenNewWindow(object url) {
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
		/// <param name="window">The window reference</param>
		private static async Task OverlaysDownload(object args, BrowserWindow window) {
			var data = ConvertArgs<OverlaysAction>(args);
			MustCancel = false;

			await Services.Overlays.Download(data, new Progressor(window));
		}

		/// <summary>
		/// Copies roms from a folder to another
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="window">The window reference</param>
		private static async Task RomsAdd(object args, BrowserWindow window) {
			var data = ConvertArgs<RomsAction>(args);
			MustCancel = false;

			await Services.Roms.Add(data, new Progressor(window));
		}

		/// <summary>
		/// Deletes rom from a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="window">The window reference</param>
		private static async Task RomsDelete(object args, BrowserWindow window) {
			var data = ConvertArgs<RomsAction>(args);
			MustCancel = false;

			await Services.Roms.Delete(data, new Progressor(window));
		}

		/// <summary>
		/// Keeps roms in a folder
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="window">The window reference</param>
		private static async Task RomsKeep(object args, BrowserWindow window) {
			var data = ConvertArgs<RomsAction>(args);
			MustCancel = false;

			await Services.Roms.Keep(data, new Progressor(window));
		}

		/// <summary>
		/// Selects a file
		/// </summary>
		/// <param name="path">The default path</param>
		/// <param name="window">The window reference</param>
		private static async Task SelectFile(object path, BrowserWindow window) {
			var options = new OpenDialogOptions {
				Properties = new OpenDialogProperty[] { OpenDialogProperty.openFile },
				DefaultPath = path as string ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				Filters = new FileFilter[] { new FileFilter { Extensions = new string[] { ".csv" } } }
			};

			string[] files = await Electron.Dialog.ShowOpenDialogAsync(window, options);
			Electron.IpcMain.Send(window, "select-file-reply", files);
		}

		/// <summary>
		/// Handles progression messages
		/// </summary>
		public class Progressor {
			private readonly BrowserWindow window;

			/// <summary>
			/// Initializes a new instance of the <see cref="Progressor"/> class.
			/// </summary>
			/// <param name="window">The window reference.</param>
			public Progressor(BrowserWindow window) {
				this.window = window;
			}

			/// <summary>
			/// Sends a "done" progress message
			/// </summary>
			/// <param name="label">The label.</param>
			/// <param name="folder">The result folder, if any.</param>
			public void Done(string label, string folder) {
				// display result
				if (MessageHandler.MustCancel) {
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
		}
	}
}
