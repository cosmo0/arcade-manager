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
     * Installs a local overlay pack
     * 
     * @param {string} roms The path to the roms folder
     * @param {string} config The path to the Retropie configs share
     * @param {string} pack The path to the overlays pack
     */
    installPack (roms, config, pack) {
        this.emit('start.install');

        // checks that a folders.json file exists
        if (!fs.existsSync(path.join(pack, 'folders.json'))) {
            throw 'No folder.json file has been found in the overlay pack.';
        }

        let folders = JSON.parse(fs.readFileSync(path.join(pack, 'folders.json'), { 'encoding': 'utf8' }));
        let packConfigsFolder = path.join(pack, folders.roms);
        fs.readdir(roms, (err, romfiles) => {
            if (err) throw err;
            fs.readdir(packConfigsFolder, (err, packConfigs) => {
                if (err) throw err;
            
                let total = romfiles.length;
                if (typeof folders.common !== 'undefined' && folders.common) { total++; }
                if (typeof folders.shaders !== 'undefined' && folders.shaders) { total++; }
                let current = 1;

                // copy common files
                let installCommon = new Promise((resolve) => {
                    if (typeof folders.common !== 'undefined' && folders.common) {
                        this.emit('progress.install', total, current++, 'common files');
                        console.log('installing common files');

                        let commonTarget = path.join(config, folders.common.dest);
                        fs.ensureDir(commonTarget, (err) => {
                            if (err) throw err;
                            fs.copy(path.join(pack, folders.common.src), commonTarget, { 'overwrite': false }, (err) => {
                                if (err) throw err;
                                resolve();
                            });
                        });
                    } else {
                        resolve();
                    }
                });

                let installShaders = new Promise((resolve) => {
                    // copy shaders
                    if (typeof folders.shaders !== 'undefined' && folders.shaders) {
                        this.emit('progress.install', total, current++, 'shaders');
                        console.log('Installing shaders');

                        let shadersTarget = path.join(config, folders.shaders.dest);
                        fs.ensureDir(shadersTarget, (err) => {
                            if (err) throw err;
                            fs.copy(path.join(pack, folders.shaders.src), shadersTarget, { 'overwrite': false }, (err) => {
                                if (err) throw err;
                                resolve();
                            });
                        });
                    } else {
                        resolve();
                    }
                });

                let requests = romfiles.reduce((promisechain, rom, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        this.emit('progress.install', total, current++, rom);
                        console.log('Processing %s', rom);

                        if (!rom.endsWith('.zip')) {
                            resolve();
                            return;
                        }

                        // for each zip, search a matching rom cfg
                        let packCfgIdx = packConfigs.indexOf(rom + '.cfg');
                        if (packCfgIdx < 0) {
                            console.log('No overlay found for %s', rom);
                            resolve();
                        } else {    
                            let packCfg = packConfigs[packCfgIdx];
                            let destCfg = path.join(roms, packCfg);

                            // copy the rom cfg
                            fs.copy(path.join(packConfigsFolder, packCfg), destCfg, { 'overwrite': false }, (err) => {
                                if (err) throw err;

                                // parse rom cfg to get overlay cfg
                                let packCfgContent = fs.readFileSync(destCfg, { 'encoding': 'utf8' });
                                let overlayFile = /input_overlay[\s]*=[\s]*(.*\.cfg)/igm.exec(packCfgContent)[1]; // extract overlay path
                                overlayFile = overlayFile.substring(overlayFile.lastIndexOf('/') + 1); // just the file name

                                // copy overlay cfg
                                let destOverlayFile = path.join(config, folders.overlays.dest, overlayFile);
                                fs.copy(path.join(pack, folders.overlays.src, overlayFile), destOverlayFile, { 'overwrite': false }, (err) => {
                                    if (err) throw err;

                                    // parse overlay cfg to get overlay image
                                    let overlayContent = fs.readFileSync(destOverlayFile, { 'encoding': 'utf-8' });
                                    let overlayImage = /overlay0_overlay[\s]*=[\s]*(.*\.png)/igm.exec(overlayContent)[1];

                                    // copy overlay image
                                    fs.copy(
                                        path.join(pack, folders.overlays.src, overlayImage),
                                        path.join(config, folders.overlays.dest, overlayImage),
                                        { 'encoding': 'utf-8' }, (err) => {
                                            if (err) throw err;
                                            resolve();
                                        });
                                });
                            });
                        }
                    }));
                }, Promise.resolve());

                installCommon
                .then(() => installShaders)
                .then(() => requests)
                .then(() => {
                    this.emit('end.install');
                });
            });
        });
    }

    /**
     * Downloads and installs an overlay pack
     * 
     * @param {string} romsFolder The path to the roms
     * @param {string} configFolder The path to the config share
     * @param {string} repository The Github repository (ex: user/repo)
     * @param {string} roms The roms configs folder (ex: overlays/roms)
     * @param {string} overlays The overlays configs folder (ex: overlays/config/overlays)
     * @param {string} common The common files folder, if any (ex: overlays/config/common)
     * @param {string} shaders The shaders folder, if any (ex: overlays/config/shaders)
     */
    downloadPack (romsFolder, configFolder, repository, roms, overlays, common, shaders) {
        this.emit('start.download');

        this.emit('progress.download', 100, 1, 'files list');

        // get roms configs list
        downloader.listFiles(repository, roms, (romConfigs) => {
            let total = romConfigs.length;
            if (typeof common !== 'undefined' && common) { total++; }
            if (typeof shaders !== 'undefined' && shaders) { total++; }
            let current = 1;

            // download and install common configs
            let installCommon = new Promise((resolve) => {
                if (typeof common !== 'undefined' && common) {
                    console.log('Installing common config');
                    this.emit('progress.download', total, current++, 'common');
                    downloader.downloadFolder(repository, common.src, path.join(configFolder, common.dest));
                    resolve();
                }
            });

            let installShaders = new Promise((resolve) => {
                // download and install shaders
                if (typeof shaders !== 'undefined' && shaders) {
                    console.log('Installing shaders');
                    this.emit('progress.download', total, current++, 'shaders');
                    downloader.downloadFolder(repository, shaders.src, path.join(configFolder, shaders.dest));
                    resolve();
                }
            });

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
                                let overlayFile = /input_overlay[\s]*=[\s]*(.*\.cfg)/igm.exec(romcfgContent)[1]; // extract overlay path
                                overlayFile = overlayFile.substring(overlayFile.lastIndexOf('/')); // just the file name
                                let packOverlayFile = path.join(overlays.src, overlayFile); // concatenate with pack path                            
                                
                                // download and copy overlay cfg
                                downloader.downloadFile(repository, packOverlayFile, (packOverlayFileContent) => {
                                    let localoverlaycfg = path.join(configFolder, overlays.dest, overlayFile);
                                    fs.writeFile(localoverlaycfg, packOverlayFileContent, (err) => {
                                        if (err) throw err;

                                        // parse overlay cfg to get overlay image
                                        let packOverlayImage = /overlay0_overlay[\s]*=[\s]*(.*\.png)/igm.exec(packOverlayFileContent)[1];
                                        let packOverlayImageFile = path.join(overlays.src, packOverlayImage);

                                        // download and copy overlay image
                                        downloader.downloadFile(repository, packOverlayImageFile, (imageContent) => {
                                            let localoverlayimg = path.join(configFolder, overlays.dest, packOverlayImage);
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
            .then(() => installShaders)
            .then(() => requests)
            .then(() => {
                this.emit('end.download');
            });
        });
    }
};