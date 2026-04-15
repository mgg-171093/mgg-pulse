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

# Detect ImageMagick (magick or convert)
$magick = $null
if (Get-Command 'magick' -ErrorAction SilentlyContinue) {
    $magick = 'magick'
} elseif (Get-Command 'convert' -ErrorAction SilentlyContinue) {
    $magick = 'convert'
}

if (-not $magick) {
    Write-Warning @"
ImageMagick not found. icon.ico was NOT generated.
Install ImageMagick and re-run:  tools\gen-icon.ps1
Download: https://imagemagick.org/script/download.php
"@
    exit 0   # non-fatal — build continues without .ico
}

Write-Host "Generating icon.ico from $SourcePng ..."

# Resize to standard icon sizes and pack into a single .ico
& $magick $SourcePng `
    -define icon:auto-resize=256,48,32,16 `
    $DestIco

if ($LASTEXITCODE -ne 0) {
    Write-Error "ImageMagick exited with code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "icon.ico written to: $DestIco"
