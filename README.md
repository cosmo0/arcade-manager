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
* Remove `slow-pi` to remove the games that are too slow to play on a Raspberry Pi 3

## Contents

The provided dat files have the following content, to let you customize your romset as you wish with combinations of "add" and "remove" operations.

All lists remove the non-working games, except the "complete" and "nonworking".  
For the purpose of these lists, "working" games are either perfectly emulated, or fully playable with a few graphical or audio problems. If a game has serious graphical issues (small or flipped screen, missing important elements...) or no audio, it is not considered "working".

**Initial copy** - start with one of the following lists, depending on the type of set you want.

* `set-complete`: the complete romset; should be useless to you, but just in case...
* `set-working`: all the working games
* `set-noclones`: no clones, only parent roms
* `set-noclones-noconsoles`: no clones, no NeoGeo (you should use FBA), no Nintendo VS/Playchoice (they're copy of the NES games but with a timer)
* `set-lite-stick`: no consoles, only arcade games, only games playable with an arcade stick
* `set-lite-pad`: no consoles, only arcade games, only games playable with an analog gamepad (Xbox, Playstation...)

**Control schemes** - depending on how you play your games (pad, stick, bartop, cabinet...), you might want to add or remove some control schemes.

* `controls-analog`: only analog-controlled games (trackball, dial, wheel...), often playable with an analog pad, but not with an arcade stick
* `controls-alternative`: only "alternative" control schemes (rotary stick, keyboard, lightgun...) that are just not possible to use without dedicated controls
* `controls-pad`: only games playable with an analog pad, no "alternative" controls
* `controls-stickonly`: only games playable with an arcade stick, no analog or "alternative" controls

**Game types** - you will probably want to only use "regular arcade" games, so these filters allow you to add just them or remove unwanted ones.

* `games-arcade`: "regular arcade" games (excludes the "alternatives" below)
* `games-alternative`: "alternative" games that often require a special setup (casino, quiz, mahjong, rhythm, multiplayer-only...), as well as "mature" games.

**Filters** - fine-control filters if you have started with a large set and applied many filters.

* `filter-clones`: all the clones
* `filter-nonworking`: all the non-working games
* `filter-workingclones-noparent`: all the clones that have a non-working parent
* `filter-consoles`: all the "console" games (NegoGeo, Nintendo VS/Playchoice).
* `filter-slow-pi`: all the games that are too slow on a Raspberry Pi 3

## Resources and references

* [ArcadeItalia](http://adb.arcadeitalia.net/)