# TODO

## DAT check

* list https://www.progettosnaps.net/dats/MAME/ as source for XML files in the help
* warn the user that the rebuild will be based on the provided dat file & that the included files are non-merged
* compare rom files based only on crc and/or sha1 in case the name has changed - handle the rename when rebuilding
* unit tests for RebuildGame
* Have a progress bar for the steps and a progress bar for the items in the step

## Other

* Use the total and progress properties of MessageHandler everywhere (csv, roms, etc)
* Translate progression messages
* Actually use the logger to do something
* Create unit tests for all missing services (csv, downloader, overlays, roms, updater, wizard)
