const fs = require('fs-extra');
const path = require('path');
const http = require('http');
const https = require('https');
const events = require('events');

const Downloader = require('./downloader.js');
const downloader = new Downloader();

function copyFolder() {

}

module.exports = class Overlays extends events {
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
     * Downloads and installs an overlay pack
     * 
     * @param {string} romsFolder The path to the roms
     * @param {string} repository The Github repository (ex: user/repo)
     * @param {string} files The folder where the config files to copy are located (ex: overlays/)
     * @param {string} common The folder where common files are located (ex: overlays/common/)
     */
    downloadPack (romsFolder, repository, files, common) {
        this.emit('start.download');

        this.emit('progress.download', 100, 1, 'files list');

        // get roms configs list
        downloader.listFiles(repository, files, (romConfigs) => {
            let total = romConfigs.length;
            if (typeof common !== 'undefined' && common) { total++; }
            let current = 1;

            // download and install common configs
            let installCommon = new Promise((resolve) => {
                if (typeof common !== 'undefined' && common) {
                    console.log('Installing common files');
                    this.emit('progress.download', total, current++, 'common files');
                    downloader.downloadFolder(repository, common.src, path.join(romsFolder, common.dest));
                }
                
                resolve();
            });

            // download and install rom configs
            let requests = romConfigs.reduce((promisechain, romcfg, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    this.emit('progress.download', total, current++, romcfg.name);

                    // only process config files
                    if (romcfg.type !== 'file' || !romcfg.name.endsWith('.zip.cfg')) { 
                        resolve();
                        return;
                    }

                    let zip = romcfg.name.replace('.cfg', '');
                    if (fs.existsSync(path.join(romsFolder, zip))) {
                        console.log('Installing overlay for %s', zip);

                        // download and copy rom cfg
                        downloader.downloadFile(repository, romcfg.path, (romcfgContent) => {
                            let localromcfg = path.join(romsFolder, romcfg.name);
                            fs.ensureFileSync(localromcfg);
                            fs.writeFile(localromcfg, romcfgContent, (err) => {
                                if (err) throw err;

                                // parse rom cfg to get overlay cfg
                                let overlayFile = /input_overlay[\s]*=[\s]*"?(.*\.cfg)"?/igm.exec(romcfgContent)[1]; // extract overlay path
                                let packOverlayFile = path.join(files, overlayFile); // concatenate with pack path                          
                                
                                // download and copy overlay cfg
                                downloader.downloadFile(repository, packOverlayFile, (packOverlayFileContent) => {
                                    let localoverlaycfg = path.join(romsFolder, overlayFile);
                                    fs.ensureFileSync(localoverlaycfg);
                                    fs.writeFile(localoverlaycfg, packOverlayFileContent, (err) => {
                                        if (err) throw err;

                                        // parse overlay cfg to get overlay image
                                        let packOverlayImage = /overlay0_overlay[\s]*=[\s]*"?(.*\.png)"?/igm.exec(packOverlayFileContent)[1];
                                        // build path to image file
                                        let packOverlayImageFile = path.join(path.dirname(packOverlayFile), packOverlayImage);

                                        // download and copy overlay image
                                        downloader.downloadFile(repository, packOverlayImageFile, (imageContent) => {
                                            let localoverlayimg = path.join(path.dirname(localoverlaycfg), packOverlayImage);
                                            fs.writeFile(localoverlayimg, imageContent, (err) => {
                                                if (err) throw err;
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
                this.emit('end.download');
            });
        });
    }
};