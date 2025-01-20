# Arcade manager for Retropie & Recalbox

[![Windows build status](https://ci.appveyor.com/api/projects/status/80a8164snm8dxqb5?svg=true)](https://ci.appveyor.com/project/cosmo0/arcade-manager-dotnet-win)
[![Linux build status](https://ci.appveyor.com/api/projects/status/ltp88yfy8b22y8sv?svg=true)](https://ci.appveyor.com/project/cosmo0/arcade-manager-dotnet-linux)
[![MacOS build status](https://ci.appveyor.com/api/projects/status/jxkja8m43yjjcdo8?svg=true)](https://ci.appveyor.com/project/cosmo0/arcade-manager-dotnet-mac)

This repository provides a rom management tool, specifically designed for use with Retropie & Recalbox, but it works with anything.

It helps you to manage and filter your arcade rom collection (MAME/FBNeo) by removing unwanted, bad, and unplayable games.

It's also useful if you feel a bit lost among the several thousand games that full MAME romsets provides, and only want the very best.

## Features

**THIS TOOL DOES NOT CHANGE THE ROMSET VERSION!!!**

Copying files assumes you have a working, **non-merged** romset corresponding to the MAME/FBA version you're using. If you ever need to change your romset version, use a tool like [ClrMamePro](https://mamedev.emulab.it/clrmamepro/).

* Multi-platform, works on Windows, MacOS and Linux
* User-friendly, easy to use interface
* Wizard with pre-built lists of games
* Download and install an overlays pack (Retropie or Recalbox)
* Manage rom files: copy or cleanup a selection of roms
* Manage games lists: download pre-built files, merge and split files, convert DAT or INI files, and more
* Included help

Obviously, this tool does not download roms.

## Usage

Launch ArcadeManager on a computer running Windows, MacOS or Linux. If you're using a Rapsberry Pi for emulation (or another computer), it can connect to it using network shares (but you should consider using a USB key for rom storage).

## External help

If you are new to arcade emulation, please read these resources:

* [Arcade roms and how to play them, a non-technical guide](https://retropie.org.uk/forum/topic/7247/)
* [Demistifying MAME roms](https://choccyhobnob.com/mame/demystifying-mame-roms/)

## Screenshots

![Home](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-home.png)
![Wizard](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-wizard.png)
![Install overlays](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-overlay-download.png)
![Download CSV](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-csv-download.png)
![Copy roms](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-rom-copy.png)

## Help with translations

Anyone can help with translations: they're simple text files (INI style) located in `Data\translations`.

If you think you can help, please create a pull request!

## Development

Prerequisites:

* Dotnet 8 SDK
* NodeJS 22.x
* `dotnet tool install --global ElectronNET.CLI`

Build and run:

````bash
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
