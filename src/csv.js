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
        let mainCsv = csvparse(
            fs.readFileSync(main),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });
        let secondaryCsv = csvparse(
            fs.readFileSync(secondary),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });

        console.log('Merging main (%i lines) and secondary (%i lines)', mainCsv.length, secondaryCsv.length);

        // read the secondary csv and add entries to main that do not yet exist
        for (let i = 0; i < secondaryCsv.length; i++) {
            this.emit('progress.add', secondaryCsv.length, i + 1, secondaryCsv[i].name);

            let mainItem = mainCsv.find((item) => item.name === secondaryCsv[i].name);
            // if no matching main CSV entry is found: add it
            if (typeof mainItem === 'undefined') {
                mainCsv.push(secondaryCsv[i]);
            }
        }

        console.log('Result has %i lines; saving it', mainCsv.length);

        // save the result
        fs.writeFileSync(
            target,
            stringify(
                mainCsv,
                {
                    header: true,
                    delimiter: defaultDelimiter
                })
            );

        this.emit('end.add');
        console.log('OK');
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
        let mainCsv = csvparse(
            fs.readFileSync(main),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });
        let secondaryCsv = csvparse(
            fs.readFileSync(secondary),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });

        console.log('Removing lines from main (%i lines) that exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);

        let merged = [];

        // read the main csv and add entries to merge that do not exist in secondary
        for (let i = 0; i < mainCsv.length; i++) {
            this.emit('progress.remove', mainCsv.length, i + 1, mainCsv[i].name);

            let secondaryItem = secondaryCsv.find((item) => item.name === mainCsv[i].name);
            // if no matching main CSV entry is found: add to merge
            if (typeof secondaryItem === 'undefined') {
                merged.push(mainCsv[i]);
            }
        }

        console.log('Result has %i lines; saving it', merged.length);

        // save the result
        fs.writeFileSync(
            target,
            stringify(
                merged,
                {
                    header: true,
                    delimiter: defaultDelimiter
                })
            );

        this.emit('end.remove');
        console.log('OK');
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
        let mainCsv = csvparse(
            fs.readFileSync(main),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });
        let secondaryCsv = csvparse(
            fs.readFileSync(secondary),
            {
                columns: true,
                auto_parse: false,
                auto_parse_date: false,
                delimiter: defaultDelimiter
            });

        console.log('Removing lines from main (%i lines) that exist in secondary (%i lines)', mainCsv.length, secondaryCsv.length);

        let merged = [];

        // read the main csv and add entries to merge that do not exist in secondary
        for (let i = 0; i < mainCsv.length; i++) {
            this.emit('progress.keep', mainCsv.length, i + 1, mainCsv[i].name);

            let secondaryItem = secondaryCsv.find((item) => item.name === mainCsv[i].name);
            // if a matching main CSV entry is found: add to merge
            if (typeof secondaryItem !== 'undefined') {
                merged.push(mainCsv[i]);
            }
        }

        console.log('Result has %i lines; saving it', merged.length);

        // save the result
        fs.writeFileSync(
            target,
            stringify(
                merged,
                {
                    header: true,
                    delimiter: defaultDelimiter
                })
            );

        this.emit('end.keep');
        console.log('OK');
    }
};