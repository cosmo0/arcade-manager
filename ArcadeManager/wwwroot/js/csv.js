/**
 * List the files in a CSV repository and displays them in the window
 *
 * @param {string} repository The repository: username/repository
 * @param {string} folder The path to the folder: csv/mame2003
 * @param {string} details The path to the files details: csv/files.json
 * @param {string} name The name of the repository to load
 */
function listFiles(repository, folder, details, name) {
    $('#files').empty().html('Loading ' + name + ', please wait...');
    const template = $('#fileItemTemplate').html();

    getRemoteList(repository, details, folder, (data) => {
        $('#files').empty();

        for (let filedetail of data) {
            const fileItem = $(template);
            fileItem.find('.file').text(filedetail.filename);
            fileItem.find('.desc').text(filedetail.description);
            if (filedetail.types.indexOf('main') >= 0) {
                fileItem.addClass('text-primary border-primary');
            }

            // handle click
            fileItem.on('click', (e) => {
                newFile(filedetail.filename, (filename) => {
                    if (typeof filename !== 'undefined' && filename !== '') {
                        progressInit('Download file');

                        downloadFile(repository, folder + '/' + filedetail.filename, filename, (success) => {
                            if (success) {
                                progressDone('Done.');
                            } else {
                                progressDone('An error has occurred during file download.');
                            }
                        });
                    }
                });
            });

            $('#files').append(fileItem);
        }
    });
}