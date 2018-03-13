
http://adb.arcadeitalia.net/lista_mame.php?lang=en

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
* `set-lite-stick`: ADD: input: buttons only, joystick ; TODO: script with removing noconsoles and analog
* `set-lite-pad`: ADD: input: pedal, trackball, dial, paddle, stick ; TODO: script with removing noconsoles and alternative
* `set-goodgames`: manual filter from lite-pad

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
* `filter-consoles`: Complete + driver source file: neogeo.cpp;playch10.cpp;vsnes.cpp;snesb.cpp;nss.cpp;megaplay.cpp;megatech.cpp
* `filter-slow`: TODO: driver source file : find 3D games (N64, Namco, etc)