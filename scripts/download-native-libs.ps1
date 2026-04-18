<#
.SYNOPSIS
    Downloads native XGBoost libraries from Python wheels
.DESCRIPTION
    This script downloads XGBoost Python wheels from PyPI and extracts
    the native libraries for linux-x64, osx-x64, and win-x64 platforms.
.PARAMETER XGBoostVersion
    The XGBoost version to download (e.g., "3.2.0")
.PARAMETER OutputDir
    The output directory where native libraries will be extracted (default: ./native)
.EXAMPLE
    .\download-native-libs.ps1 -XGBoostVersion "3.2.0"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$XGBoostVersion,

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "native",

    [Parameter(Mandatory=$false)]
    [string]$TempDir = "temp_downloads"
)

$ErrorActionPreference = "Stop"

# Ensure we're in the repository root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

# Function to extract .whl files (which are just ZIP files)
function Extract-WheelFile {
    param(
        [string]$WheelPath,
        [string]$DestinationPath
    )

    Write-Host "Extracting $WheelPath to $DestinationPath..."

    # Create destination if it doesn't exist
    if (-not (Test-Path $DestinationPath)) {
        New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
    }

    # Extract using .NET (wheel files are zip files)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($WheelPath, $DestinationPath)
}

# Function to find and copy native library from extracted wheel
function Copy-NativeLibrary {
    param(
        [string]$ExtractedPath,
        [string]$DestinationPath,
        [string]$LibraryPattern
    )

    Write-Host "Searching for native library with pattern: $LibraryPattern"

    # Find the library file
    $libFiles = Get-ChildItem -Path $ExtractedPath -Recurse -Filter $LibraryPattern

    if ($libFiles.Count -eq 0) {
        Write-Warning "No library file found matching pattern: $LibraryPattern"
        return $false
    }

    # Create destination directory
    if (-not (Test-Path $DestinationPath)) {
        New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
    }

    # Copy the library file
    foreach ($libFile in $libFiles) {
        $destFile = Join-Path $DestinationPath $libFile.Name
        Write-Host "Copying $($libFile.FullName) to $destFile"
        Copy-Item -Path $libFile.FullName -Destination $destFile -Force
    }

    return $true
}

# Function to download file with progress
function Download-FileWithProgress {
    param(
        [string]$Url,
        [string]$OutputPath
    )

    Write-Host "Downloading from: $Url"
    Write-Host "Saving to: $OutputPath"

    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Url, $OutputPath)
        Write-Host "Download completed successfully"
        return $true
    }
    catch {
        Write-Warning "Failed to download from $Url : $_"
        return $false
    }
}

# Function to download libomp.dylib from a Homebrew bottle on ghcr.io
function Download-LibompFromHomebrew {
    param(
        [string]$Bottle,
        [string]$DestinationPath,
        [string]$TempDir,
        [int]$MaxRetries = 3,
        [int]$RetryDelaySeconds = 5
    )

    Write-Host "Fetching libomp bottle info from Homebrew..."
    $brewInfo = Invoke-RestMethod "https://formulae.brew.sh/api/formula/libomp.json" -UseBasicParsing
    $libompVersion = $brewInfo.versions.stable
    $bottleInfo = $brewInfo.bottle.stable.files.$Bottle

    if (-not $bottleInfo) {
        Write-Warning "No Homebrew bottle found for libomp/$Bottle"
        return $false
    }

    $sha256 = $bottleInfo.sha256
    $blobUrl = "https://ghcr.io/v2/homebrew/core/libomp/blobs/sha256:$sha256"
    Write-Host "Downloading libomp $libompVersion ($Bottle) from Homebrew..."

    # Get anonymous token for ghcr.io
    $tokenResp = Invoke-RestMethod "https://ghcr.io/token?scope=repository:homebrew/core/libomp:pull&service=ghcr.io" -UseBasicParsing
    $token = $tokenResp.token

    $tarPath = Join-Path $TempDir "libomp-$Bottle.tar.gz"
    $extractDir = Join-Path $TempDir "libomp-$Bottle"

    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        $originalProgressPreference = $ProgressPreference
        try {
            if (Test-Path $tarPath) { Remove-Item $tarPath -Force }

            $ProgressPreference = 'SilentlyContinue'
            Invoke-WebRequest -Uri $blobUrl -OutFile $tarPath -UseBasicParsing -Headers @{
                'Authorization' = "Bearer $token"
                'Accept'        = 'application/octet-stream'
                'User-Agent'    = 'PowerShell/XGBoostSharp Native Library Downloader'
            }
            Write-Host "Download completed successfully"
            break
        }
        catch {
            if (Test-Path $tarPath) { Remove-Item $tarPath -Force -ErrorAction SilentlyContinue }

            if ($attempt -lt $MaxRetries) {
                Write-Warning "Download attempt $attempt of $MaxRetries failed: $_"
                Write-Host "Retrying in $RetryDelaySeconds seconds..."
                Start-Sleep -Seconds $RetryDelaySeconds
            }
            else {
                Write-Warning "Failed to download libomp bottle after $MaxRetries attempts: $_"
                return $false
            }
        }
        finally {
            $ProgressPreference = $originalProgressPreference
        }
    }

    # Extract bottle tarball (tar is available on Windows 10+)
    Write-Host "Extracting libomp bottle..."
    New-Item -ItemType Directory -Path $extractDir -Force | Out-Null
    tar -xzf $tarPath -C $extractDir

    # Find libomp.dylib inside the extracted bottle (path: libomp/<version>/lib/libomp.dylib)
    $dylib = Get-ChildItem -Path $extractDir -Recurse -Filter "libomp.dylib" | Select-Object -First 1
    if (-not $dylib) {
        Write-Warning "libomp.dylib not found inside extracted bottle"
        return $false
    }

    if (-not (Test-Path $DestinationPath)) {
        New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
    }

    $destFile = Join-Path $DestinationPath "libomp.dylib"
    Write-Host "Copying $($dylib.FullName) to $destFile"
    Copy-Item -Path $dylib.FullName -Destination $destFile -Force
    return $true
}
# Main script
Write-Host "=== XGBoost Native Library Downloader ===" -ForegroundColor Cyan
Write-Host "XGBoost Version: $XGBoostVersion"
Write-Host "Output Directory: $OutputDir"
Write-Host ""

