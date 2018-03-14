# MAME DAT tools for Retropie

This repository provides DAT management tools to manage your romset, specifically designed for use with Retropie installed on a Raspberry Pi.

**THIS TOOL DOES NOT REBUILD ROMSETS !!!** It assumes you have a working romset corresponding to the MAME version you're using.  
If you ever need to change your romset version use a tool like [CLRMAMEPRO](https://mamedev.emulab.it/clrmamepro/).

## Usage

The tool provides the following filtering abilities:

* Copy games from one location to another from a list
* Remove games from one location based on a list

You will probably just want to use the `set-lite-stick` or `set-lite-pad`, depending on your control scheme of choice. They provide a complete, ready-to-use list of games working on a Raspberry Pi 3 with either an arcade stick or a "modern" pad (with analog sticks, like Xbox or Playstation controllers).

An advanced usage might be:

* Add `set-noclones-noconsoles` to copy all non-consoles working roms of MAME2003 from your romset
* Add `filter-workingclones-noparent` to add the clones that have a non-working parent
* Remove `games-alternative` to remove "alternative" games
* Remove `controls-alternative` to remove "alternative" controls
* Remove `filter-slow` to remove the games that are too slow to play on a Raspberry Pi 3

## Contents

The provided dat files have the following content, to let you customize your romset as you wish with combinations of "add" and "remove" operations.

All list remove the non-arcade games (casino, mahjong, mature...). All lists remove the non-working games, except the "complete" and "nonworking".  
For the purpose of these lists, "working" games are either perfectly emulated, or fully playable with a few graphical or audio problems.  
Warning: since MAME "working with issues" databases do not distinguish between a few bugs (clipping, flicker, wrong colors...) and more serious issues (flipped screen, wrong ratio, unresizable screen...), "working" games are sometimes not really playable.

To build your own filter, check out [the filters file](FILTERS.md).

**Initial copy** - start with one of the following lists, depending on the type of set you want.

* `set-complete`: the complete romset, including non-working games, but excluding casino games, mature games, etc.
* `set-working`: all the working games
* `set-noclone`: no clone roms
* `set-noconsole`: no clones, no NeoGeo/Nintendo VS/Playchoice 10/Nintendo Super System/MegaPlay...
* `set-lite-stick`: no consoles, no analog controls
* `set-lite-pad`: no consoles, includes analog controls
* `set-classics-80s-pad`: manually curated list of games from the 80's that are considered "classics", and are playable on a Pi3 with a gamepad.
* `set-classics-90s-pad`: manually curated list of games from the 80's and 90's that are considered "classics", and are playable on a Pi3 with a gamepad. May include NeoGeo games.
* `set-goodgames-pad`: manually curated list of games, playable on a Pi3 with a gamepad.

**Control schemes** - depending on how you play your games (pad, stick, bartop, cabinet...), you might want to add or remove some control schemes.

* `controls-analog`: only analog-controlled games (trackball, dial, wheel...), sometimes playable with an analog pad, but not with an arcade stick
* `controls-alternative`: only "alternative" control schemes (rotary stick, keyboard, lightgun...) that are just not possible to use without dedicated controls
* `controls-pad`: only games playable with an analog pad, no "alternative" controls
* `controls-stickonly`: only games playable with an arcade stick, no analog or "alternative" controls

**Filters** - fine-control filters if you have started with a large set and applied many filters.

* `filter-clones`: all the clones
* `filter-nonworking`: all the non-working games
* `filter-workingclones-noparent`: all the clones that have a non-working parent
* `filter-consoles`: all the "console" games (NegoGeo, Nintendo VS/Playchoice...)
* `filter-slow`: all games that are too slow on a Raspberry Pi 3 (mostly 3D games)

## Resources and references

* [ArcadeItalia](http://adb.arcadeitalia.net/)
* [ProgettoSnaps](http://progettosnaps.net/)