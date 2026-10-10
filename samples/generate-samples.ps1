$here = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

$yn = Read-Host -Prompt "Create fake romset in tmp\roms? [y/n]"
if ($yn -ne "y") {
    return;
}

if (-not(Test-Path "$here\..\tmp")) { mkdir "$here\..\tmp"; }
if (-not(Test-Path "$here\..\tmp\roms")) { mkdir "$here\..\tmp\roms"; }

$list = Get-Content "$here\generate-seed.txt"

Write-Host "Generating $($list.Length) files"

$i = 0
$list | ForEach-Object {
    $path = "$here\..\tmp\roms\$_.zip"
    $i++
    Write-Progress -Activity "Generating fake roms" -Status "Creating $_" -PercentComplete ($i / $list.Length * 100)
    if (-not(Test-Path $path)) {
        New-Item -Path $path -ItemType File -Force | Out-Null
        Write-Host -NoNewline "."
    }
}

Write-Host ""
Write-Host "##########################"
Write-Host "Done."
