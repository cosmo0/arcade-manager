$sets = @("romset_fbneo", "romset_mame", "romset_mame2003")

$here = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

foreach ($s in $sets) {
    $folder = Join-Path $here $s
    $file = Join-Path $here "$s.txt"

    if (-not(Test-Path $folder)) { mkdir $folder }

    Write-Host "Checking if folder $s already has zip files"
    if ((Get-ChildItem -Path $folder -Filter "*.zip" -Name).Count -gt 0) {
        Write-Host "  Zip files already exist in $s : skipping"
        continue
    }

    $list = Get-Content $file
    
    Write-Host "Generating $($list.Length) files in $s"

    $i = 0
    $list | ForEach-Object {
        $path = "$folder\$_.zip"
        $i++
        $percent = [Math]::Round($i / $list.Length * 100, 0)
        Write-Progress -Activity "Generating fake roms" -Status "Creating $_" -PercentComplete $percent
        if (-not(Test-Path $path)) {
            New-Item -Path $path -ItemType File -Force | Out-Null
            if ($i % 500 -eq 0) {
                Write-Host "  generation $percent % done"
            }
        }
    }
}

Write-Host ""
Write-Host "##########################"
Write-Host "Done."
