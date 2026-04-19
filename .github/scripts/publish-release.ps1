#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes installer release atomically and updates latest.json on main.

.DESCRIPTION
  CONTRACT
  - Inputs:
    - Version, Tag, InstallerPath, Repo, CommitSha
    - LatestPath, PropsPath (optional)
    - DryRun (optional, validates flow without release/mutation)
  - Behavior:
    1) Validate installer exists
    2) Compute SHA-256 and download URL
    3) Publish GitHub release with installer asset (unless DryRun)
    4) Update latest.json (unless DryRun)
    5) Commit props + latest.json and push main (unless DryRun)

  EXIT CODE CONTRACT
  - 0: Success
  - 20: Installer file not found
  - 21: GitHub release publish failed
  - 22: latest.json update failed
  - 23: Commit or push failed
  - 24: Required argument missing
  - 25: Unexpected runtime error

.PARAMETER Version
  Release version (x.y.z)

.PARAMETER Tag
  Release tag (vx.y.z)

.PARAMETER InstallerPath
  Full path to installer executable

.PARAMETER Repo
  GitHub repository (owner/name)

.PARAMETER CommitSha
  Commit SHA used as release target

.PARAMETER LatestPath
  Path to app/build/latest.json

.PARAMETER PropsPath
  Path to app/Directory.Build.props for release commit

.PARAMETER DryRun
  Validate release metadata and mutation plan without side effects.
#>

[CmdletBinding()]
param(
    [string] $Version,
    [string] $Tag,
    [string] $InstallerPath,
    [string] $Repo,
    [string] $CommitSha,
    [string] $LatestPath = 'app/build/latest.json',
    [string] $PropsPath = 'app/Directory.Build.props',
    [switch] $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exit-WithCode {
    param(
        [int] $Code,
        [string] $Message
    )

    if (-not [string]::IsNullOrWhiteSpace($Message)) {
        [Console]::Error.WriteLine($Message)
    }

    exit $Code
}

function Assert-Required {
    param(
        [string] $Name,
        [string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        Exit-WithCode 24 "Missing required argument: $Name"
    }
}

function Invoke-NativeCommand {
    param(
        [scriptblock] $Command,
        [int] $FailureCode,
        [string] $FailureMessage
    )

    try {
        & $Command
    }
    catch {
        Exit-WithCode $FailureCode "$FailureMessage Exception: $($_.Exception.Message)"
    }

    $nativeExit = $LASTEXITCODE
    if ($nativeExit -ne 0) {
        Exit-WithCode $FailureCode "$FailureMessage ExitCode=$nativeExit"
    }
}

try {
    Assert-Required -Name 'Version' -Value $Version
    Assert-Required -Name 'Tag' -Value $Tag
    Assert-Required -Name 'InstallerPath' -Value $InstallerPath
    Assert-Required -Name 'Repo' -Value $Repo
    Assert-Required -Name 'CommitSha' -Value $CommitSha

    $resolvedInstaller = Resolve-Path -Path $InstallerPath -ErrorAction SilentlyContinue
    if (-not $resolvedInstaller) {
        Exit-WithCode 20 "Installer not found: $InstallerPath"
    }

    $installerName = [System.IO.Path]::GetFileName($resolvedInstaller)
    $sha256 = (Get-FileHash -Path $resolvedInstaller -Algorithm SHA256).Hash.ToLowerInvariant()
    $downloadUrl = "https://github.com/$Repo/releases/download/$Tag/$installerName"

    if ($DryRun) {
        Write-Host "[dry-run] Release would publish tag $Tag from $CommitSha with asset $installerName"
        Write-Host "[dry-run] latest.json would be updated at $LatestPath"
        @{ version = $Version; tag = $Tag; sha256 = $sha256; url = $downloadUrl } | ConvertTo-Json -Compress | Write-Output
        exit 0
    }

    Invoke-NativeCommand -Command {
        gh release create $Tag $resolvedInstaller --title "MGG Pulse $Tag" --target $CommitSha --notes "Automated release for MGG Pulse $Version."
    } -FailureCode 21 -FailureMessage "Release publish failed for $Tag."

    try {
        $latest = Get-Content -Path $LatestPath -Raw | ConvertFrom-Json
        $latest.version = $Version
        $latest.url = $downloadUrl
        $latest.sha256 = $sha256
        $latest.notes = "Automated release via GitHub Actions."
        $latest | ConvertTo-Json -Depth 10 | Set-Content -Path $LatestPath -Encoding utf8
    }
    catch {
        Exit-WithCode 22 "latest.json update failed at $LatestPath. Exception: $($_.Exception.Message)"
    }

    Invoke-NativeCommand -Command {
        git add $PropsPath $LatestPath
    } -FailureCode 23 -FailureMessage 'git add failed after publish.'

    Invoke-NativeCommand -Command {
        git -c user.name="github-actions[bot]" -c user.email="41898282+github-actions[bot]@users.noreply.github.com" commit -m "chore(release): publish metadata for $Tag [skip ci] [skip release]"
    } -FailureCode 23 -FailureMessage 'git commit failed after publish.'

    Invoke-NativeCommand -Command {
        git push origin main
    } -FailureCode 23 -FailureMessage 'git push failed after publish.'

    @{ version = $Version; tag = $Tag; sha256 = $sha256; url = $downloadUrl } | ConvertTo-Json -Compress | Write-Output
    exit 0
}
catch {
    Exit-WithCode 25 "Unexpected runtime error: $($_.Exception.Message)"
}
