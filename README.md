# Arcade manager for Retropie

[![MacOS Build Status](https://travis-ci.org/cosmo0/retropie-arcade-manager.svg?branch=master)](https://travis-ci.org/cosmo0/retropie-arcade-manager)
[![Windows/Linux Build status](https://ci.appveyor.com/api/projects/status/npgiar9ncapx2al4?svg=true)](https://ci.appveyor.com/project/cosmo0/retropie-arcade-manager)
[![license](https://img.shields.io/github/license/cosmo0/retropie-arcade-manager.svg)](https://github.com/cosmo0/retropie-arcade-manager/blob/master/LICENSE.md)

This repository provides a rom management tool, specifically designed for use with Retropie.  
It helps you to manage and filter your rom collection by removing unwanted, bad, and unplayable games.  
It's also useful if you feel a bit lost among the several thousand games that MAME provides, and only want the very best.

**THIS TOOL DOES NOT REBUILD ROMSETS!!!** It assumes you have a working romset corresponding to the MAME/FBA version you're using.  
If you ever need to change your romset version use a tool like [ClrMamePro](https://mamedev.emulab.it/clrmamepro/).

If you are new to arcade emulation, please read these resources:

* [Arcade roms and how to play them, a non-technical guide](https://retropie.org.uk/forum/topic/7247/)
* [Demistifying MAME roms](https://choccyhobnob.com/mame/demystifying-mame-roms/)
* [How to use MAME with Retropie](https://retropie.org.uk/forum/topic/2859/)
* [Validating, rebuilding and filtering ROM collections](https://github.com/RetroPie/RetroPie-Setup/wiki/Validating,-Rebuilding,-and-Filtering-ROM-Collections)
* [Which arcade emulator should I choose?](https://www.reddit.com/r/RetroPie/comments/6v86nd/what_rom_set_works_best_with_mame/dlyhccz/)
* [FBA vs MAME](https://retropie.org.uk/forum/topic/13769/)

## Usage

The tool provides the following abilities:

* Install overlay packs for your arcade games
* Copy or remove rom files based on CSV files
* Manage CSV files: merge, split, etc.
