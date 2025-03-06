# TODO

## DAT check

* Fix/rebuild:
  * if no error: copy
  * get the list of required files
  * search the romset and other if they exist
  * if any required file is missing skip the game
  * rebuild the zip
* Have a progress bar for the steps and a progress bar for the items in the step
* Move the localizer service to the web app

## Other

* Use the total and progress properties of MessageHandler everywhere (csv, roms, etc)
* Translate progression messages
* Actually use the logger to do something
* Create unit tests for all missing services (csv, downloader, overlays, roms, updater, wizard)
