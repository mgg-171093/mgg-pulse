#Requires -Version 5.1
<#
.SYNOPSIS
    Generates icon.ico (16/32/48/256) from assets/branding/icon-app.png using ImageMagick.
    Falls back to a no-op with a clear warning when ImageMagick is not installed.

.DESCRIPTION
    Called by build/build.ps1 during the release pipeline. Also callable standalone:
        pwsh -File tools/gen-icon.ps1

.OUTPUTS
    assets/branding/icon.ico  (overwritten on each run)
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot  = Resolve-Path "$PSScriptRoot\.."
$SourcePng = Join-Path $RepoRoot 'assets\branding\icon-app.png'
$DestIco   = Join-Path $RepoRoot 'assets\branding\icon.ico'

if (-not (Test-Path $SourcePng)) {
    Write-Error "Source PNG not found: $SourcePng"
    exit 1
}

# Detect ImageMagick (magick only; avoid Windows convert.exe collision)
$magick = $null
if (Get-Command 'magick' -ErrorAction SilentlyContinue) {
    $magick = 'magick'
}

function New-IcoWithDotNet {
    param(
        [Parameter(Mandatory = $true)][string] $PngPath,
        [Parameter(Mandatory = $true)][string] $IcoPath
    )

    Add-Type -AssemblyName System.Drawing
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class IconNative
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}
"@

    $bitmap = New-Object System.Drawing.Bitmap($PngPath)
    try {
        $iconHandle = $bitmap.GetHicon()
        $icon = [System.Drawing.Icon]::FromHandle($iconHandle)
        try {
            $fileStream = [System.IO.File]::Open($IcoPath, [System.IO.FileMode]::Create)
            try {
                $icon.Save($fileStream)
            }
            finally {
                $fileStream.Dispose()
            }
        }
        finally {
            $icon.Dispose()
            [IconNative]::DestroyIcon($iconHandle) | Out-Null
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

Write-Host "Generating icon.ico from $SourcePng ..."

if ($magick) {
    # Resize to standard icon sizes and pack into a single .ico
    & $magick $SourcePng `
        -define icon:auto-resize=256,48,32,16 `
        $DestIco

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "ImageMagick failed (exit code $LASTEXITCODE). Falling back to System.Drawing icon generation."
        New-IcoWithDotNet -PngPath $SourcePng -IcoPath $DestIco
    }
}
else {
    Write-Warning "ImageMagick not found. Falling back to System.Drawing icon generation."
    New-IcoWithDotNet -PngPath $SourcePng -IcoPath $DestIco
}

if (-not (Test-Path $DestIco)) {
    Write-Error "icon.ico generation failed. Destination file was not created."
    exit 1
}

Write-Host "icon.ico written to: $DestIco"
