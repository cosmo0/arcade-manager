const fs = require('fs-extra');
const path = require('path');
const http = require('http');
const https = require('https');

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
    }
};