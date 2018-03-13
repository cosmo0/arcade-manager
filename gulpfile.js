const gulp = require('gulp');
const path = require('path');
const cproc = require('child_process');
const fs = require('fs-extra');
const delEmpty = require('delete-empty');

// variable definition
const src = path.resolve('src');
const srcout = path.resolve('out/src');
const isWin = process.platform === 'win32';

// default task
gulp.task('default', [ 'cleanup', 'prepare', 'build', 'cleanupapp', 'packageapp', 'packagesetup' ]);

// remove and re-create out folder
gulp.task('cleanup', function(done) {
    // TODO

    done();
});

// copy all files from src to out
gulp.task('prepare', [ 'cleanup' ], function(done) {
    // TODO
    
    done();
});

// build the app in out
gulp.task('build', [ 'prepare' ], function(done) {
    cproc.exec('npm install --production',
        { cwd: srcout },
        function (err, stdout, stderr) {
            console.log(stderr);
            if (err) throw err;

            console.log(stdout);
            
            done();
        });
});

// Cleanup app output folder
gulp.task('cleanupapp', [ 'build' ], function(done) {
    fs.walk(path.join(srcout, 'node_modules'))
    .on('data', function (item) {
        
        // normalize folder paths
        var f = item.path.replace('/', '\\');

        if (f.match(/\\.bin\\/i) ||
            // readme etc
            f.match(/\.md$/i) ||
            f.match(/author[s]?/i) ||
            f.match(/copying\.txt$/i) ||
            f.match(/example\.(html|js)/i) ||
            // build files
            f.match(/bower\.json$/i) ||
            f.match(/gruntfile\.js$/i) ||
            f.match(/makefile$/i) ||
            f.match(/cakefile$/i) ||
            // misc files
            f.match(/jsl\.node\.conf$/i) ||
            f.match(/gen-doc\.sh/i) ||
            f.match(/coverage\.html/i) ||
            f.match(/codecov\.yml/i) ||
            // minimized files
            f.match(/\.min\.js$/i) ||
            // files starting with dot
            f.match(/^\./i) ||
            // files ending with tilde
            f.match(/~$/i) ||
            // "src" folders, inside src
            f.match(/\\src\\(^node_modules)\\/i) ||
            // misc folders
            f.match(/\\t[e]?st[s]?\\/i) ||
            f.match(/\\coverage\\/i) ||
            f.match(/\\bench(mark)?\\/i) ||
            f.match(/\\build\\/i) ||
            f.match(/\\example[s]?\\/i) ||
            f.match(/\\sample[s]?\\/i) ||
            f.match(/\\images\\/i) ||
            f.match(/\\screenshots\\/i) ||
            f.match(/\\doc[s]?\\/i) ||
            f.match(/\\man\\/i)) {

            try {
                fs.unlinkSync(f);
            } catch (errRemove) {
                console.error('File ' + f + ' could not be deleted :' + errRemove);

                // ENOENT : entry does not exist anymore : probably means the folder has been deleted already.
                // EPERM : no permission to delete : probably means that the antivirus is locking the file or something. No matter.
                if (errRemove.code !== 'ENOENT' && errRemove.code !== 'EPERM') {
                    throw errRemove;
                }
            }
        }
    })
    .on('end', function () {
        // remove empty folders
        delEmpty.sync(srcout, { force: true });

        done();
    });
});

// package the app before publication
gulp.task('packageapp', [ 'cleanupapp' ], function(done) {
    if (isWin) {
        copyElectron('win64', done);
    } else {
        copyElectron('osx64', done);
        
        packageAppMac();
    }
});

// create a zip/dmg
gulp.task('zip', [ 'packageapp' ], function(done) {
    // TODO

    done();
});

/**
 * Copies the Electron files
 * 
 * @param {string} buildPlatform: the platform (win64, osx64)
 * @param {function} done: the callback
 */
function copyElectron(buildPlatform, done) {
    var nwOutDir = path.join(buildout, buildPlatform);

    // TODO: copy Electron app to out
    
    // rename app folder on mac
    if (!isWin) {
        fs.renameSync(path.join(nwOutDir, "electron.app"), path.join(nwOutDir, "retropie-arcade.app"));
    }

    // copy src files
    var packageDir = isWin ? path.join(nwOutDir, 'package.nw') : path.join(nwOutDir, 'retropie-arcade.app/Contents/Resources/app.nw');
    fs.mkdirsSync(packageDir);

    fs.copySync(srcout, packageDir);
    
    done();
}

/**
 * Packages the app for Mac in a DMG file
 * 
 * @param {function} done: the callback
 */
function packageAppMac(done) {
    var appPath = path.join(buildout, 'retropie-arcade/osx64/retropie-arcade.app');
    var outPath = path.join(buildout, 'retropie-arcade');

    // app icon
    fs.copySync(path.join(setup, 'mac/app.icns'), path.join(appPath, 'Contents/Resources/app.icns'));
    fs.copySync(path.join(setup, 'mac/app.icns'), path.join(appPath, 'Contents/Resources/document.icns'));

    // app plist file
    fs.copySync(path.join(setup, 'mac/Info.plist'), path.join(appPath, 'Contents/Info.plist'));

    // package plist file (equivalent to pkgbuild --analyze --root osx64 retropie-arcade.plist)
    fs.copySync(path.join(setup, 'mac/retropie-arcade.plist'), path.join(outPath, 'retropie-arcade.plist'));
    
    // distribution.xml file (equivalent to productbuild --synthesize --package retropie-arcade.pkg distribution.xml)
    fs.copySync(path.join(setup, 'mac/distribution.xml'), path.join(outPath, 'distribution.xml'));

    // TODO: create DMG

    // TODO? sign the app
    // codesign -s "Developer ID Application: xxxx (xxxx)
    // TODO? create pkg for app
    // pkgbuild --sign "Developer ID Installer: xxx (xxx)" --root "osx64" --component-plist "retropie-arcade.plist" --install-location "/Applications/" --identifier "com.retropie-arcade" --version "4.4" "retropie-arcade.pkg"
    // TODO? create distribution package
    // productbuild --sign "Developer ID Installer: xxx (xxx)" --distribution distribution.xml retropie-arcade_macOS_' + appinfos.fileVersion + '.pkg'
}

/**
 * Executes a command
 * 
 * @param {string} command: the command to execute
 * @param {string} cwd: the current working directory
 * @param {function} callback: the callback
 */
function exec(command, cwd, callback) {
    if (typeof cwd === "function") {
        callback = cwd;
        cwd = "";
    }

    cproc.exec(command, { cwd: cwd, maxBuffer: (5000 * 1024) }, function(err, stdout, stderr) {
        console.log(stdout);
        
        if (stderr) {
            console.error(stderr);
        }

        if (err) {
            console.error(err);
            throw err;
        }

        if (callback && typeof callback === "function") {
            callback();
        }
    });
}