<#
.DESCRIPTION
Some MAME romsets in the latest versions have moved some common files into external ones.
This script extracts the "device_ref" property from a MAME XML file and lists the existing zips
from a romset - it's not possible to distinguish between regular devices and rom devices otherwise

You will need a MAME XML file - you can generate one from ADB, or use the following command in your
mame installation folder, then copy the result to this folder:
    mame.exe -listxml | Out-File mame.xml

.PARAMETER xml
The path to the MAME XML file

.PARAMETER roms
The path to your romset

.EXAMPLE
.\extractdevices.ps1 -xml "mame.xml" -roms "c:\mame\roms" | Out-File "devices.csv"
#>

param(
    [Parameter(Mandatory=$true)]
    [String]$xml,

    [Parameter(Mandatory=$true)]
    [String]$roms
)

[xml]$mame = Get-Content $xml

$skipped = "konami_cpu","z80","nvram","watchdog","screen","palette","speaker","generic_latch_8","m68000",`
    "gfxdecode","timer","ipt_merge_any_hi","ng_memcard","ram","naomi_m2_board","powervr2","harddisk_image",`
    "address_map_bank","neogeo_ctrl_edge","neogeo_joyac","neogeo_control_port","neogeo_cart_slot","ng_cmc_prot",`
    "discrete","galaxian_sound","generic_fifo_u32_device","neosprite_opt","neocart_rom"

$mame.mame.machine | % {
    $line = ""

    $_.device_ref | ? {
        $skipped -notcontains $_.name
    } | Select-Object -Property name -Unique | % {
        if (Test-Path "$roms\$($_.name).zip") {
            $line += ";"+$_.name
        }
    }

    if ($line -ne "") {
        $line = $_.name + $line
        
        $line
    }
}