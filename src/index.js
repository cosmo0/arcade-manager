const electron = require('electron');
const app = electron.app;
const BrowserWindow = electron.BrowserWindow;
const path = require('path');
const url = require('url');

// global reference to window so it's not closed
// when garbage collected
let mainWindow;

/**
 * Creates the main window
 */
function createWindow () {
    mainWindow = new BrowserWindow({
        width: 1024,
        height: 768
    });

    mainWindow.loadURL(url.format({
        pathname: path.join(__dirname, 'index.html'),
        protocol: 'file:',
        slashes: true
    }));

    // Open the DevTools.
    // mainWindow.webContents.openDevTools()

    // Emitted when the window is closed.
    mainWindow.on('closed', function () {
        mainWindow = null;
    });
}

// create the main window when the app is ready
app.on('ready', createWindow);

// quit when all windows are closed.
app.on('window-all-closed', function () {
    // Mac: quit app when last window is closed
    if (process.platform !== 'darwin') { app.quit(); }
});

// open window when app is activated
app.on('activate', function () {
    // Mac: re-create main window when icon is clicked
    // but no windows exist (should not be possible, but hey)
    if (mainWindow === null) { createWindow(); }
});
