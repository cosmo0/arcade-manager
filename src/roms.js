const fs = require('fs-extra');
const path = require('path');
const events = require('events');
const csvparse = require('csv-parse/lib/sync');

// delimiter
const defaultDelimiter = ';';

module.exports = class Roms extends events {
    /**
     * Adds roms from a romset to a folder,
     * based on a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} romset The path to the romset folder
     * @param {string} selection The path to the selection folder
     * @param {functiion} callback The callback method
     */
    add (file, romset, selection, callback) {
        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;

            let fileCsv = csvparse(
                fileContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });

            console.log('Copying %i files', fileCsv.length);

            let requests = fileCsv.reduce((promisechain, line, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    let game = line.name;
                    let zip = line.name + '.zip';
                    let sourceRom = path.join(romset, zip);
                    let destRom = path.join(selection, zip);
    
                    this.emit('progress.add', fileCsv.length, index + 1, zip);
    
                    // test if source file exists and destination does not
                    if (fs.existsSync(sourceRom) && !fs.existsSync(destRom)) {
                        // copy rom
                        fs.copy(sourceRom, destRom, (err) => {
                            if (err) throw err;
    
                            // copy CHD
                            let sourceChd = path.join(romset, game);
                            if (fs.existsSync(sourceChd)) {
                                fs.copy(sourceChd, path.join(selection, game), (err) => {
                                    if (err) throw err;
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
                this.emit('done.add');
                callback();
            });
        });
    }

    /**
     * Removes roms from a folder,
     * based on a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} selection The path to the selection folder
     * @param {functiion} callback The callback method
     */
    remove (file, selection, callback) {
        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;

            let fileCsv = csvparse(
                fileContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });

            for (let i = 0; i < fileCsv.length; i++) {
                let zip = fileCsv[i].name + '.zip';
                let rom = path.join(selection, zip);

                this.emit('progress.remove', fileCsv.length, i + 1, zip);

                // test if rom exists
                fs.pathExists(rom, (err, romExists) => {
                    if (romExists) {
                        // delete rom
                        fs.remove(rom, (err) => {
                            console.log('%s deleted', rom);
                            if (i + 1 >= fileCsv.length) { callback(); }
                        });
                    } else {
                        if (i + 1 >= fileCsv.length) { callback(); }
                    }
                });
            }
        });
    }

    /**
     * Keeps only listed roms in a folder
     * that are listed in a CSV file
     * 
     * @param {string} file The path to the file
     * @param {string} selection The path to the selection folder
     * @param {functiion} callback The callback method
     */
    keep (file, selection, callback) {
        fs.readFile(file, { 'encoding': 'utf8' }, (err, fileContents) => {
            if (err) throw err;
            
            let fileCsv = csvparse(
                fileContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });

            // list files in selection folder
            var files = fs.readdirSync(selection);
            for (let i = 0; i < files.length; i++) {
                let zip = files[i];

                this.emit('progress.keep', files.length, i + 1, zip);

                // skip non-zip files
                if (!zip.endsWith('.zip')) {
                    if (i + 1 >= files.length) { callback(); }
                    continue;
                }

                // file not found in csv -> remove it
                let csvItem = fileCsv.find((item) => item.name === zip.replace('.zip', ''));
                if (typeof secondaryItem === 'undefined') {
                    console.log('remove %s', zip);
                    fs.unlinkSync(path.join(selection, zip));
                }
            }
        });
    }
};