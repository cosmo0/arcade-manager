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
        // checks that a folders.json file exists
        if (!fs.existsSync(path.join(pack, 'folders.json'))) {
            throw 'No folder.json file has been found in the overlay pack.';
        }

        let folders = JSON.parse(fs.readFileSync(path.join(pack, 'folders.json'), { 'encoding': 'utf8' }));
        let packConfigsFolder = path.join(pack, folders.roms);
        let romfiles = fs.readdirSync(roms);
        let packConfigs = fs.readdirSync(packConfigsFolder);

        let total = romfiles.length;
        let current = 1;

        let copyCommon = new Promise((fulfill, reject) => {
            // copy common files
            if (typeof folders.common !== 'undefined' && folders.common) {
                total++;
                
                this.emit('progress.install', total, current++, 'common files');

                let commonTarget = path.join(config, folders.common.dest);
                fs.ensureDirSync(commonTarget);
                fs.copySync(path.join(pack, folders.common.src), commonTarget, { 'overwrite': false });
                
                fulfill();
            } else {
                fulfill();
            }
        });

        let copyShaders = new Promise((fulfill, reject) => {
            // copy shaders
            if (typeof folders.shaders !== 'undefined' && folders.shaders) {
                total++;
                
                this.emit('progress.install', total, current++, 'shaders');

                let shadersTarget = path.join(config, folders.shaders.dest);
                fs.ensureDirSync(shadersTarget);
                fs.copySync(path.join(pack, folders.shaders.src), shadersTarget, { 'overwrite': false });

                fulfill();
            } else {
                fulfill();
            }
        });

        let copyOverlays = new Promise((fulfill, reject) => {
            // list all roms & roms cfg
            for (let rom of romfiles) {
                current++;
                
                if (!rom.endsWith('.zip')) { continue; }

                // for each zip, search a matching rom cfg
                let packCfgIdx = packConfigs.indexOf(rom + '.cfg');
                if (packCfgIdx < 0) {
                    console.log('No overlay found for %s', rom);
                    continue;
                } else {
                    this.emit('progress.install', total, current, rom);

                    let packCfg = packConfigs[packCfgIdx];
                    let destCfg = path.join(roms, packCfg);

                    // copy the rom cfg
                    fs.copySync(path.join(packConfigsFolder, packCfg), destCfg, { 'overwrite': false });

                    // parse rom cfg to get overlay cfg
                    let packCfgContent = fs.readFileSync(destCfg, { 'encoding': 'utf8' });
                    let overlayFile = /input_overlay[\s]*=[\s]*(.*\.cfg)/igm.exec(packCfgContent)[1]; // extract overlay path
                    overlayFile = overlayFile.substring(overlayFile.lastIndexOf('/') + 1); // just the file name

                    // copy overlay cfg
                    let destOverlayFile = path.join(config, folders.overlays.dest, overlayFile);
                    fs.copySync(path.join(pack, folders.overlays.src, overlayFile), destOverlayFile, { 'overwrite': false });

                    // parse overlay cfg to get overlay image
                    let overlayContent = fs.readFileSync(destOverlayFile, { 'encoding': 'utf-8' });
                    let overlayImage = /overlay0_overlay[\s]*=[\s]*(.*\.png)/igm.exec(overlayContent)[1];
            
                    // copy overlay image
                    fs.copySync(
                        path.join(pack, folders.overlays.src, overlayImage),
                        path.join(config, folders.overlays.dest, overlayImage),
                        { 'encoding': 'utf-8' });
                }
            }
        });

        copyCommon().then(() => copyShaders()).then(() => copyOverlays());
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
        
        /***********************************
        *
        *                   THIS METHOD IS NOT FINISHED
        * 
        * Since Github limits the anonymous calls to their API to 60 requests an hour,
        * and an overlay download requires at least 3 API calls (game cfg, overlay cfg, image),
        * and there are up to 1200 overlays in some pack, even the authenticated requests
        * (max 5000 requests per hour) wouldn't work in some cases.
        * 
        * Making this work would require:
        * - making the user enter their Github account credentials
        * - checking the remaining limits before downloading anything
        * 
        * ... and it's not a priority, so... maybe I'll do it later.
        *
        ************************************/
        
        return;
        
        // download and install common configs
        if (typeof common !== 'undefined' && common) {
            console.log('Installing common config');
            downloader.downloadFolder(repository, common.src, path.join(configFolder, common.dest));
        }

        // download and install shaders
        if (typeof shaders !== 'undefined' && shaders) {
            console.log('Installing shaders');
            downloader.downloadFolder(repository, shaders.src, path.join(configFolder, shaders.dest));
        }

        // get roms configs list
        downloader.listFiles(repository, roms, (romConfigs) => {
            for (let romcfg of romConfigs) {
                if (romcfg.type !== 'file') { continue; } // only process files
                
                let zip = romcfg.name.replace('.cfg', '');
                if (fs.existsSync(path.join(romsFolder, zip))) {
                    console.log('Installing overlay for %s', zip);

                    // download and install rom cfg
                    downloader.downloadFile(repository, romcfg.path, (romcfgContent) => {
                        let localromcfg = path.join(configFolder, overlays.dest);
                        fs.ensureFileSync(localromcfg);
                        fs.writeFileSync(localromcfg, romcfgContent);
                        
                        // parse rom cfg to get overlay cfg

                        // download and install overlay cfg

                        // parse overlay cfg to get overlay image

                        // download and install overlay image
                    });

                }
            }
        });
    }
};