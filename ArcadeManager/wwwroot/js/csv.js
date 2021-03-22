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

    ipc('csv-getlist', { repository, details, folder }, (sender, data) => {
        const filesdetails = JSON.parse(data.filesdetailsContent);
        $('#files').empty();

        for (let filedetail of filesdetails.files) {
            // get matching file in the folder
            const fileinfos = data.folderContents.find((item) => item.path === filedetail.filename);
            if (typeof fileinfos !== 'undefined') {
                const fileItem = $(template);
                fileItem.find('.file').text(filedetail.filename);
                fileItem.find('.desc').text(filedetail.description);
                if (filedetail.types.indexOf('main') >= 0) {
                    fileItem.addClass('text-primary border-primary');
                }

                // handle click
                fileItem.on('click', (e) => {
                    newFile(fileinfos.path, (sender, filename) => {
                        if (typeof filename !== 'undefined' && filename !== '') {
                            progressInit('Download file');

                            downloadFile(repository, folder + '/' + fileinfos.path, (success) => {
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
        }
    });
}