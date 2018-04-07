const fs = require('fs-extra');
const path = require('path');
const http = require('http');
const https = require('https');

const downloader = require('./downloader.js');

module.exports = {
    /**
     * Checks that the program can access the specified folder
     * 
     * @param {string} folder The folder to check for access
     * @param {string} checkWrite Whether to check for write access
     * @returns {bool} Whether the check is successful
     */
    checkAccess: function checkAccess (folder, checkWrite) {
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
    },

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
    downloadPack: function downloadPack(romsFolder, configFolder, repository, roms, overlays, common, shaders) {
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