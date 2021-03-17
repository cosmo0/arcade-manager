using System;
using ElectronNET.API;

namespace ArcadeManager.Behavior
{
    /// <summary>
    /// Class for messages handling
    /// </summary>
    public static class MessageHandler
    {
        /// <summary>
        /// Initializes the global message handling
        /// </summary>
        public static void InitMessageHandling(BrowserWindow window)
        {
            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On("open-blank", OpenNewWindow);

                Electron.IpcMain.On("get-os", (args) => {
                    Electron.IpcMain.Send(window, "get-os", Settings.Os);
                });
                Electron.IpcMain.On("change-os", ChangeOs);
            }
        }

        /// <summary>
        /// Changes the selected OS
        /// </summary>
        /// <param name="obj"></param>
        private static void ChangeOs(object obj)
        {
            if (obj != null)
            {
                // save the OS in settings
                Settings.Os = obj.ToString();
            }
        }

        /// <summary>
        /// Opens a new browser window to the specified URL
        /// </summary>
        /// <param name="url">The URL to open</param>
        private static async void OpenNewWindow(object url)
        {
            if (url != null)
            {
                Console.WriteLine("open blank link to: " + url.ToString());
                await Electron.Shell.OpenExternalAsync(url.ToString());
            }
            else
            {
                Console.WriteLine("Unable to open a blank link: no URL provided");
            }
        }
    }
}
