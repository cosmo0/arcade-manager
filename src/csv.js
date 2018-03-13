const fs = require('fs');
const path = require('path');
const csvparse = require('csv-parse/lib/sync');
const stringify = require('csv-stringify/lib/sync');

/*
* Expected CSV columns from ArcadeItalia:
*
    name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;
    sasampleof;is_runnable;is_device;is_bios;biosof;is_mechanical;is_arcade;
    use_chds;savestate;source_file;first_emulator;first_emulator_date;
    last_emulator;last_emulator_date;players;buttons;coins;sound_channels;
    screens;display_type;display_rotate;screen_orientation;display_width;
    display_height;display_refresh;input_controls;monitor_type;chips_cpu;
    chips_cpu_details;chips_audio;chips_audio_details;driver_status;
    driver_emulation;driver_color;driver_sound;driver_graphic;driver_cocktail;
    driver_protection;dump_status;device_types;category;genre;serie;language;
    colors;has_device_refs;has_roms;has_disks;has_dip_switches;has_bios_sets;
    has_configs;has_ports;has_devices;has_slots;has_adjusters;has_soft_lists;
    has_ram_opts;has_cheats;nplayers;url_playonline;url_shortplays;
    url_shortplay_ms;game_rate;mature;on_top_score;on_bacheca_record;
    on_arcaworld;ranking;bestgame;alltime;cabinets;
*/

// delimiter
const defaultDelimiter = ';';

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

        console.log('OK');
    },

    /**
     * Removes roms in the main file that are listed in the
     * secondary file, and saves the result in the target file.
     * 
     * @param {string} main The path to the main CSV file
     * @param {string} secondary The path to the secondary CSV file
     * @param {string} target The path to the combined file to save
     */
    remove: function remove (main, secondary, target) {

    }
};