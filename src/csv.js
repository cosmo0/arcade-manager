const fs = require('fs-extra');
const path = require('path');
const events = require('events');
const csvparse = require('csv-parse/lib/sync');
const stringify = require('csv-stringify/lib/sync');
const parseString = require('xml2js').parseString;
const ini = require('ini');
const sanitize = require("sanitize-filename");

// delimiter
const defaultDelimiter = ';';

module.exports = class Csv extends events {
    /**
     * Parses the provided content and returns a CSV file object
     * 
     * @param {any} content The content to parse
     * @returns {object} The parsed CSV object
     */
    parse(content) {
        let firstline = content.split('\n', 1)[0];

        // check if first line is "name", or has a "name" column header
        if (firstline.indexOf(';')
            && (
                firstline.indexOf('name;') === 0
                || firstline.indexOf(';name;') > 0
            )
            || firstline === 'name') {

            return csvparse(
                content,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });
        } else if (firstline.indexOf(';') < 0) {
            // first line does not have a separator, it's probably just a list of names
            return csvparse(
                content,
                {
                    columns: [ 'name' ],
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });
        } else {
            // first line has separators but no "name" column: error
            throw 'If your CSV file has several columns, it must contain a "name" column';
        }
    }

    /**
     * Combines roms from the main and secondary CSV files,
     * and saves the result in the target file.
     * 
     * @param {string} main The path to the main CSV file
     * @param {string} secondary The path to the secondary CSV file
     * @param {string} target The path to the combined file to save
     */
    add (main, secondary, target) {
        this.emit('start.add');

        // load up csv files
        fs.readFile(main, { 'encoding': 'utf8' }, (err, mainContents) => {
            if (err) throw err;

            let mainCsv = this.parse(mainContents);

            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                if (err) throw err;

                let secondaryCsv = this.parse(secondaryContents);

                console.log('Merging main (%i lines) and secondary (%i lines)', mainCsv.length, secondaryCsv.length);
                this.emit('log', 'Main file has ' + mainCsv.length + ' games');
                this.emit('log', 'Secondary file has' + secondaryCsv.length + ' games');

                // read the secondary csv and add entries to main that do not yet exist
                let requests = secondaryCsv.reduce((promisechain, line, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        this.emit('progress.add', secondaryCsv.length, index + 1, line.name);

                        let mainItem = mainCsv.find((item) => item.name === line.name);
                        // if no matching main CSV entry is found: add it
                        if (typeof mainItem === 'undefined') {
                            mainCsv.push(line);
                        }

                        resolve();
                    }));
                }, Promise.resolve());

                requests.then(() => {
                    console.log('Result has %i lines; saving it', mainCsv.length);
                    this.emit('log', 'Result file has ' + mainCsv.length + ' games');

                    // save the result
                    fs.writeFile(target, stringify(mainCsv, { header: true, delimiter: defaultDelimiter }), (err) => {
                        if (err) throw err;

                        this.emit('end.add', target);
                        console.log('OK');
                    });
                });
            });
        });
    }

    /**
     * Removes roms in the main file that are listed in the
     * secondary file, and saves the result in the target file.
     * 
     * @param {string} main The path to the main CSV file
     * @param {string} secondary The path to the secondary CSV file
     * @param {string} target The path to the combined file to save
     */
    remove (main, secondary, target) {
        this.emit('start.remove');

        // load up csv files
        fs.readFile(main, { 'encoding': 'utf8' }, (err, mainContents) => {
            let mainCsv = this.parse(mainContents);
            
            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                let secondaryCsv = this.parse(secondaryContents);

                console.log('Removing lines from main (%i lines) that exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);
                this.emit('log', 'Main file has ' + mainCsv.length + ' games');
                this.emit('log', 'Secondary file has' + secondaryCsv.length + ' games');

                let merged = [];

                // read the main csv and add entries to merge that do not exist in secondary
                let requests = mainCsv.reduce((promisechain, line, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        this.emit('progress.remove', mainCsv.length, index + 1, line.name);

                        let secondaryItem = secondaryCsv.find((item) => item.name === line.name);
                        // if no matching main CSV entry is found: add to merge
                        if (typeof secondaryItem === 'undefined') {
                            merged.push(line);
                        }

                        resolve();
                    }));
                }, Promise.resolve());

                requests.then(() => {
                    console.log('Result has %i lines; saving it', merged.length);
                    this.emit('log', 'Result file has ' + merged.length + ' games');

                    // save the result
                    fs.writeFile(target, stringify(merged, { header: true, delimiter: defaultDelimiter }), (err) => {
                        this.emit('end.remove', target);
                        console.log('OK');
                    });                        
                });
            });
        });
    }

    /**
     * Only keeps roms in the main file that are listed in the
     * secondary file, and saves the result in the target file.
     * 
     * @param {string} main The path to the main CSV file
     * @param {string} secondary The path to the secondary CSV file
     * @param {string} target The path to the combined file to save
     */
    keep (main, secondary, target) {
        this.emit('start.keep');

        // load up csv files
        fs.readFile(main, { 'encoding': 'utf8' }, (err, mainContents) => {
            let mainCsv = this.parse(mainContents);

            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                let secondaryCsv = this.parse(secondaryContents);

                console.log('Removing lines from main (%i lines) that do not exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);
                this.emit('log', 'Main file has ' + mainCsv.length + ' games');
                this.emit('log', 'Secondary file has' + secondaryCsv.length + ' games');

                let merged = [];

                // read the main csv and add entries to merge that do not exist in secondary
                let requests = mainCsv.reduce((promisechain, line, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        this.emit('progress.keep', mainCsv.length, index + 1, line.name);

                        let secondaryItem = secondaryCsv.find((item) => item.name === line.name);
                        // if a matching main CSV entry is found: add to merge
                        if (typeof secondaryItem !== 'undefined') {
                            merged.push(line);
                        }

                        resolve();
                    }));
                }, Promise.resolve());

                requests.then(() => {
                    console.log('Result has %i lines; saving it', merged.length);
                    this.emit('log', 'Result file has ' + merged.length + ' lines');

                    // save the result
                    fs.writeFile(target, stringify(merged, { header: true, delimiter: defaultDelimiter }), (err) => {
                        this.emit('end.keep', target);
                        console.log('OK');
                    });
                });
            });
        });
    }

    /**
     * Converts a DAT (XML) file to CSV
     * 
     * @param {any} dat The path to the DAT file to convert
     * @param {any} target The path to the target CSV file
     */
    convertdat (dat, target) {
        this.emit('start.convert');

        this.emit('progress.convert', 100, 1, 'DAT file');

        // read DAT file
        fs.readFile(dat, { 'encoding': 'utf8' }, (err, datContents) => {
            if (err) throw err;

            // check that it's an XML file and it looks like something expected
            if (!datContents.startsWith('<?xml')) {
                throw 'Unable to read the DAT file, expected to start with <?xml version="1.0"?>';
            }
            
            parseString(datContents, (err, datXml) => {
                if (err) throw err;

                if (!datXml.datafile || !datXml.datafile.game) {
                    throw 'DAT file needs to be in the CLR MAME PRO format';
                }

                this.emit('log', 'DAT file has ' + datXml.datafile.game.length + ' games');

                // create a file handler to write into
                let stream = fs.createWriteStream(target, { 'encoding': 'utf8' });

                // write the header
                stream.write('name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sampleof\n');

                // for each game in the DAT, write a line in the CSV
                let requests = datXml.datafile.game.reduce((promisechain, game, index) => {
                    return promisechain.then(() => new Promise((resolve) => {
                        this.emit('progress.convert', datXml.datafile.game.length, index + 1, game.$.name);

                        let line = '';
                        line += game.$.name + ';';
                        line += '"' + (game.description ? game.description[0] : '').replace(';', '-').replace('"', '') + '";';
                        line += (game.year ? game.year[0] : '') + ';';
                        line += '"' + (game.manufacturer ? game.manufacturer[0] : '') + '";';
                        
                        line += (game.$.cloneof ? 'NO' : 'YES') + ';'; // is_parent
                        line += (game.$.romof || '-') + ';'; // romof
                        line += (game.$.cloneof ? 'YES' : 'NO') + ';'; // is_clone
                        line += (game.$.cloneof || '-') + ';'; // cloneof
                        line += (game.$.sampleof || '-'); // sampleof

                        line += '\n';

                        stream.write(line, () => {
                            resolve();
                        });
                    }));
                }, Promise.resolve());

                requests.then(() => {
                    console.log('done');
                    this.emit('end.convert', target);
                    
                    // close the file handler
                    stream.end();
                });
            });
        });
    }

    /**
     * Converts a INI file to multiple CSV
     * 
     * @param {any} inifile The path to the INI file to convert
     * @param {any} target The path to the target folder
     */
    convertini (inifile, target) {
        this.emit('start.convert');

        this.emit('progress.convert', 100, 1, 'INI file');

        // read INI file
        fs.readFile(inifile, { 'encoding': 'utf8' }, (err, iniContents) => {
            if (err) throw err;

            // parse INI
            let iniValues = ini.parse(iniContents);
            let keys = Object.keys(iniValues).filter(v => v !== 'FOLDER_SETTINGS');

            let requests = keys.reduce((promisechain, section, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    this.emit('progress.convert', keys.length, index + 1, section);

                    let sectionValues = iniValues[section];
                    let linekeys = Object.keys(sectionValues);

                    // empty section
                    if (linekeys.length === 0) { resolve(); return; }
                    
                    this.emit('log', 'INI section "' + section + '" with ' + linekeys.length + ' games');

                    // CSV header
                    let result = 'name;value\n';

                    // build the CSV lines
                    for (let linekey of linekeys) {
                        result += linekey + ';';
                        
                        // just the game name without any value is parsed as "game:true"
                        result += sectionValues[linekey] === true ? '' : sectionValues[linekey];
                        result += '\n';
                    }

                    // determine file name (remove special chars)
                    let fileName = keys.length > 1 ? sanitize(section) : path.basename(inifile).replace('.ini', '');
                    if (fileName === '') { fileName = 'unknown'; }
                    fileName += '.csv';

                    // make sure not to overwrite existing conversions
                    if (fs.existsSync(path.join(target, fileName))) {
                        let fileIndex = 1;
                        while (fs.existsSync(path.join(target, fileName.replace('.csv', fileIndex + '.csv')))) {
                            fileIndex++;
                        }

                        fileName = fileName.replace('.csv', fileIndex + '.csv');
                    }

                    // write the file and resolve
                    fs.writeFile(path.join(target, fileName), result, { flag: 'wx' }, (err) => {
                        if (err) throw err;

                        this.emit('log', 'Result file saved as ' + fileName);

                        resolve();
                    });
                }));
            }, Promise.resolve());

            requests.then(() => {
                console.log('done');
                this.emit('end.convert', target);
            });
        });
    }

    /**
     * Creates a "CSV" from the files in a folder
     * 
     * @param {any} folder The folder to list the files from
     * @param {any} target The target CSV file
     */
    listfiles (folder, target) {
        this.emit('start.listfiles');

        // get files list
        fs.readdir(folder, (err, filesList) => {
            if (err) throw err;

            // create a file handler to write into
            let stream = fs.createWriteStream(target, { 'encoding': 'utf8' });

            // write the header
            stream.write('name;\n');

            // for each zip file, write a line in the CSV
            let requests = filesList.reduce((promisechain, game, index) => {
                return promisechain.then(() => new Promise((resolve) => {
                    this.emit('progress.listfiles', filesList.length, index + 1, game);

                    // only list zip files
                    if (game.endsWith('.zip')) {
                        stream.write(game.replace('.zip', '') + ';\n', () => {
                            resolve();
                        });
                    } else {
                        resolve();
                    }
                }));
            }, Promise.resolve());

            requests.then(() => {
                console.log('done');
                this.emit('log', 'Result file has ' + filesList.length + ' games');

                this.emit('end.listfiles', target);
                
                // close the file handler
                stream.end();
            });
        });
    }
};