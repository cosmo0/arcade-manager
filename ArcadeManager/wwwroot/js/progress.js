
$(() => {
    // bind progress events
    ipcRenderer.on("progress", (sender, data) => {
        if (data.init) {
            progressInit(data.label, data.canCancel);
        }
        else if (data.end) {
            progressDone(data.label, data.folder);
        }
        else {
            progress(data.total, data.current, data.label);
        }
    });

    // bind progress log
    ipcRenderer.on("progress-log", (sender, data) => {
        progressLog(data.msg, data.error);
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
        ipcRenderer.once('cancel');
    });
});

/**
 * Shows the progression modal
 *
 * @param {string} title The modal title
 * @param {bool} canCancel Whether the user can cancel the operation
 */
function progressInit(title, canCancel) {
    if (typeof canCancel === 'undefined') { canCancel = false; }
    let p = $('#progress');

    // display modal
    p.modal({ 'backdrop': 'static', 'keyboard': false });

    // hides/displays some items
    p.find('.modal-footer, .msg, .log').addClass('d-none');
    p.find('.progress, .details').removeClass('d-none');

    // display or hide cancel button
    if (canCancel) {
        p.find('.stop').removeClass('d-none');
    } else {
        p.find('.stop').addClass('d-none');
    }

    // reset texts
    p.find('.modal-title').text(title);
    p.find('.details, .log').text('');

    // reset styles
    p.find('.progress .progress-bar').width('0%');
    p.find('.log').removeClass('alert-danger').addClass('alert-info');
    $('#progress .stop button')
        .prop('disabled', false)
        .removeClass('disabled btn-outline-secondary')
        .addClass('btn-outline-danger');
}

/**
 * Advances the progression modal
 *
 * @param {int} total The total number of items
 * @param {int} current The current item number
 * @param {string} details The details to display
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
 * Finishes the progression
 *
 * @param {string} msg The message to display
 * @param {string} path A path to open, if any
 */
function progressDone(msg, path) {
    let p = $('#progress');
    p.find('.modal-footer, .msg').removeClass('d-none');
    p.find('.progress, .details, .stop').addClass('d-none');
    p.find('.msg').html(msg);

    if (path) {
        p.find('.open')
            .removeClass('d-none')
            .off('click')
            .on('click', (e) => {
                ipcRenderer.send('open-folder', path);
                e.preventDefault();
            });
    }
}
