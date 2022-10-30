# Arcade manager for Retropie & Recalbox

[![Windows Build status](https://ci.appveyor.com/api/projects/status/80a8164snm8dxqb5?svg=true&passingText=Windows&pendingText=Windows&failingText=Windows)](https://ci.appveyor.com/project/cosmo0/arcade-manager-win)
[![Linux Build status](https://ci.appveyor.com/api/projects/status/ltp88yfy8b22y8sv?svg=true&passingText=Linux&pendingText=Linux&failingText=Linux)](https://ci.appveyor.com/project/cosmo0/arcade-manager-linux)
[![MacOS Build status](https://ci.appveyor.com/api/projects/status/jxkja8m43yjjcdo8?svg=true&passingText=MacOS&pendingText=MacOS&failingText=MacOS)](https://ci.appveyor.com/project/cosmo0/arcade-manager-mac)

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

## Usage

Launch ArcadeManager on a computer running Windows, MacOS or Linux. If you're using a Rapsberry Pi for emulation (or another computer), it will connect to it using network shares.

ArcadeManager includes help pages: click on the "help" link at the bottom of the main screen. Let me know if you have any trouble using it.

## External help

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

Build and run:

````bash
dotnet tool install --global ElectronNET.CLI
cd ArcadeManager
dotnet build
electronize start
````

Then attach Visual Studio to the `ArcadeManager` process.

Generate a fake romset in `tmp\roms` (empty zip files with the right names):

````powershell
.\generate-samples.ps1
````

Create an install package:

````bash
electronize build /target win
electronize build /target osx
electronize build /target linux
electronize build /target win /electron-arch arm64
electronize build /target osx /electron-arch arm64
electronize build /target linux /electron-arch arm64
````
