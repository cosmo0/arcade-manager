const { ipcRenderer } = require("electron");

let selectedOs,
    appData;

$(() => {
    // bind blank pages navigation
    $(document).on('click', 'a.blank', (e) => {
        ipc('open-blank', $(e.currentTarget).attr('href'));
        e.preventDefault();
    });

    // bind browse buttons
    $(document).on('click', '.browse', (e) => {
        const btn = $(e.target);
        const input = $('#' + btn.data('input'));
        const existingPath = input.val();

        // disable button to prevent multiple clicks when the explorer takes some time to respond
        btn.prop('disabled', true).addClass('disabled');

        if (btn.hasClass('folder')) {
            // select a folder
            getFolder(existingPath, (folder) => {
                if (folder && folder.length !== 0) { input.val(folder); }

                btn.prop('disabled', false).removeClass('disabled');
            });
        }
        else if (btn.hasClass('new')) {
            // create a new file
            newFile(existingPath, (filename) => {
                if (typeof filename !== 'undefined' && filename !== '') {
                    if (!filename.endsWith('.csv')) { filename += '.csv'; }
                    input.val(filename);
                }

                btn.prop('disabled', false).removeClass('disabled');
            });
        }
        else {
            // select a file
            selectFile(existingPath, (file) => {
                if (file && file.length !== 0) { input.val(file); }

                btn.prop('disabled', false).removeClass('disabled');
            });
        }
    });

    // files list modal
    $('#filesListCopy').on('click', () => {
        navigator.clipboard.writeText($('#filesList').text());
        $('#filesListCopyMessage').removeClass('d-none').fadeOut(1000);
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
    if (cb && typeof cb === 'function') {
        ipcRenderer.once(method + "-reply", (_, result) => {
            console.log('Get results back from ' + method + '-reply');
            if (result && result instanceof Array && result.length > 0) {
                result = result[0];
            }

            cb(result);
        });
    }

    console.log('sending ' + method + ' with data ' + JSON.stringify(data));
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
            if (os) { os = JSON.parse(os); }

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
    ipc('change-os', os);
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
            if (data) { data = JSON.parse(data); }

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
 * Copies a file
 * 
 * @param {String} source the source file to copy
 * @param {String} target the target file to copy to
 * @param {Boolean} overwrite whether to overwrite the target file
 * @param {Function} cb the callback
 */
function copyFile(source, target, overwrite, cb) {
    ipc('copy-file', { source, target, overwrite }, cb);
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

/**
 * Gets a remote list of files
 * 
 * @param {String} repository the repository
 * @param {String} details the file details
 * @param {String} folder the folder
 */
function getRemoteList(repository, details, folder, cb) {
    ipc('download-getlist', { repository, details, folder }, cb);
}

/**
 * Gets a local list of files
 * 
 * @param {String} folder the folder
 */
function getLocalList(folder, cb) {
    ipc('local-getlist', { folder }, cb);
}

/**
 * Checks that an update is available
 * 
 * @param {Function} cb the callback
 */
function checkUpdate(cb) {
    ipc('update-check', null, cb);
}

/**
 * Checks that a list of games exist on the target folder
 * 
 * @param {String} file the file containing the list of games
 * @param {String} folder the folder containing the files to check
 * @param {Function} cb the callback, if nothing is missing or the user continues
 */
function checkRoms(file, folder, cb) {
    ipc('roms-check', { main: file, romset: folder }, (result) => {
        if (!result || result.length === 0) {
            // no missing files
            cb();
        } else {
            // missing files: make the user confirm
            $('#filesListModal #filesList').html(result.join('<br>\n'));
            $('#filesListModal').modal('show');

            $('#filesListModal #filesListContinue').off('click').one('click', () => {
                $('#filesListModal').modal('hide');
                cb();
            });
        }
    });
}