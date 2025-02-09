let processed = [];
let processedFormat = 'list';

$(() => {
    // bind progress events
    ipcRenderer.on('progress', (origin, target, data) => {
        data = !data ? {} : data[0];

        if (data.init) {
            console.log('Init: ' + data.label);
            progressInit(data.label, data.canCancel);
        }
        else if (data.end) {
            console.log('Done: ' + data.label);
            progressDone(data.label, data.folder);
        }
        else {
            console.log('Progress: ' + data.total);
            progress(data.total, data.current, data.label);
        }
    });

    // bind display of files processed
    ipcRenderer.on('progress-processed', (origin, data) => {
        data = !data ? {} : data[0];
        
        progressProcessed(data);
    });

    // bind display of files fixed
    ipcRenderer.on('progress-processed', (origin, data) => {
        data = !data ? {} : data[0];
        
        progressFixed(data);
    });

    // bind errors
    window.onerror = (msg, url, line, col, error) => {
        let extra = !col ? '' : ' ; column: ' + col;
        extra += !error ? '' : '<br><br>error: ' + error;

        progressLog("Error: " + msg + "<br><br>File: " + url + "<br>line: " + line + extra, true);

        return true; // error handled
    };

    // bind cancel
    $('#progress .stop button').on('click', (e) => {
        // disable button
        let btn = $(e.currentTarget);
        btn.addClass('disabled btn-outline-secondary')
            .removeClass('btn-outline-danger')
            .prop('disabled', true);

        // send cancel message
        ipc('cancel');
    });

    // bind result list copy
    $('#filesListCopyLog').off('click').on('click', () => {
        navigator.clipboard.writeText($('#processedList').text());
        $('#filesListCopyLogMessage').removeClass('d-none').show().fadeOut(1000);
    });

    // bind "only errors" checkbox change
    $('#processedOnlyErrors').off('change').on('change', () => {
        refreshProcessed();
    });

    // bind processed format change
    $('input:radio[name=processedFormat]').on('change', (e) => {
        processedFormat = e.target.value;
        refreshProcessed();
    });
});

/**
 * Shows the progression modal
 *
 * @param {String} title The modal title
 * @param {Boolean} canCancel Whether the user can cancel the operation
 */
function progressInit(title, canCancel) {
    if (typeof canCancel === 'undefined') { canCancel = false; }
    let p = $('#progress');

    // display modal
    p.modal({ 'backdrop': 'static', 'keyboard': false });

    // hides/displays some items
    p.find('.modal-footer, .msg, .log, .processed').addClass('d-none');
    p.find('.progress, .details').removeClass('d-none');

    // display or hide cancel button
    if (canCancel) {
        p.find('.stop').removeClass('d-none');
    } else {
        p.find('.stop').addClass('d-none');
    }

    // reset texts
    p.find('.modal-title').text(title);
    p.find('.details, .log, #processedList').text('');

    // reset styles
    p.find('.progress .progress-bar').width('0%');
    p.find('.log').removeClass('alert-danger').addClass('alert-info');
    $('#progress .stop button')
        .prop('disabled', false)
        .removeClass('disabled btn-outline-secondary')
        .addClass('btn-outline-danger');
    
    // reset "display only errors"
    $('#processedOnlyErrors').prop('checked', true).prop('disabled', true);
    $('#filesListCopyLog').prop('disabled', true);

    // reset list of processed games
    processed = [];
}

/**
 * Advances the progression modal
 *
 * @param {Number} total The total number of items
 * @param {Number} current The current item number
 * @param {String} details The details to display
 */
function progress(total, current, item) {
    let p = $('#progress');

    // set texts
    p.find('.details').text('Processing ' + item);

    // calculate current percentage
    let percent = current != 0 ? current / total * 100 : 0;
    p.find('.progress .progress-bar').width(percent + '%');
}

/**
 * Logs some info in the progress dialog
 * 
 * @param {String} msg the message to log
 * @param {Boolean} isError whether the message is an error message
 */
