const { ipcRenderer } = require("electron");

$(() => {
    // bind blank pages navigation
    $('a.blank').on('click', (e) => {
        ipcRenderer.send('open-blank', $(e.currentTarget).attr('href'));
        e.preventDefault();
    });
});

/**
 * Gets the selected OS (Recalbox/Retropie)
 */
function getOs(cb) {
    ipcRenderer.on('get-os', (event, os) => {
        console.log(`OS: ${os}`);
        if (cb && typeof cb === 'function') {
            cb(os);
        }
    });

    ipcRenderer.send('get-os');
}