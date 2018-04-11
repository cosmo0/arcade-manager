const fs = require('fs-extra');
const path = require('path');
const http = require('http');
const https = require('https');
const events = require('events');

const protocol = 'https:'
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

            let isbinary = res.headers["content-type"].indexOf('text/plain') < 0;

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
     */
    downloadFolder (repository, folder, targetFolder) {
        fs.ensureDirSync(targetFolder);
        
        this.listFiles(repository, folder, (list) => {
            for (let item of list) {
                if (item.type === 'file') {
                    let dest = path.join(targetFolder, item.name);
                    if (!fs.existsSync(dest)) {
                        console.log('write file to ' + dest);
                        downloader.downloadFile(repository, item.path, (content) => {
                            fs.writeFileSync(dest, content);
                        });
                    }
                } else {
                    this.downloadFolder(repository, item.path, path.join(targetFolder, item.name));
                }
            }
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
        let url = '/repos/' + repository + '/contents/' + folder;
        https.get({ protocol, 'host': api, 'path': url, 'headers': { 'User-Agent': 'retropie-arcade-manager' } }, (res) => {
            res.setEncoding('utf8');
            let rawData = '';
            res.on('data', (chunk) => { rawData += chunk; });
            res.on('end', () => {
                callback(JSON.parse(rawData));
            });
        });
    }
};