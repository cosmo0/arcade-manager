let processed = [];

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

    // bind errors
    window.onerror = function (msg, url, line, col, error) {
        var extra = !col ? '' : ' ; column: ' + col;
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
        navigator.clipboard.writeText(target.text());
        $('#filesListCopyLogMessage').removeClass('d-none').fadeOut(1000);
    });

    // bind "only errors" checkbox change
    $('#processedOnlyErrors').off('change').on('change', () => {
        const onlyErrors = $('#processedOnlyErrors').is(':checked');
        for (let game of processed) {
            displayGame(game, onlyErrors);
        }
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
 * Displays the list of processed games
 * 
 * @param {String} raw the raw string of the processed games
 */
function progressProcessed(raw) {
    $('#progress .processed').removeClass('d-none');

    const data = JSON.parse(raw);

    displayGame(data, $('#processedOnlyErrors').is(':checked'));
}

/**
 * Displays a game result
 * 
 * @param {Object} data the game data
 * @param {Boolean} onlyErrors whether to display only errors
 * @returns 
 */
function displayGame(data, onlyErrors) {
    const target = $('#processedList');

    processed.push(data);

    if (onlyErrors && !data.haserror) {
        return;
    }

    const item = $('<div></div>');

    if (data.haserror) {
        item.append(`<div class="text-danger"><i class="icon-warning"></i> ${data.name} : ${data.errordetails}</div>`);
        for (let file of data.romfiles) {
            if (file.haserror) {
                item.append(`<div class="pl-5">${file.name} : ${file.errordetails}</div>`)
            }
        }
    } else {
        item.text(data.name + ' : OK');
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
