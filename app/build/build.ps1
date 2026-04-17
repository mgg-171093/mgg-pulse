#Requires -Version 5.1
<#
.SYNOPSIS
    Release build pipeline for MGG Pulse.

.DESCRIPTION
    1. Reads Version from Directory.Build.props (or $Version parameter)
    2. Runs dotnet publish (win-x64, Release, framework-dependent)
    3. Calls tools/gen-icon.ps1 to produce icon.ico
    4. Compiles the installer with Inno Setup (iscc)
    5. Outputs MGGPulse-Setup-{version}.exe to build/output/

.PARAMETER Version
    Override the version number. Defaults to reading <Version> from Directory.Build.props.

.PARAMETER SkipIco
    Skip icon generation (useful when ImageMagick is not installed).

.EXAMPLE
    pwsh -File build/build.ps1
    pwsh -File build/build.ps1 -Version 1.3.1
#>

[CmdletBinding()]
param(
    [string] $Version = '',
    [switch] $SkipIco
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot   = Resolve-Path "$PSScriptRoot\.."
$UIProject  = Join-Path $RepoRoot 'src\MGG.Pulse.UI\MGG.Pulse.UI.csproj'
$PropsFile  = Join-Path $RepoRoot 'Directory.Build.props'
$PublishDir = Join-Path $RepoRoot 'build\publish'
$OutputDir  = Join-Path $RepoRoot 'build\output'
$IssScript  = Join-Path $RepoRoot 'build\pulse.iss'
$GenIco     = Join-Path $RepoRoot 'tools\gen-icon.ps1'

# -- Resolve version -------------------------------------------------------
if (-not $Version) {
    if (Test-Path $PropsFile) {
        [xml]$props = Get-Content $PropsFile
        $Version = $props.Project.PropertyGroup.Version
    }

    if (-not $Version) {
        [xml]$csproj = Get-Content $UIProject
        $Version = $csproj.Project.PropertyGroup.Version
    }

    if (-not $Version) {
        $Version = '1.0.0'
        Write-Warning "<Version> not found in Directory.Build.props/.csproj - defaulting to $Version"
    }
}

Write-Host ""
Write-Host "=== MGG Pulse build v$Version ===" -ForegroundColor Cyan
Write-Host ""

# -- Clean publish dir -----------------------------------------------------
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
New-Item -ItemType Directory -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Path $OutputDir -ErrorAction SilentlyContinue | Out-Null

# -- dotnet publish --------------------------------------------------------
Write-Host "[1/4] dotnet publish ..."
dotnet publish $UIProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output $PublishDir `
    -p:Version=$Version

if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed"; exit 1 }

# -- Generate icon ---------------------------------------------------------
if (-not $SkipIco) {
    Write-Host "[2/4] Generating icon.ico ..."
    & pwsh -File $GenIco
}
else {
    Write-Host "[2/4] Skipping icon generation (SkipIco set)"
}

# -- Inno Setup compile ----------------------------------------------------
$iscc = $null
foreach ($candidate in @(
    'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    'C:\Program Files\Inno Setup 6\ISCC.exe'
)) {
    if (Test-Path $candidate) { $iscc = $candidate; break }
}

if (-not $iscc) {
    if (Get-Command 'iscc' -ErrorAction SilentlyContinue) { $iscc = 'iscc' }
}

if (-not $iscc) {
    Write-Warning @"
Inno Setup not found. Installer was NOT compiled.
Install Inno Setup 6 and re-run:  build\build.ps1
Download: https://jrsoftware.org/isdl.php
"@
}
else {
    Write-Host "[3/4] Compiling installer with Inno Setup ..."
    & $iscc $IssScript "/DAppVersion=$Version" "/DPublishDir=$PublishDir" "/DOutputDir=$OutputDir"
    if ($LASTEXITCODE -ne 0) { Write-Error "ISCC failed"; exit 1 }
}

# -- Done ------------------------------------------------------------------
Write-Host ""
Write-Host "[4/4] Build complete." -ForegroundColor Green
Write-Host "      Installer: $OutputDir\MGGPulse-Setup-$Version.exe"