# Create temp directory
if (Test-Path $TempDir) {
    Write-Host "Cleaning existing temp directory..."
    Remove-Item -Path $TempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

# Try to get wheel URLs from PyPI API
Write-Host "Fetching wheel information from PyPI..."
try {
    $pypiUrl = "https://pypi.org/pypi/xgboost/$XGBoostVersion/json"
    $pypiResponse = Invoke-RestMethod -Uri $pypiUrl -UseBasicParsing
    $wheels = $pypiResponse.urls | Where-Object { $_.packagetype -eq "bdist_wheel" }
}
catch {
    Write-Error "Failed to fetch wheel information from PyPI: $_"
    exit 1
}

# Define platforms and their wheel patterns
$platforms = @(
    @{
        Name = "linux-x64"
        WheelPattern = "*manylinux*x86_64.whl"
        LibraryPatterns = @("libxgboost.so", "libgomp*")
        OutputSubDir = "linux-x64"
    },
    @{
        Name = "osx-x64"
        WheelPattern = "*macosx*x86_64.whl"
        LibraryPatterns = @("libxgboost.dylib")
        LibompBottle   = "sonoma"
        OutputSubDir = "osx-x64"
    },
    @{
        Name = "osx-arm64"
        WheelPattern = "*macosx*arm64.whl"
        LibraryPatterns = @("libxgboost.dylib")
        LibompBottle   = "arm64_sonoma"
        OutputSubDir = "osx-arm64"
    },
    @{
        Name = "win-x64"
        WheelPattern = "*win_amd64.whl"
        LibraryPatterns = @("xgboost.dll")
        OutputSubDir = "win-x64"
    }
)

$successCount = 0
$failCount = 0

foreach ($platform in $platforms) {
    Write-Host ""
    Write-Host "=== Processing $($platform.Name) ===" -ForegroundColor Yellow

    # Find matching wheel from PyPI
    $matchingWheel = $wheels | Where-Object { $_.filename -like $platform.WheelPattern } | Select-Object -First 1

    if (-not $matchingWheel) {
        Write-Warning "No wheel found for $($platform.Name) matching pattern: $($platform.WheelPattern)"
        $failCount++
        continue
    }

    $wheelUrl = $matchingWheel.url
    $wheelFilename = $matchingWheel.filename
    $wheelPath = Join-Path $TempDir $wheelFilename
    $extractPath = Join-Path $TempDir "$($platform.Name)_extracted"
    $outputPath = Join-Path $OutputDir $platform.OutputSubDir

    Write-Host "Found wheel: $wheelFilename"

    # Download the wheel
    $downloaded = Download-FileWithProgress -Url $wheelUrl -OutputPath $wheelPath

    if (-not $downloaded) {
        Write-Warning "Skipping $($platform.Name) due to download failure"
        $failCount++
        continue
    }

    # Extract the wheel
    try {
        Extract-WheelFile -WheelPath $wheelPath -DestinationPath $extractPath
    }
    catch {
        Write-Warning "Failed to extract wheel for $($platform.Name): $_"
        $failCount++
        continue
    }

    # Copy native libraries (may be multiple patterns per platform)
    $allCopied = $true
    foreach ($pattern in $platform.LibraryPatterns) {
        $copied = Copy-NativeLibrary -ExtractedPath $extractPath -DestinationPath $outputPath -LibraryPattern $pattern

        if (-not $copied) {
            Write-Warning "No file found matching pattern '$pattern' for $($platform.Name)"
            # Only fail if the primary library (first pattern) is missing
            if ($pattern -eq $platform.LibraryPatterns[0]) {
                $allCopied = $false
            }
        }
    }

    # Download libomp from Homebrew if this platform specifies a bottle
    if ($platform.LibompBottle -and $allCopied) {
        Write-Host "Downloading libomp from Homebrew for $($platform.Name)..."
        $libompOk = Download-LibompFromHomebrew -Bottle $platform.LibompBottle -DestinationPath $outputPath -TempDir $TempDir
        if (-not $libompOk) {
            Write-Warning "Failed to download libomp for $($platform.Name)"
            $allCopied = $false
        }
    }

    if ($allCopied) {
        Write-Host "Successfully processed $($platform.Name)" -ForegroundColor Green
        $successCount++
    }
    else {
        Write-Warning "Failed to copy primary native library for $($platform.Name)"
        $failCount++
    }
}

# Cleanup
Write-Host ""
Write-Host "Cleaning up temporary files..."
Remove-Item -Path $TempDir -Recurse -Force

# Summary
Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Successful: $successCount"
Write-Host "Failed: $failCount"
Write-Host ""
Write-Host "Native libraries have been extracted to: $OutputDir"

if ($failCount -gt 0) {
    Write-Warning "Some platforms failed to download. Please check the messages above."
    exit 1
}

Write-Host "All native libraries downloaded successfully!" -ForegroundColor Green
exit 0
