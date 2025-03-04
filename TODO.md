# TODO

## DAT check

* Create a readonly version of GameRomFile to ensure I can't mistakenly modify it
* Create a Zip infrastructure layer and a corresponding model to abstract away the ZipArchive (it will also allow to pass only this model with a path property and not both the zip and path as two parameters targetZip/targetZipPath)
* Have a progress bar for the steps and a progress bar for the items in the step

## Other

* Use the total and progress properties of MessageHandler everywhere (csv, roms, etc)
* Translate progression messages
* Actually use the logger to do something
* Create unit tests for all missing services (csv, downloader, overlays, roms, updater, wizard)
