#Requires -Version 5.1
<#
.SYNOPSIS
    Generates icon.ico (16/32/48/256) from assets/branding/icon-app.png.

.DESCRIPTION
    Primary path uses tools/generate_icon.py (Python + Pillow strict ICO writer).
    Falls back to ImageMagick, and then to System.Drawing if dependencies are unavailable.

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
$PythonGenerator = Join-Path $PSScriptRoot 'generate_icon.py'

if (-not (Test-Path $SourcePng)) {
    Write-Error "Source PNG not found: $SourcePng"
    exit 1
}

# Detect Python and ImageMagick (magick only; avoid Windows convert.exe collision)
$python = $null
if (Get-Command 'python' -ErrorAction SilentlyContinue) {
    $python = 'python'
}
elseif (Get-Command 'py' -ErrorAction SilentlyContinue) {
    $python = 'py'
}

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

$generated = $false

if ($python -and (Test-Path $PythonGenerator)) {
    Write-Host "Using Python/Pillow strict ICO generator: $PythonGenerator"

    if ($python -eq 'py') {
        & $python -3 $PythonGenerator
    }
    else {
        & $python $PythonGenerator
    }

    if ($LASTEXITCODE -eq 0 -and (Test-Path $DestIco)) {
        $generated = $true
    }
    else {
        Write-Warning "Python generator failed (exit code $LASTEXITCODE). Falling back to ImageMagick/System.Drawing."
    }
}

if (-not $generated -and $magick) {
    Write-Host "Using ImageMagick fallback."
    & $magick $SourcePng `
        -define icon:auto-resize=256,48,32,16 `
        $DestIco

    if ($LASTEXITCODE -eq 0 -and (Test-Path $DestIco)) {
        $generated = $true
    }
    else {
        Write-Warning "ImageMagick fallback failed (exit code $LASTEXITCODE). Falling back to System.Drawing icon generation."
    }
}

if (-not $generated) {
    Write-Warning "Using System.Drawing fallback icon generation."
    New-IcoWithDotNet -PngPath $SourcePng -IcoPath $DestIco
    $generated = (Test-Path $DestIco)
}

if (-not $generated -or -not (Test-Path $DestIco)) {
    Write-Error "icon.ico generation failed. Destination file was not created."
    exit 1
}

Write-Host "icon.ico written to: $DestIco"
