var selectedOs,
    appData;

$(() => {
    // bind blank pages navigation
    $('a.blank').on('click', (e) => {
        ipcRenderer.send('open-blank', $(e.currentTarget).attr('href'));
        e.preventDefault();
    });

    // bind browse buttons
    $('.browse').on('click', (e) => {
        const btn = $(e.target);
        const input = $('#' + btn.data('input'));
        const existingPath = input.val();

        if (btn.hasClass('folder')) {
            // select a folder
            getFolder(existingPath, (folder) => {
                if (folder && folder.length !== 0) { input.val(folder); }
            });
        }
        else if (btn.hasClass('new')) {
            // create a new file
            newFile(existingPath, (filename) => {
                if (typeof filename !== 'undefined' && filename !== '') {
                    if (!filename.endsWith('.csv')) { filename += '.csv'; }
                    input.val(filename);
                }
            });
        }
        else {
            // select a file
            selectFile(existingPath, (file) => {
                if (file && file.length !== 0) { input.val(file); }
            });
        }
    });
});

/**
 * Sends a message to the IPC handler
 * 
 * @param {String} method the method to invoke
 * @param {any} data the data to send to the back-end
 * @param {Function} cb the callback function
 */
function ipc(method, data, cb) {
    ipcRenderer.once(method + "-reply", (sender, result) => {
        if (cb && typeof cb === 'function') {
            cb(result);
        }
    });

    ipcRenderer.send(method, data);
}

/**
 * Gets the selected OS (Recalbox/Retropie)
 *
 * @param {Function} cb the callback
 */
function getOs(cb) {
    if (selectedOs) {
        cb(selectedOs);
    } else {
        ipc('get-os', null, (os) => {
            selectedOs = os;
            cb(os);
        });
    }
}

/**
 * Sets the selected OS (Recalbox/Retropie)
 * 
 * @param {String} os the selected OS
 */
function setOs(os) {
    selectedOs = os;
    ipcRenderer.send('change-os', os);
}

/**
 * Gets the AppData
 * 
 * @param {any} cb
 */
function getAppData(cb) {
    if (appData) {
        cb(appData);
    } else {
        ipc('get-appdata', null, (data) => {
            appData = data;
            cb(data);
        });
    }
}

/**
 * Gets a folder path
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function getFolder(current, cb) {
    ipc('select-directory', current, cb);
}

/**
 * Create a file
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function newFile(current, cb) {
    ipc('new-file', current, cb);
}

/**
 * Selects a file
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function selectFile(current, cb) {
    ipc('select-file', current, cb);
}

/**
 * Download a file from a Github repository
 * 
 * @param {String} repository the repository
 * @param {String} path the path to the file
 * @param {String} localfile the path to the local file to save
 * @param {Function} cb the callback
 */
function downloadFile(repository, path, localfile, cb) {
    ipc('download-file', { repository, path, localfile }, cb);
}
