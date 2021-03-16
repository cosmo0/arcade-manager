$(() => {
    const { ipcRenderer } = require("electron");

    // new pages navigation
    $('a.blank').on('click', (e) => {
        ipcRenderer.send('open-blank', $(e.currentTarget).attr('href'));
        e.preventDefault();
    });
});