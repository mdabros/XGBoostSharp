<#
.SYNOPSIS
    Builds native XGBoost NuGet packages
.DESCRIPTION
    This script builds NuGet packages for the native XGBoost libraries for all platforms.
    It uses the nuspec files in the pkg directory and the native libraries in the native directory.
.PARAMETER Version
    The version number for the packages (e.g., "1.0.4")
.PARAMETER OutputDir
    The output directory for the built packages (default: ./artifacts)
.PARAMETER Configuration
    Build configuration (default: Release)
.EXAMPLE
    .\build-native-packages.ps1 -Version "1.0.4"
.EXAMPLE
    .\build-native-packages.ps1 -Version "1.0.4" -OutputDir "C:\packages"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "artifacts",

    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Ensure we're in the repository root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

Write-Host "=== XGBoost Native Package Builder ===" -ForegroundColor Cyan
Write-Host "Repository Root: $repoRoot"
Write-Host "Version: $Version"
Write-Host "Output Directory: $OutputDir"
Write-Host "Configuration: $Configuration"
Write-Host ""

# Check if nuget.exe is available
$nugetExe = Get-Command nuget.exe -ErrorAction SilentlyContinue
if (-not $nugetExe) {
    # Check for local nuget.exe in repo root
    $localNuget = Join-Path $repoRoot "nuget.exe"
    if (Test-Path $localNuget) {
        Write-Host "Using local nuget.exe from repository root"
        $nugetCommand = $localNuget
    }
    else {
        Write-Host "nuget.exe not found. Downloading..." -ForegroundColor Yellow
        try {
            $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
            $ProgressPreference = 'SilentlyContinue'
            Invoke-WebRequest -Uri $nugetUrl -OutFile $localNuget
            $ProgressPreference = 'Continue'
            Write-Host "Downloaded nuget.exe successfully" -ForegroundColor Green
            $nugetCommand = $localNuget
        }
        catch {
            Write-Error "Failed to download nuget.exe: $_"
            exit 1
        }
    }
}
else {
    $nugetCommand = "nuget.exe"
}

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Write-Host "Created output directory: $OutputDir"
}

# Get absolute path for output directory
$OutputDir = (Resolve-Path $OutputDir).Path

# Define the native package nuspec files
$packages = @(
    "libxgboost-linux-x64.nuspec",
    "libxgboost-osx-x64.nuspec",
    "libxgboost-win-x64.nuspec"
)

$successCount = 0
$failCount = 0
$builtPackages = @()

# Build each package
foreach ($package in $packages) {
    $nuspecPath = Join-Path "pkg" $package

    if (-not (Test-Path $nuspecPath)) {
        Write-Warning "Nuspec file not found: $nuspecPath"
        $failCount++
        continue
    }

    Write-Host ""
    Write-Host "=== Building $package ===" -ForegroundColor Yellow
    Write-Host "Nuspec: $nuspecPath"

    try {
        # Build the package using nuget pack
        $packArgs = @(
            "pack",
            $nuspecPath,
            "-Version", $Version,
            "-OutputDirectory", $OutputDir,
            "-Properties", "Configuration=$Configuration"
        )

        Write-Host "Running: nuget pack $([System.IO.Path]::GetFileName($nuspecPath))..."

        & $nugetCommand @packArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully built $package" -ForegroundColor Green

            # Discover the actual .nupkg file produced (ID comes from the nuspec <id> field)
            $produced = Get-ChildItem -Path $OutputDir -Filter "*.nupkg" | Where-Object { $_.Name -notIn ($builtPackages | ForEach-Object { Split-Path $_ -Leaf }) }
            foreach ($p in $produced) { $builtPackages += $p.FullName }
            $successCount++
        }
        else {
            Write-Warning "Failed to build $package (exit code: $LASTEXITCODE)"
            $failCount++
        }
    }
    catch {
        Write-Warning "Error building $package : $_"
        $failCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Build Summary ===" -ForegroundColor Cyan
Write-Host "Successful: $successCount"
Write-Host "Failed: $failCount"
Write-Host ""

if ($builtPackages.Count -gt 0) {
    Write-Host "Built packages:" -ForegroundColor Green
    foreach ($pkg in $builtPackages) {
        Write-Host "  - $pkg"
    }
}

if ($failCount -gt 0) {
    Write-Warning "Some packages failed to build. Please check the messages above."
    exit 1
}

Write-Host ""
Write-Host "All native packages built successfully!" -ForegroundColor Green
exit 0
