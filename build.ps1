#requires -version 5

<#
.SYNOPSIS
Builds this repository.
.DESCRIPTION
This script runs MSBuild on this repository.
.PARAMETER Configuration
Specify MSBuild configuration: Debug, Release
.PARAMETER Clean
Specifies if the "Clean" target should be run prior to the "Build" target.
.PARAMETER Pack
Produce NuGet packages.
.PARAMETER Sign
Sign assemblies and NuGet packages (requires additional configuration not provided by this script).
.PARAMETER CI
Sets the MSBuild "ContinuousIntegrationBuild" property to "true".
.PARAMETER Verbosity
MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]
.PARAMETER MSBuildArguments
Additional MSBuild arguments to be passed through.
.EXAMPLE
Building release packages.
    build.ps1 -Configuration Release -Pack /p:some_param=some_value
#>
[CmdletBinding(PositionalBinding = $false, DefaultParameterSetName='Groups')]
param(
    [ValidateSet('Debug', 'Release')]$Configuration,

    [switch]$Clean,

    [switch]$Pack,

    [switch]$Sign,

    [switch]$CI,
    
    [string]$Verbosity = 'minimal',

    [switch]$Help,

    # Remaining arguments will be passed to MSBuild directly
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$MSBuildArguments
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'
$SolutionFile = "$PSScriptRoot/IntelligentPlant.BackgroundTasks.sln"
$Artifacts = "$PSScriptRoot/artifacts"

if ($Help) {
    Get-Help $PSCommandPath
    exit 1
}

. "$PSScriptRoot/build/tools.ps1"

# Set MSBuild verbosity
$MSBuildArguments += "/v:$Verbosity"

# Select targets
$MSBuildTargets = @()
if ($Clean) {
    $MSBuildTargets += 'Clean'
}
$MSBuildTargets += 'Build'
if ($Pack) {
    $MSBuildTargets += 'Pack'
}

$local:targets = [string]::Join(';',$MSBuildTargets)
$MSBuildArguments += "/t:$targets"

# Set default configuration if required
if (-not $Configuration) {
    $Configuration = 'Debug'
}
$MSBuildArguments += "/p:Configuration=$Configuration"

# If the Sign flag is set, add a SignOutput build argument.
if ($Sign) {
    $MSBuildArguments += "/p:SignOutput=true"
}

# Configure version numbers to use in build.
$Version = Get-Content "$PSScriptRoot/build/version.json" | Out-String | ConvertFrom-Json
$MajorMinorVersion = "$($Version.Major).$($Version.Minor)"
$MajorMinorPatchVersion = "$($MajorMinorVersion).$($Version.Patch)"

$AssemblyVersion = "$($MajorMinorVersion).0.0"
$FileVersion = "$($MajorMinorPatchVersion).0"
if ([string]::IsNullOrEmpty($Version.PreRelease)) {
    $PackageVersion = $MajorMinorPatchVersion
} else {
    $PackageVersion = "$($MajorMinorPatchVersion)-$($Version.PreRelease)"
}

$MSBuildArguments += "/p:""AssemblyVersion=$($AssemblyVersion)"""
$MSBuildArguments += "/p:""FileVersion=$($FileVersion)"""
$MSBuildArguments += "/p:""Version=$($PackageVersion)"""

if ($CI) {
    $MSBuildArguments += "/p:ContinuousIntegrationBuild=true"
}

$local:exit_code = $null
try {
    # Clear artifacts folder
    Clear-Artifacts

    # Run the build
    Run-Build
}
catch {
    Write-Host $_.ScriptStackTrace
    $exit_code = 1
}
finally {
    if (! $exit_code) {
        $exit_code = $LASTEXITCODE
    }
}

exit $exit_code