function progressLog(msg, isError) {
    if (typeof isError === 'undefined') { isError = false; }

    let p = $('#progress');
    let log = p.find('.log');
    if (log.hasClass('d-none')) { log.removeClass('d-none'); }
    log.append(msg + '<br>');

    // display close button if it's an error
    if (isError) {
        // log is error
        p.find('.log').removeClass('alert-info').addClass('alert-danger');
        // display/hide items
        p.find('.modal-footer').removeClass('d-none');
        p.find('.progress, .stop').addClass('d-none');
    }
}

/**
 * Refreshes the processed games list
 */
function refreshProcessed() {
    const onlyErrors = $('#processedOnlyErrors').is(':checked');
    $('#processedList').empty();

    if (processedFormat == 'csv') {
        $('#processedList').css('white-space', 'pre').append('game;file;status\n');
    } else {
        $('#processedList').css('white-space', 'normal');
    }

    for (let game of processed) {
        displayGame(game, onlyErrors);
    }
}

/**
 * Displays a processed game
 * 
 * @param {String} raw the raw string of the processed game
 */
function progressProcessed(raw) {
    $('#progress .processed').removeClass('d-none');

    const data = JSON.parse(raw);

    // add to the list if it doesn't exist
    if (!processed.some(g => g.name === data.name)) {
        processed.push(data);

        displayGame(data, $('#processedOnlyErrors').is(':checked'));
    }
}

/**
 * Updates a fixed game
 * 
 * @param {String} raw the raw string of the fixed game
 */
function progressFixed(raw) {
    const data = JSON.parse(raw);

    // update the list
    const idx = processed.findIndex(g => g.name === data.name);
    if (idx < 0) {
        processed.push(data);
    } else {
        processed[idx] = data;
    }

    // TODO: update a specific game instead of rewriting everything
    refreshProcessed();
}

/**
 * Displays a game result
 * 
 * @param {Object} data the game data
 * @param {Boolean} onlyErrors whether to display only errors
 * @returns 
 */
function displayGame(data, onlyErrors) {
    const target = $('#processedList'),
        item = $('<div></div>\n');

    if (data.haserror || data.romfiles.some(rf => rf.haserror)) {
        if (processedFormat == 'list') {
            item.append(`<div class="text-danger"><i class="icon-warning"></i> ${data.name} : ${data.errordetails ?? ''}</div>\n`);
        } else {
            item.append(`${data.name};${data.name}.zip;"${data.errordetails ?? 'OK'}"\n`);
        }

        for (let file of data.romfiles) {
            if (!onlyErrors && !file.haserror) {
                if (processedFormat == 'list') {
                    item.append(`<div class="pl-5">${file.name} : OK</div>\n`);
                } else {
                    item.append(`${data.name};${file.name};OK\n`);
                }
            } else if (file.haserror) {
                if (processedFormat == 'list') {
                    item.append(`<div class="pl-5">${file.name} : ${file.errordetails}</div>\n`);
                } else {
                    item.append(`${data.name};${file.name};"${file.errordetails}"\n`);
                }
            }
        }
    } else if (!onlyErrors) {
        if (processedFormat == 'list') {
            item.text(data.name + ' : OK');
        } else {
            item.text(`${data.name};${data.name}.zip;OK\n`);
        }
    }

    target.append(item);
}

/**
 * Finishes the progression
 *
 * @param {String} msg The message to display
 * @param {String} path A path to open, if any
 */
function progressDone(msg, path) {
    let p = $('#progress');
    p.find('.modal-footer, .msg').removeClass('d-none');
    p.find('.progress, .details, .stop').addClass('d-none');
    p.find('.msg').html(msg);

    $('#processedOnlyErrors, #filesListCopyLog').prop('disabled', false);

    if (path) {
        p.find('.open')
            .removeClass('d-none')
            .off('click')
            .on('click', (e) => {
                ipc('open-folder', path);
                e.preventDefault();
            });
    }
}
