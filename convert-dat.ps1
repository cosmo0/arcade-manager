
###############################################
# Converts a DAT (XML) file (from CLRMamePro)
# into a CSV
###############################################

gci "tmp\*.dat" | % {
    [xml]$dat = get-content -path $_.FullName

    write "Processing $_"

    #name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sasampleof;is_runnable;is_device;
    #is_bios;biosof;is_mechanical;is_arcade;use_chds;savestate;source_file;first_emulator;first_emulator_date;
    #last_emulator;last_emulator_date;players;buttons;coins;sound_channels;screens;display_type;display_rotate;
    #screen_orientation;display_width;display_height;display_refresh;input_controls;monitor_type;chips_cpu;
    #chips_cpu_details;chips_audio;chips_audio_details;driver_status;driver_emulation;driver_color;driver_sound;
    #driver_graphic;driver_cocktail;driver_protection;dump_status;device_types;category;genre;serie;language;
    #colors;has_device_refs;has_roms;has_disks;has_dip_switches;has_bios_sets;has_configs;has_ports;has_devices;
    #has_slots;has_adjusters;has_soft_lists;has_ram_opts;has_cheats;nplayers;url_playonline;url_shortplays;
    #url_shortplay_ms;game_rate;mature;on_top_score;on_bacheca_record;on_arcaworld;ranking;bestgame;alltime;cabinets;

    $file = "name;description;year;manufacturer;is_parent;romof;is_clone;cloneof;sasampleof;`n"

    $dat.datafile.game | % {
        $file += "$($_.name);"
        $file += "`"$($_.description.Replace(';', '-'))`";"
        $file += $_.year + ";"
        $file += "`"$($_.manufacturer)`";"
        $file += $(If ($_.cloneof -eq $null) { "YES" } Else { "NO" }) + ";" #is_parent
        $file += $(If ($_.romof -eq $null) { "-" } Else { $_.romof }) + ";" #romof
        $file += $(If ($_.cloneof -eq $null) { "NO" } Else { "YES" }) + ";" #is_clone
        $file += $(If ($_.cloneof -eq $null) { "-" } Else { $_.cloneof }) + ";" #cloneof
        $file += $(If ($_.sampleof -eq $null) { "-" } Else { $_.sampleof }) + ";" #sampleof

        $file += "`n"
    }

    $file | out-file "tmp\$($_.Name.Replace('.dat', '')).csv" -encoding "UTF8"

    write "OK"
}