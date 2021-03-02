const fs = require('fs-extra');
const path = require('path');
const https = require('follow-redirects').https;
const events = require('events');

const protocol = 'https:';
const api = 'api.github.com';
const raw = 'https://raw.githubusercontent.com';

module.exports = class Downloader extends events {
    /**
     * Downloads a file from GitHub and returns its content
     * 
     * @param {String} repository The repository (user/repo)
     * @param {String} file The file path (path/to/file.txt)
     * @param {Function} callback The callback method
     * @returns {Buffer} The file content
     */
    downloadFile (repository, file, callback) {
        let url = raw + '/' + repository + '/master/' + file;
        https.get(url, (res) => {
            if (res.statusCode !== 200) {
                res.resume();
                throw 'Unable to download file ' + res.req.path;
            }

            let isbinary = res.headers['content-type'].indexOf('text/plain') < 0;

            if (!isbinary) {
                res.setEncoding('utf8');
            }

            let rawData = [];
            res.on('data', (chunk) => {
                rawData.push(chunk);
            }).on('end', () => {
                if (isbinary) {
                    callback(Buffer.concat(rawData));
                } else {
                    callback(Buffer.from(rawData.join(''), 'utf8').toString('utf8'));
                }
            });
        });
    }

    /**
     * Recursively downloads a folder into a target folder
     * 
     * @param {string} repository The repository (user/repo)
     * @param {string} folder The path to the folder (path/to/folder)
     * @param {string} targetFolder The folder to download into
     * @param {bool} overwrite Whether to overwrite existing files
     * @param {function} replace A function to replace things in the content
     * @param {function} callback A callback
     */
    downloadFolder (repository, folder, targetFolder, overwrite, replace, callback) {
        fs.ensureDirSync(targetFolder);
        
        console.log('Downloading folder %s to %s', folder, targetFolder);

        this.listFiles(repository, folder, (list) => {
            let requests = list.reduce((promisechain, item) => {
                return promisechain.then(() => new Promise((resolve) => {
                    if (item.type === 'file' || item.type === 'blob') {
                        let dest = path.join(targetFolder, item.path);
                        if (overwrite === true || !fs.existsSync(dest)) {
                            console.log('write file to ' + dest);
                            this.downloadFile(repository, folder + '/' + item.path, (content) => {
                                if (replace && typeof content === 'string') { content = replace(content); }
                                fs.writeFileSync(dest, content);
                                resolve();
                            });
                        }
                    } else {
                        this.downloadFolder(repository, folder + '/' + item.path, path.join(targetFolder, item.path), overwrite, replace, resolve);
                    }
                }));
            }, Promise.resolve());

            // execute requests
            requests
                .then(() => {
                    console.log('Folder %s has been downloaded', folder);
                    if (callback) callback();
                });
        });
    }

    /**
     * Lists the files in a folder
     * 
     * @param {String} repository The repository (user/repo)
     * @param {String} folder The path to the folder (path/to/folder)
     * @param {Function} callback The callback method
     * @returns {String} The files list
     */
    listFiles (repository, folder, callback) {
        let upFolder = folder.substring(0, folder.lastIndexOf('/'));

        // get level-up folder to get the SHA of the folder
        let urlUpFolders = '/repos/' + repository + '/contents/' + upFolder;
        console.log('Listing files in %s / %s', repository, upFolder);
        https.get({ protocol, 'host': api, 'path': urlUpFolders, 'headers': { 'User-Agent': 'arcade-manager' } }, (res) => {
            res.setEncoding('utf8');
            let rawDataFolders = '';
            res.on('data', (chunk) => { rawDataFolders += chunk; });
            res.on('end', () => {
                let siblingFolders = JSON.parse(rawDataFolders);
                for (let f of siblingFolders) {
                    if (f.path === folder) {
                        // get full list of files from this folder
                        let urlFullFilesList = '/repos/' + repository + '/git/trees/' + f.sha;
                        console.log('Listing files for tree %s', f.sha);
                        https.get({ protocol, 'host': api, 'path': urlFullFilesList, 'headers': { 'User-Agent': 'arcade-manager' } }, (res) => {
                            res.setEncoding('utf8');
                            let rawDataFiles = '';
                            res.on('data', (chunk) => { rawDataFiles += chunk; });
                            res.on('end', () => {
                                callback(JSON.parse(rawDataFiles).tree);
                            });
                        });
                        break;
                    }
                }
            });
        });
    }
};