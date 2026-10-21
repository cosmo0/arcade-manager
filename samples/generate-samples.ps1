param (
	$file = ".\generate-seed.txt",
	$out = "..\tmp\roms"
)

$yn = Read-Host -Prompt "Create fake romset in $out based on $file ? [y/n]"
if ($yn -ne "y") {
    return;
}

if (-not(Test-Path $out)) { mkdir $out }

$list = Get-Content $file

Write-Host "Generating $($list.Length) files"

$i = 0
$list | ForEach-Object {
    $path = "$out\$_.zip"
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
