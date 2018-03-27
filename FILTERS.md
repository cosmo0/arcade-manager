
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
* `set-noclone-noconsole`: MERGE: noclone - consoles
* `set-lite-stick`: ADD: input: buttons only, joystick ; MERGE: noconsoles - analog - alternative - slow
* `set-lite-pad`: ADD: input: pedal, trackball, dial, paddle, stick ; MERGE: noconsoles - alternative - slow
* `set-classics-pad`: MANUAL: about 200 games that are considered "classics" (well-known or niche), and are playable on a Pi3 with a gamepad (includes NeoGeo games)
* `set-classics-lite-pad`: MANUAL: about 50 games that are considered "classics"

# CONTROLS

SCRIPT: filter after export: when a game has wheel + shift stick, it has dial and stick.

* `controls-analog`: Complete + input: pedal, trackball, dial, paddle, stick ; MERGE: - alternative
* `controls-alternative`: Complete + input: gambling, keyboard, mahjong, hanafuda, keypad, mouse, positional, triple joystick, double joystick, lightgun
* `controls-stickonly`: Complete + input: buttons only, joystick ; MERGE: - analog - alternative
* `controls-pad`: MERGE: analog + stickonly - alternative

# FILTERS

* `filter-nonworking`: No filter + emulation: not working
* `filter-clones`: Complete + clones only
* `filter-workingclones-noparent`: SCRIPT: create specific script, based on "complete", to remove the clones except when the parent is not working and the clone is working.
* `filter-consoles`: Complete + driver source file: neogeo.c;neogeo.cpp;playch10.c;playch10.cpp;vsnes.c;vsnes.cpp;snesb.c;snesb.cpp;nss.c;nss.cpp;megaplay.c;megaplay.cpp;megatech.c;megatech.cpp
* `filter-slow`: Complete + driver source file: namcos11.c;namcos11.cpp;namcos12.c;namcos12.cpp;seattle.c;seattle.cpp;kinst.c;kinst.cpp;naomi.c;naomi.cpp;namcos22.c;namcos22.cpp;midvunit.c;midvunit.cpp;stv.c;stv.cpp;model2.c;model2.cpp;model3.c;model3.cpp;zn.c;zn.cpp

# QUALITY

* Complete + categories: best games: export one file per value.

# SOURCES

Base CSV:
- http://adb.arcadeitalia.net/lista_mame.php?lang=en

Classics:
- https://www.ranker.com/crowdranked-list/the-best-classic-arcade-games
- https://www.bmigaming.com/top100arcade.htm
- https://www.techradar.com/news/best-arcade-games
- https://www.arcade-museum.com/TOP100.php