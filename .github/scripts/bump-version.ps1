#Requires -Version 5.1
<#
.SYNOPSIS
  Bumps patch version in Directory.Build.props and emits outputs for workflows.

.DESCRIPTION
  CONTRACT
  - Inputs:
    - PropsPath (path to Directory.Build.props)
    - DryRun (optional, no file mutation)
  - Outputs:
    - Writes `version` and `tag` to GITHUB_OUTPUT when available
    - Writes JSON payload to stdout: { "version": "x.y.z", "tag": "vx.y.z" }

  EXIT CODE CONTRACT
  - 0: Success
  - 10: Props file not found
  - 11: Version node missing or invalid
  - 12: Failed to persist updated props file
  - 13: Unexpected runtime error

.PARAMETER PropsPath
  Path to Directory.Build.props. Default: app/Directory.Build.props

.PARAMETER DryRun
  If set, computes next version without mutating files.
#>

[CmdletBinding()]
param(
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

try {
    $resolvedPropsPath = Resolve-Path -Path $PropsPath -ErrorAction SilentlyContinue
    if (-not $resolvedPropsPath) {
        Exit-WithCode 10 "Props file not found: $PropsPath"
    }

    [xml] $props = Get-Content -Path $resolvedPropsPath
    $currentRawVersion = $props.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($currentRawVersion)) {
        Exit-WithCode 11 'Version node is missing in Directory.Build.props'
    }

    try {
        $currentVersion = [Version] $currentRawVersion
    }
    catch {
        Exit-WithCode 11 "Version is invalid: $currentRawVersion"
    }

    $nextVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build + 1)
    $version = $nextVersion.ToString()
    $tag = "v$version"

    if (-not $DryRun) {
        $props.Project.PropertyGroup.Version = $version
        $props.Project.PropertyGroup.AssemblyVersion = '{0}.{1}.{2}.0' -f $nextVersion.Major, $nextVersion.Minor, $nextVersion.Build
        $props.Project.PropertyGroup.FileVersion = '{0}.{1}.{2}.0' -f $nextVersion.Major, $nextVersion.Minor, $nextVersion.Build

        try {
            $props.Save($resolvedPropsPath)
        }
        catch {
            Exit-WithCode 12 "Failed to persist props file: $resolvedPropsPath"
        }
    }

    if ($env:GITHUB_OUTPUT) {
        "version=$version" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
        "tag=$tag" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    }

    @{ version = $version; tag = $tag } | ConvertTo-Json -Compress | Write-Output
    exit 0
}
catch {
    Exit-WithCode 13 "Unexpected runtime error: $($_.Exception.Message)"
}
