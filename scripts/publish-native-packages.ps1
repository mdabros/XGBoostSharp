<#
.SYNOPSIS
    Publishes native XGBoost NuGet packages
.DESCRIPTION
    This script publishes the native XGBoost NuGet packages to a NuGet feed.
    Supports NuGet.org, Azure Artifacts, and other NuGet v3 feeds.
.PARAMETER XGBoostVersion
    The XGBoost version embedded in the package IDs (e.g., "2.0.3")
.PARAMETER Version
    The NuGet package version to publish (e.g., "1.0.4")
.PARAMETER Source
    The NuGet feed URL to publish to (default: https://api.nuget.org/v3/index.json)
.PARAMETER ApiKey
    The API key for authenticating with the NuGet feed
.PARAMETER PackageDir
    The directory containing the packages to publish (default: ./artifacts)
.PARAMETER SkipDuplicate
    Skip packages that already exist on the feed (useful for re-runs)
.EXAMPLE
    .\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "your-api-key"
.EXAMPLE
    .\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -Source "https://nuget.pkg.github.com/username/index.json" -ApiKey "ghp_token"
.EXAMPLE
    .\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "key" -SkipDuplicate
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$XGBoostVersion,

    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$Source = "https://api.nuget.org/v3/index.json",

    [Parameter(Mandatory=$true)]
    [string]$ApiKey,

    [Parameter(Mandatory=$false)]
    [string]$PackageDir = "artifacts",

    [Parameter(Mandatory=$false)]
    [switch]$SkipDuplicate
)

$ErrorActionPreference = "Stop"

# Ensure we're in the repository root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

Write-Host "=== XGBoost Native Package Publisher ===" -ForegroundColor Cyan
Write-Host "Repository Root: $repoRoot"
Write-Host "XGBoost Version: $XGBoostVersion"
Write-Host "Package Version: $Version"
Write-Host "Package Directory: $PackageDir"
Write-Host "Target Source: $Source"
Write-Host "Skip Duplicate: $SkipDuplicate"
Write-Host ""

# Check if dotnet or nuget is available
$nugetExe = Get-Command nuget.exe -ErrorAction SilentlyContinue
$dotnetExe = Get-Command dotnet.exe -ErrorAction SilentlyContinue

if (-not $nugetExe -and -not $dotnetExe) {
    Write-Error "Neither nuget.exe nor dotnet.exe found in PATH. Please install NuGet CLI or .NET SDK."
    exit 1
}

$useDotnet = $null -ne $dotnetExe

# Verify package directory exists
if (-not (Test-Path $PackageDir)) {
    Write-Error "Package directory not found: $PackageDir"
    exit 1
}

# Define the expected package names using the XGBoost version embedded in the package IDs
$packageNames = @(
    "libxgboost-$XGBoostVersion-linux-x64",
    "libxgboost-$XGBoostVersion-osx-x64",
    "libxgboost-$XGBoostVersion-win-x64"
)

$successCount = 0
$failCount = 0
$skippedCount = 0
$publishedPackages = @()

# Publish each package
foreach ($packageName in $packageNames) {
    $packageFile = "$packageName.$Version.nupkg"
    $packagePath = Join-Path $PackageDir $packageFile

    if (-not (Test-Path $packagePath)) {
        Write-Warning "Package file not found: $packagePath"
        $failCount++
        continue
    }

    Write-Host ""
    Write-Host "=== Publishing $packageFile ===" -ForegroundColor Yellow
    Write-Host "Package: $packagePath"

    try {
        if ($useDotnet) {
            # Use dotnet nuget push
            $pushArgs = @(
                "nuget", "push",
                $packagePath,
                "--source", $Source,
                "--api-key", $ApiKey
            )

            if ($SkipDuplicate) {
                $pushArgs += "--skip-duplicate"
            }

            Write-Host "Running: dotnet $($pushArgs[0..1] -join ' ') $packageFile ..."

            & dotnet @pushArgs
        }
        else {
            # Use nuget.exe push
            $pushArgs = @(
                "push",
                $packagePath,
                "-Source", $Source,
                "-ApiKey", $ApiKey
            )

            if ($SkipDuplicate) {
                $pushArgs += "-SkipDuplicate"
            }

            Write-Host "Running: nuget.exe push $packageFile ..."

            & nuget.exe @pushArgs
        }

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully published $packageFile" -ForegroundColor Green
            $publishedPackages += $packageFile
            $successCount++
        }
        elseif ($SkipDuplicate -and $LASTEXITCODE -eq 1) {
            Write-Host "Package $packageFile already exists (skipped)" -ForegroundColor Yellow
            $skippedCount++
        }
        else {
            Write-Warning "Failed to publish $packageFile (exit code: $LASTEXITCODE)"
            $failCount++
        }
    }
    catch {
        Write-Warning "Error publishing $packageFile : $_"
        $failCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Publish Summary ===" -ForegroundColor Cyan
Write-Host "Successfully Published: $successCount"
Write-Host "Skipped (Already Exists): $skippedCount"
Write-Host "Failed: $failCount"
Write-Host ""

if ($publishedPackages.Count -gt 0) {
    Write-Host "Published packages:" -ForegroundColor Green
    foreach ($pkg in $publishedPackages) {
        Write-Host "  - $pkg"
    }
}

if ($failCount -gt 0) {
    Write-Warning "Some packages failed to publish. Please check the messages above."
    exit 1
}

Write-Host ""
Write-Host "Publishing completed!" -ForegroundColor Green
exit 0
