
http://adb.arcadeitalia.net/lista_mame.php?lang=en

After exporting from ArcadeItalia, sometimes add a column (lang2 for instance) in the header between `on_arcaworld` and `ranking`. Sometimes not. Good luck.

# GLOBAL FILTER

* MAME versions ("version to"):
  * MAME4ALL, MAME 2000: 0.37b5
  * MAME 2003: 0.78
  * MAME 2010: 0.139
  * MAME 2014: 0.159
  * MAME 2016: 0.174
  * ADVANCEMAME 0.94: 0.94
  * ADVANCEMAME 1.4, 3: 0.106
* Base filter before `complete`: Arcade, no casino, no system, no mahjong, no bios, no mature, no device, no mechanical, no screenless
* Base filter before `working`: emulation: working or imperfect ; driver: good or imperfect for emulation/sound/color/graphics ; protection: good or preliminary

# SETS

* `set-noclone`: ADD: only parents
* `set-noclone-noconsole`: SCRIPT: main "noclone", remove "consoles"
* `set-lite-stick`: ADD: input: buttons only, joystick ; SCRIPT: remove "analog", "alternative", "slow"
* `set-lite-pad`: ADD: input: pedal, trackball, dial, paddle, stick ; SCRIPT: remove "alternative", "slow"
* `set-classics-80s-pad`: MANUAL: games from the 80's that are considered "classics", and are playable on a Pi3 with a gamepad.
* `set-classics-90s-pad`: MANUAL: games from the 80's and 90's that are considered "classics", and are playable on a Pi3 with a gamepad. May include NeoGeo games.
* `set-goodgames-pad`: MANUAL: good games, according to me.

# CONTROLS

SCRIPT: filter after export: when a game has wheel + shift stick, it has dial and stick.

* `controls-analog`: Complete + input: pedal, trackball, dial, paddle, stick ; SCRIPT: remove "alternative"
* `controls-alternative`: Complete + input: gambling, keyboard, mahjong, hanafuda, keypad, mouse, positional, triple joystick, double joystick, lightgun
* `controls-stickonly`: Complete + input: buttons only, joystick ; SCRIPT: remove "analog", remove "alternative"
* `controls-pad`: SCRIPT: main "analog", add "stickonly", remove "alternative"

# FILTERS

* `filter-nonworking`: No filter + emulation: not working
* `filter-clones`: Complete + clones only
* `filter-workingclones-noparent`: SCRIPT: create specific script, based on "complete", to remove the clones except when the parent is not working and the clone is working.
* `filter-consoles`: Complete + driver source file: neogeo.c;neogeo.cpp;playch10.c;playch10.cpp;vsnes.c;vsnes.cpp;snesb.c;snesb.cpp;nss.c;nss.cpp;megaplay.c;megaplay.cpp;megatech.c;megatech.cpp
* `filter-slow`: Complete + driver source file: namcos11.c;namcos11.cpp;namcos12.c;namcos12.cpp;seattle.c;seattle.cpp;kinst.c;kinst.cpp;naomi.c;naomi.cpp;namcos22.c;namcos22.cpp;midvunit.c;midvunit.cpp;stv.c;stv.cpp;model2.c;model2.cpp;model3.c;model3.cpp;zn.c;zn.cpp
