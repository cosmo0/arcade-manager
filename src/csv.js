const fs = require('fs');
const path = require('path');
const csvparse = require('csv-parse/lib/sync');

module.exports = {
    /**
     * Combines roms from the main and secondary CSV files,
     * and saves the result in the target file.
     * 
     * @param {string} main The path to the main CSV file
     * @param {string} secondary The path to the secondary CSV file
     * @param {string} target The path to the combined file to save
     */
    add: function add (main, secondary, target) {
        let merge = [];
        let mainCsv = csvparse(main, {
            auto_parse: false,
            auto_parse_date: false,
            delimiter: ";"
        });
        console.log(mainCsv);
    },

    remove: function remove (main, secondary, target) {

    }
};