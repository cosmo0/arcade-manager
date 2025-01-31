# Arcade manager for Retropie & Recalbox

[![ArcadeManager Windows](https://github.com/cosmo0/arcade-manager/actions/workflows/win.yml/badge.svg)](https://github.com/cosmo0/arcade-manager/actions/workflows/win.yml)
[![ArcadeManager Linux](https://github.com/cosmo0/arcade-manager/actions/workflows/linux.yml/badge.svg)](https://github.com/cosmo0/arcade-manager/actions/workflows/linux.yml)
[![ArcadeManager MacOS](https://github.com/cosmo0/arcade-manager/actions/workflows/mac.yml/badge.svg)](https://github.com/cosmo0/arcade-manager/actions/workflows/mac.yml)

This is a rom management tool that will work with any arcade emulator romset. It helps you filter your arcade rom collection (MAME/FBNeo) by removing unwanted, bad, and unplayable games.

It also includes several selection of "best games", if you feel a bit lost among the several thousand games that full MAME romsets provide.

It also installs overlays/bezels, but only Retropie and Recalbox are currently supported.

## Download

The latest version is available here: <https://github.com/cosmo0/arcade-manager/releases/>

## Features

**⚠ THIS TOOL DOES NOT CHANGE THE ROMSET VERSION ⚠**

Copying files assumes you have a working, **non-merged** romset corresponding to the MAME/FBNeo version you're using. If you ever need to change your romset version, use a tool like [ClrMamePro](https://mamedev.emulab.it/clrmamepro/).

* Multi-platform, works on Windows, MacOS and Linux
* User-friendly, easy to use interface
* Wizard with pre-built lists of "best games"
* Manage rom files: copy or cleanup a selection of roms
* Download and install an overlays pack (Retropie or Recalbox)
* Manage games lists to use with the roms file management: merge and split files, convert DAT or INI files, and more
* Includes extensive help

Obviously, this tool does not download roms.

## Usage

Launch ArcadeManager on a computer running Windows, MacOS or Linux. It can connect to another computer (or Raspberry Pi) using network shares.

## External help

If you are new to arcade emulation, please read these resources:

* [Arcade roms and how to play them, a non-technical guide](https://retropie.org.uk/forum/topic/7247/)
* [Demistifying MAME roms](https://web.archive.org/web/20180101211010/https://choccyhobnob.com/mame/demystifying-mame-roms/)

## Screenshots

![Home](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-home.png)
![Wizard](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-wizard.png)
![Install overlays](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-overlay-download.png)
![Download CSV](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-csv-download.png)
![Copy roms](https://raw.githubusercontent.com/cosmo0/arcade-manager/docs/images/screen-rom-copy.png)

## Translating

Anyone can help with translations: they're simple text files (INI style) located in `Data\translations`.

If you think you can help, please create a pull request! Or just send me the translated files. I'm happy either way.

You can also contribute using [Weblate](https://hosted.weblate.org/projects/arcademanager/arcademanager-ui/)

## Development

### Prerequisites

* Dotnet 6 SDK - <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>
* Dotnet 8 SDK - <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>
* NodeJS 22.x or later - <https://nodejs.org/download>
* Open a command line and run `dotnet tool install --global ElectronNET.CLI`

### Build and run

````bash
cd src/ArcadeManager
dotnet build
electronize start
````

To debug, attach Visual Studio to the `ArcadeManager` process.

Run `.\samples\generate-samples.ps1` in Powershell to generate a fake romset in `tmp\roms` (empty zip files with the right names).

### Create an install package

Choose your OS and architecture:

````bash
electronize build /target win
electronize build /target osx /electron-arch arm64
electronize build /target linux
````

Windows and Linux ARM64 are not currently supported.  
MacOS x86 is not supported anymore.

If you want to build for another architecture, edit the file `src\ArcadeManager\electron.manifest.json`, scroll down to "build", locate your platform (win, osx, linux), and in "target", change the "arch" property to your architecture (x64 or arm64). Don't forget to build using the matching `/electron-arch` parameter.
