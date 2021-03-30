const electron = require('electron');
const {shell} = electron;
const {app, dialog} = electron.remote;
const https = require('follow-redirects').https;
const events = require('events');
const settings = require('electron-settings');

const protocol = 'https:';
const api = 'api.github.com';

module.exports = class Updater extends events {
    /**
     * Checks for updates and notifies the user if any is found
     */
    checkUpdate() {
        // get all tags from repository
        let url = '/repos/cosmo0/arcade-manager/tags';
        https.get({ protocol, 'host': api, 'path': url, 'headers': { 'User-Agent': 'arcade-manager' } }, (res) => {
            res.setEncoding('utf8');
            let rawData = '';
            res.on('data', (chunk) => { rawData += chunk; });
            res.on('end', () => {
                // parse version number
                let data = JSON.parse(rawData);
                let latest = data[0].name.replace('v', '');
                console.log('latest version: %s', latest);

                // version has been ignored
                if (settings.hasSync('ignoreVersion') && settings.getSync('ignoreVersion') === latest) {
                    console.log('Version has been ignored: %s', latest);
                    return;
                }

                // if a newer version is available, ask user to download it
                if (this.compare(latest, app.getVersion()) > 0) {
                    console.log('newer version available: %s', latest);
                    dialog.showMessageBox({
                        'type': 'question',
                        'message': 'A newer version is available : ' + latest + '.'
                            + '\nYou are currently using ' + app.getVersion() + '.'
                            + '\nDo you want to download it?',
                        'buttons': [ 'Yes', 'No' ],
                        'defaultId': 0,
                        'cancelId': 1,
                        'checkboxLabel': 'Do not notify me again for this version'
                    }, (response, checked) => {
                        if (checked) { settings.setSync('ignoreVersion', latest); }

                        // user has clicked yes
                        if (response === 0) {
                            shell.openExternal('https://github.com/cosmo0/arcade-manager/releases');
                        }
                    });
                } else {
                    console.log('no newer version');
                }
            });
        });
    }

    /**
     * Compares two versions numbers
     * See: https://stackoverflow.com/a/6832706/
     * 
     * @param {any} a The first version
     * @param {any} b The second version
     * @returns 1 if a > b ; -1 if a < b ; 0 if a == b
     */
    compare(a, b) {
        if (a === b) { return 0; }
    
        var a_components = a.split('.');
        var b_components = b.split('.');
    
        var len = Math.min(a_components.length, b_components.length);
    
        // loop while the components are equal
        for (var i = 0; i < len; i++) {
            // A bigger than B
            if (parseInt(a_components[i]) > parseInt(b_components[i])) {
                return 1;
            }
    
            // B bigger than A
            if (parseInt(a_components[i]) < parseInt(b_components[i])) {
                return -1;
            }
        }
    
        // If one's a prefix of the other, the longer one is greater.
        if (a_components.length > b_components.length) {
            return 1;
        }
    
        if (a_components.length < b_components.length) {
            return -1;
        }
    
        // Otherwise they are the same.
        return 0;
    }
};