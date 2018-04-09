const fs = require('fs-extra');
const path = require('path');
const events = require('events');
const csvparse = require('csv-parse/lib/sync');
const stringify = require('csv-stringify/lib/sync');

// delimiter
const defaultDelimiter = ';';

module.exports = class Csv extends events {
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

            let mainCsv = csvparse(
                mainContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });

            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                if (err) throw err;

                let secondaryCsv = csvparse(
                    secondaryContents,
                    {
                        columns: true,
                        auto_parse: false,
                        auto_parse_date: false,
                        delimiter: defaultDelimiter
                    });

                console.log('Merging main (%i lines) and secondary (%i lines)', mainCsv.length, secondaryCsv.length);

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

                    // save the result
                    fs.writeFile(target, stringify(mainCsv, { header: true, delimiter: defaultDelimiter }), (err) => {
                        if (err) throw err;

                        this.emit('end.add');
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
            let mainCsv = csvparse(
                mainContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });
            
            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                let secondaryCsv = csvparse(
                    secondaryContents,
                    {
                        columns: true,
                        auto_parse: false,
                        auto_parse_date: false,
                        delimiter: defaultDelimiter
                    });

                console.log('Removing lines from main (%i lines) that exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);

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

                    // save the result
                    fs.writeFile(target, stringify(merged, { header: true, delimiter: defaultDelimiter }), (err) => {
                        this.emit('end.remove');
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
            let mainCsv = csvparse(
                mainContents,
                {
                    columns: true,
                    auto_parse: false,
                    auto_parse_date: false,
                    delimiter: defaultDelimiter
                });

            fs.readFile(secondary, { 'encoding': 'utf8' }, (err, secondaryContents) => {
                let secondaryCsv = csvparse(
                    secondaryContents,
                    {
                        columns: true,
                        auto_parse: false,
                        auto_parse_date: false,
                        delimiter: defaultDelimiter
                    });

                console.log('Removing lines from main (%i lines) that exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);

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

                    // save the result
                    fs.writeFile(target, stringify(merged, { header: true, delimiter: defaultDelimiter }), (err) => {
                        this.emit('end.keep');
                        console.log('OK');
                    });
                });
            });
        });
    }
};