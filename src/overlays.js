const fs = require('fs-extra');
const path = require('path');
const http = require('http');
const https = require('https');
const events = require('events');
const settings = require('electron-settings');

const Downloader = require('./downloader.js');
const downloader = new Downloader();

const data = require('./data.json');

let mustCancel = false; // cancellation token

module.exports = class Overlays extends events {
    /**
     * Asks the class to cancel the next action
     */
    cancel() {
        mustCancel = true;
    }

    /**
     * Checks that the program can access the specified folder
     * 
     * @param {string} folder The folder to check for access
     * @param {string} checkWrite Whether to check for write access
     * @returns {bool} Whether the check is successful
     */
    checkAccess (folder, checkWrite) {
        try {
            fs.ensureDirSync(folder);
        } catch (err) {
            console.error('The folder %s does not exist or cannot be read, and cannot be created!', folder);
            console.log('Exiting...');
            return false;
        }
        
        if (fs.existsSync(folder)) {
            if (checkWrite) {
                var testFile = path.join(folder, '_test-install.txt');
                try {
                    fs.writeFileSync(testFile, 'test');
                    if (fs.existsSync(testFile)) {
                        fs.unlinkSync(testFile);
                    } else {
                        console.error('Unable to write files into the folder %s!', folder);
                        console.log('Exiting...');
                        return false;
                    }
                } catch (err) {
                    console.error('Unable to write files into the folder %s: %o', folder, err);
                    console.log('Exiting...');
                    return false;
                }
    
                console.log('%s can be written to', folder);
            } else {
                console.log('%s can be read', folder);
            }
        } else {
            console.error('The folder %s does not exist or cannot be read!', folder);
            console.log('Exiting...');
            return false;
        }

        return true;
    }

    /**
     * Fixes the paths in the specified file content
     * 
     * @param {object} base The base paths (ex: { retropie: /etc/, recalbox: /etc/ })
     * @param {string} content The file content to fix
     */
    fixPath(base, content) {
        let fromOs = settings.get('os') === 'retropie' ? 'recalbox' : 'retropie';
        let toOs = settings.get('os');
        return content.replace(base[fromOs], base[toOs]);
    }

    /**
     * Downloads and installs an overlay pack
     * 
     * @param {string} romsFolder The path to the roms (ex: \\retropie\roms)
     * @param {string} configFolder The path to the Retropie config (ex: \\retropie\configs)
     * @param {object} packInfos The chosen pack informations
     * @param {bool} overwrite Whether to overwrite existing files
     */
    downloadPack (romsFolder, configFolder, packInfos, overwrite) {
        mustCancel = false;
        let nbOverlays = 0;

        let repository = packInfos.repository,
            roms = packInfos.roms,
            overlays = packInfos.overlays,
            common = packInfos.common,
            base = packInfos.base;

        const flag = overwrite ? 'w' : 'wx';
        const os = settings.get('os');
        this.emit('start.download');

        this.emit('progress.download', 100, 1, 'files list');

        // get roms configs list
        downloader.listFiles(repository, roms, (romConfigs) => {
            let total = romConfigs.length;
            let current = 1;

            // download and install common configs
            let installCommon = new Promise((resolve) => {
                if (mustCancel) { resolve(); return; }

                if (typeof common !== 'undefined' && common) {
                    total++;
                    console.log('Installing common files');
                    this.emit('progress.download', total, current++, 'common files');
                    downloader.downloadFolder(repository, common.src, path.join(configFolder, common.dest[os]), overwrite);
                }
                
                resolve();
            });

            // download and install rom configs
            let requests = romConfigs.reduce((promisechain, romcfg, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    if (mustCancel) { resolve(); return; }

                    this.emit('progress.download', total, current++, romcfg.name);

                    // only process config files
                    if (romcfg.type !== 'file' || !romcfg.name.endsWith('.zip.cfg')) { 
                        resolve();
                        return;
                    }

                    let zip = romcfg.name.replace('.cfg', '');
                    let localromcfg = path.join(romsFolder, romcfg.name);
                    if (fs.existsSync(path.join(romsFolder, zip))) {
                        console.log('Installing overlay for %s', zip);

                        // download and copy rom cfg
                        downloader.downloadFile(repository, romcfg.path, (romcfgContent) => {
                            if (mustCancel) { resolve(); return; }
                            romcfgContent = this.fixPath(base, romcfgContent);
                            fs.ensureDirSync(path.dirname(localromcfg));
                            fs.writeFile(localromcfg, romcfgContent, { flag }, (err) => {
                                if (err && err.code !== 'EEXIST') throw err;
                                if (mustCancel) { resolve(); return; }

                                // parse rom cfg to get overlay cfg
                                let overlayFile = /input_overlay[\s]*=[\s]*"?(.*\.cfg)"?/igm.exec(romcfgContent)[1]; // extract overlay path
                                overlayFile = overlayFile.substring(overlayFile.lastIndexOf('/')); // just the file name
                                let packOverlayFile = path.join(overlays.src, overlayFile); // concatenate with pack path                          
                                let localoverlaycfg = path.join(configFolder, overlays.dest[os], overlayFile);

                                // download and copy overlay cfg
                                downloader.downloadFile(repository, packOverlayFile, (packOverlayFileContent) => {
                                    if (mustCancel) { resolve(); return; }
                                    packOverlayFileContent = this.fixPath(base, packOverlayFileContent);
                                    fs.ensureDirSync(path.dirname(localoverlaycfg));
                                    fs.writeFile(localoverlaycfg, packOverlayFileContent, { flag }, (err) => {
                                        if (err && err.code !== 'EEXIST') throw err;
                                        if (mustCancel) { resolve(); return; }

                                        // parse overlay cfg to get overlay image
                                        let packOverlayImage = /overlay0_overlay[\s]*=[\s]*"?(.*\.png)"?/igm.exec(packOverlayFileContent)[1];
                                        // build path to image file
                                        let packOverlayImageFile = path.join(overlays.src, packOverlayImage);
                                        let localoverlayimg = path.join(configFolder, overlays.dest[os], packOverlayImage);

                                        // download and copy overlay image
                                        downloader.downloadFile(repository, packOverlayImageFile, (imageContent) => {
                                            if (mustCancel) { resolve(); return; }
                                            fs.writeFile(localoverlayimg, imageContent, { flag }, (err) => {
                                                if (err && err.code !== 'EEXIST') throw err;
                                                nbOverlays++;
                                                resolve();
                                            });
                                        });
                                    });
                                });
                            });
                        });
                    } else {
                        // the corresponding zip file does not exist
                        resolve();
                    }
                }));
            }, Promise.resolve());

            installCommon
            .then(() => requests)
            .then(() => {
                this.emit('log', 'Installed ' + nbOverlays + ' overlays');
                this.emit('end.download', mustCancel);
            });
        });
    }
};