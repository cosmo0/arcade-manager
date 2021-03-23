$yn = read-host -prompt "Create fake romset in tmp\roms? [y/n]"
if ($yn -ne "y") {
    return;
}

if (-not(test-path "tmp")) { mkdir "tmp"; }
if (-not(test-path "tmp\roms")) { mkdir "tmp\roms"; }

$list = get-content "generate-seed.txt"

write-host "Generating $($list.length) files"

$list | % {
    $path = "tmp\roms\$_.zip"
    if (-not(test-path $path)) {
        new-item -path $path -ItemType File -Force | out-null
        write-host -NoNewline "."
    }
}

write-host ""
write-host "##########################"
write-host "Done."
