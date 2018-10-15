# Arcade manager for Retropie & Recalbox

[![Windows/Linux Build status](https://ci.appveyor.com/api/projects/status/npgiar9ncapx2al4?svg=true)](https://ci.appveyor.com/project/cosmo0/arcade-manager)

This repository provides a rom management tool, specifically designed for use with Retropie & Recalbox.  
It helps you to manage and filter your rom collection by removing unwanted, bad, and unplayable games.  
It's also useful if you feel a bit lost among the several thousand games that full MAME romsets provides, and only want the very best.

## Features

**THIS TOOL DOES NOT CHANGE THE ROMSET VERSION!!!**  
Copying files assumes you have a working, **non-merged** romset corresponding to the MAME/FBA version you're using.  
If you ever need to change your romset version, use a tool like [ClrMamePro](https://mamedev.emulab.it/clrmamepro/).

* Multi-platform, works on Windows, MacOS and Linux
* User-friendly, easy to use interface
* Download and install an overlays pack for either Retropie or Recalbox
* Manage rom files:
  * Copy from romset
  * Remove unwanted files
  * Keep only wanted files
* Manage games lists:
  * Download pre-set files
  * Merge & split files
  * Convert DAT or INI files
  * Create games list from folder

Obviously, this tool does not download roms.

If you are new to arcade emulation, please read these resources:

* [Arcade roms and how to play them, a non-technical guide](https://retropie.org.uk/forum/topic/7247/)
* [Demistifying MAME roms](https://choccyhobnob.com/mame/demystifying-mame-roms/)
* [How to use MAME with Retropie](https://retropie.org.uk/forum/topic/2859/)
* [Validating, rebuilding and filtering ROM collections](https://github.com/RetroPie/RetroPie-Setup/wiki/Validating,-Rebuilding,-and-Filtering-ROM-Collections)
* [Which arcade emulator should I choose?](https://www.reddit.com/r/RetroPie/comments/6v86nd/what_rom_set_works_best_with_mame/dlyhccz/)
* [FBA vs MAME](https://retropie.org.uk/forum/topic/13769/)

## Screenshots

![Home](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-home.png)
![Install overlays](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-overlay-download.png)
![Download CSV](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-csv-download.png)
![Copy roms](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-rom-copy.png)

## Development

* Run the app locally: `npm start`
* Create a runnable package: `npm run pack` for current platform, or `npm run pack -- --mac --win --linux --x64` for all platforms; note you will have to pick at most 2 platforms, you can't build for Mac on Windows or for Windows on Mac
* Create a package: `npm run dist -- --mac --win --linux --x64`