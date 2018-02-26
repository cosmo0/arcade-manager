
http://adb.arcadeitalia.net/lista_mame.php?lang=en

# GLOBAL FILTER

* Version to: MAME 2003 = 0.78u5
* Base filter before "complete": Arcade, no casino, no system, no mahjong, no bios, no mature, no device, no mechanical, no screenless
* Base filter before "working": emulation: working or imperfect ; driver: good or imperfect for emulation, sound, color, graphics ; protection: good

# SETS

* `set-noclones`: ADD: only parents
* `set-noclones-noconsoles`: TODO: scripted from filter-consoles and complete
* `set-lite-stick`: ADD: input: buttons only, joystick
* `set-lite-pad`: ADD: input: pedal, trackball, paddle, stick, dial

# CONTROLS

Todo: filter after export: when a game has wheel + shift stick, it has dial and stick.

* `controls-analog`: Complete + input: pedal, trackball, paddle, stick, dial
* `controls-alternative`: Complete + input: gambling, keyboard, mahjong, hanafuda, keypad, mouse, positional, triple joystick, double joystick, lightgun
* `controls-stickonly`: Complete + input: buttons only, joystick
* `controls-pad`: Complete + analog + stickonly

# FILTERS

* `filter-nonworking`: No filter + emulation: not working
* `filter-clones`: Complete + clones only
* `filter-workingclones-noparent`: TODO: scripted from complete and working sets
* `filter-consoles`: Complete + driver source file: neogeo.cpp;playch10.cpp;vsnes.cpp;snesb.cpp;nss.cpp;megaplay.cpp
