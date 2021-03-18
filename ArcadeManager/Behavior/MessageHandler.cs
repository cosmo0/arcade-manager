using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Threading.Tasks;

namespace ArcadeManager.Behavior {

	/// <summary>
	/// Class for messages handling
	/// </summary>
	public static class MessageHandler {

		/// <summary>
		/// Initializes the global message handling
		/// </summary>
		public static void InitMessageHandling(BrowserWindow window) {
			if (HybridSupport.IsElectronActive) {
				Electron.IpcMain.On("open-blank", OpenNewWindow);

				// Get/Set OS
				Electron.IpcMain.On("get-os", (args) => { GetOs(window); });
				Electron.IpcMain.On("change-os", ChangeOs);

				// Browse events
				Electron.IpcMain.On("select-directory", async (args) => { await BrowseFolder(args, window); });
				Electron.IpcMain.On("new-file", async (args) => { await NewFile(args, window); });
				Electron.IpcMain.On("select-file", async (args) => { await SelectFile(args, window); });
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
				Settings.Os = obj.ToString();
			}
		}

		/// <summary>
		/// Gets the selected OS
		/// </summary>
		/// <param name="window">The window reference</param>
		private static void GetOs(BrowserWindow window) {
			Electron.IpcMain.Send(window, "get-os", Settings.Os);
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
