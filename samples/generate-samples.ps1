$here = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

$yn = read-host -prompt "Create fake romset in tmp\roms? [y/n]"
if ($yn -ne "y") {
    return;
}

if (-not(test-path "$here\..\tmp")) { mkdir "$here\..\tmp"; }
if (-not(test-path "$here\..\tmp\roms")) { mkdir "$here\..\tmp\roms"; }

$list = get-content "$here\generate-seed.txt"

write-host "Generating $($list.length) files"

$i = 0
$list | % {
    $path = "$here\..\tmp\roms\$_.zip"
    $i++
    write-progress -Activity "Generating fake roms" -Status "Creating $_" -PercentComplete ($i / $list.length * 100)
    if (-not(test-path $path)) {
        new-item -path $path -ItemType File -Force | out-null
        write-host -nonewline "."
    }
}

write-host ""
write-host "##########################"
write-host "Done."
