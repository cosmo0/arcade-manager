const fs = require('fs-extra');
const path = require('path');
const events = require('events');

const Csv = require('./csv.js');
const csv = new Csv();

let mustCancel = false; // cancellation token

module.exports = class Roms extends events {
    /**
     * Asks the class to cancel the next action
     */
    cancel() {
        mustCancel = true;
    }

    /**
     * Computes an entry size for display
     * 
     * @param {String} entry The file or folder entry to compute the size
     * @returns The entry size with the size unit
     */
    entrySize(entry) {
        if (!fs.existsSync(entry)) {
            return '';
        }

        let size = 0;
        let stat = fs.statSync(entry);

        if (stat.isDirectory()) {
            // if folder, sum the size of files in it
            const files = fs.readdirSync(entry);
            for (let f of files) {
                size += fs.statSync(path.join(entry, f)).size;
            }
        } else {
            size = stat.size;
        }

        // compute size in a human-readable format
        let unit = 0;
        while (size > 1024) {
            size = size / 1024;
            unit++;
        }

        return Math.round(size).toLocaleString() + ' ' + [ 'bytes', 'KB', 'MB', 'GB' ][unit];
    }

    /**
     * Adds roms from a romset to a folder,
     * based on a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} romset The path to the romset folder
     * @param {string} selection The path to the selection folder
     * @param {bool} overwrite Whether to overwrite existing files
     */
    add (file, romset, selection, overwrite) {
        mustCancel = false;
        let romsCopied = 0;

        this.emit('start.add');

        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;
            if (mustCancel) { resolve(); return; }

            let fileCsv = csv.parse(fileContents);

            fs.ensureDirSync(selection);

            console.log('Copying %i files', fileCsv.length);

            let requests = fileCsv.reduce((promisechain, line, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    if (mustCancel) { resolve(); return; }

                    let game = line.name;
                    let zip = line.name + '.zip';
                    let sourceRom = path.join(romset, zip);
                    let destRom = path.join(selection, zip);
    
                    this.emit('progress.add', fileCsv.length, index + 1, game + ' (' + this.entrySize(sourceRom) + ')');
    
                    // test if source file exists and destination does not
                    if (fs.existsSync(sourceRom) && (!fs.existsSync(destRom) || overwrite)) {
                        // copy rom
                        fs.copy(sourceRom, destRom, { overwrite }, (err) => {
                            if (err) throw err;
                            romsCopied++;
                            if (mustCancel) { resolve(); return; }
    
                            // copy CHD
                            let sourceChd = path.join(romset, game);
                            if (fs.existsSync(sourceChd)) {
                                this.emit('progress.add', fileCsv.length, index + 1, game + ' chd (' + this.entrySize(sourceChd) + ')');

                                fs.copy(sourceChd, path.join(selection, game), { overwrite }, (err) => {
                                    if (err) throw err;
                                    if (mustCancel) { resolve(); return; }

                                    console.log('%s copied', sourceChd);
                                    resolve();
                                });
                            } else {
                                console.log('%s copied', sourceRom);
                                resolve();
                            }
                        });
                    } else {
                        console.log('%s game source not found or rom already copied', game);
                        resolve();
                    }
                }));
            }, Promise.resolve());

            requests.then(() => {
                this.emit('log', 'Copied ' + romsCopied + ' roms');
                this.emit('end.add', mustCancel, selection);
            });
        });
    }

    /**
     * Removes roms from a folder,
     * based on a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} selection The path to the selection folder
     */
    remove (file, selection) {
        mustCancel = false;
        let romsRemoved = 0;

        this.emit('start.remove');

        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;
            if (mustCancel) { resolve(); return; }

            let fileCsv = csv.parse(fileContents);

            let requests = fileCsv.reduce((promisechain, line, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    if (mustCancel) { resolve(); return; }

                    let zip = line.name + '.zip';
                    let rom = path.join(selection, zip);

                    this.emit('progress.remove', fileCsv.length, index + 1, zip);

                    // test if rom exists
                    fs.pathExists(rom, (err, romExists) => {
                        if (romExists) {
                            // delete rom
                            fs.remove(rom, (err) => {
                                romsRemoved++;
                                console.log('%s deleted', rom);
                                resolve();
                            });
                        } else {
                            resolve();
                        }
                    });
                }));
            }, Promise.resolve());

            requests.then(() => {
                this.emit('log', 'Deleted ' + romsRemoved + ' roms');
                this.emit('end.remove', mustCancel, selection);
            });
        });
    }

    /**
     * Keeps only listed roms in a folder
     * that are listed in a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} selection The path to the selection folder
     */
    keep (file, selection) {
        mustCancel = false;
        let romsRemoved = 0;

        this.emit('start.keep');

        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;
            if (mustCancel) { resolve(); return; }
            
            let fileCsv = csv.parse(fileContents);

            // list files in selection folder
            fs.readdir(selection, (err, files) => {
                if (err) throw err;
                if (mustCancel) { resolve(); return; }

                let requests = files.reduce((promisechain, zip, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        if (mustCancel) { resolve(); return; }

                        this.emit('progress.keep', files.length, index + 1, zip);

                        // skip non-zip files
                        if (!zip.endsWith('.zip')) { resolve(); return; }

                        // file not found in csv -> remove it
                        let csvItem = fileCsv.find((item) => item.name === zip.replace('.zip', ''));
                        if (typeof csvItem === 'undefined') {
                            console.log('remove %s', zip);
                            fs.remove(path.join(selection, zip), (err) => {
                                romsRemoved++;
                                resolve();
                            });
                        } else {
                            // file found in csv -> keep it
                            resolve();
                        }
                    }));
                }, Promise.resolve());

                requests.then(() => {
                    this.emit('log', 'Deleted ' + romsRemoved + ' roms');
                    this.emit('end.keep', mustCancel, selection);
                });
            });
        });
    }
};