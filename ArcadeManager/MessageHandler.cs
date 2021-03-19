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
				Electron.IpcMain.On("open-blank", OpenNewWindow);
				Electron.IpcMain.On("open-folder", OpenFolder);

				// Get AppData
				Electron.IpcMain.On("get-appdata", (args) => { GetAppData(window); });

				// Get/Set OS
				Electron.IpcMain.On("get-os", (args) => { GetOs(window); });
				Electron.IpcMain.On("change-os", ChangeOs);

				// Browse events
				Electron.IpcMain.On("select-directory", async (args) => { await BrowseFolder(args, window); });
				Electron.IpcMain.On("new-file", async (args) => { await NewFile(args, window); });
				Electron.IpcMain.On("select-file", async (args) => { await SelectFile(args, window); });

				// Roms actions
				Electron.IpcMain.On("roms-add", (args) => { RomsAdd(args, window); });
				Electron.IpcMain.On("roms-delete", (args) => { RomsDelete(args, window); });
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
		/// <returns>The object</returns>
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
			Electron.IpcMain.Send(window, "get-os", ArcadeManagerEnvironment.SettingsOs);
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
		private static async void OpenFolder(object folder) {
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
		private static async void OpenNewWindow(object url) {
			if (url != null) {
				Console.WriteLine("open blank link to: " + url.ToString());
				await Electron.Shell.OpenExternalAsync(url.ToString());
			}
			else {
				Console.WriteLine("Unable to open a blank link: no URL provided");
			}
		}

		/// <summary>
		/// Copies roms from a folder to another
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="window">The window reference</param>
		private static void RomsAdd(object args, BrowserWindow window) {
			var data = ConvertArgs<RomsAction>(args);
			MustCancel = false;

			Services.Roms.Add(data, window);
		}

		/// <summary>
        /// Deletes rom from a folder
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="window">The window reference</param>
		private static void RomsDelete(object args, BrowserWindow window)
		{
			var data = ConvertArgs<RomsAction>(args);
			MustCancel = false;

			Services.Roms.Delete(data, window);
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
	}
}
