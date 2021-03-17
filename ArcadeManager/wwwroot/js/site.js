const { ipcRenderer } = require("electron");

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
 * Gets the selected OS (Recalbox/Retropie)
 *
 * @param {Function} cb the callback
 */
function getOs(cb) {
    ipcRenderer.once('get-os', (event, os) => {
        console.log(`OS: ${os}`);
        if (cb && typeof cb === 'function') {
            cb(os);
        }
    });

    ipcRenderer.send('get-os');
}

/**
 * Gets a folder path
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function getFolder(current, cb) {
    ipcRenderer.once("select-directory-reply", (sender, path) => {
        cb(path);
    });

    ipcRenderer.send("select-directory", current);
}

/**
 * Create a file
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function newFile(current, cb) {
    ipcRenderer.once("new-file-reply", (sender, path) => {
        cb(path);
    });

    ipcRenderer.send("new-file", current);
}

/**
 * Selects a file
 * 
 * @param {String} current the current path
 * @param {Function} cb the callback
 */
function selectFile(current, cb) {
    ipcRenderer.once("select-file-reply", (sender, path) => {
        cb(path);
    });

    ipcRenderer.send("select-file", current);
}
