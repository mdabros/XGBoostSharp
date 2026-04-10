<#
.SYNOPSIS
    Builds native XGBoost NuGet packages
.DESCRIPTION
    This script builds NuGet packages for the native XGBoost libraries for all platforms.
    It uses the nuspec files in the pkg directory and the native libraries in the native directory.
    For large libraries (>200MB), it automatically splits them into parts to comply with NuGet.org's 250MB limit.
.PARAMETER Version
    The version number for the packages (e.g., "1.0.4")
.PARAMETER XGBoostVersion
    The XGBoost version embedded in package IDs (e.g., "2.0.3")
.PARAMETER OutputDir
    The output directory for the built packages (default: ./artifacts)
.PARAMETER Configuration
    Build configuration (default: Release)
.PARAMETER MaxPartSizeMB
    Maximum size in MB for split parts (default: 150)
.EXAMPLE
    .\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3"
.EXAMPLE
    .\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3" -OutputDir "C:\packages"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$true)]
    [string]$XGBoostVersion,

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "artifacts",

    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [int]$MaxPartSizeMB = 150
)

$ErrorActionPreference = "Stop"

# Ensure we're in the repository root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
Set-Location $repoRoot

Write-Host "=== XGBoost Native Package Builder ===" -ForegroundColor Cyan
Write-Host "Repository Root: $repoRoot"
Write-Host "Version: $Version"
Write-Host "XGBoost Version: $XGBoostVersion"
Write-Host "Output Directory: $OutputDir"
Write-Host "Configuration: $Configuration"
Write-Host "Max Part Size: $MaxPartSizeMB MB"
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

# Helper function to split a file into parts
function Split-FileIntoParts {
    param(
        [string]$FilePath,
        [int]$MaxSizeMB,
        [string]$OutputDir
    )

    Write-Host "Splitting $FilePath into parts of max $MaxSizeMB MB..." -ForegroundColor Yellow

    $fileInfo = Get-Item $FilePath
    $maxBytes = $MaxSizeMB * 1MB
    $fileStream = [System.IO.File]::OpenRead($FilePath)
    $partNumber = 1
    $parts = @()

    try {
        while ($fileStream.Position -lt $fileStream.Length) {
            $partFileName = "$($fileInfo.BaseName).part$partNumber.zip"
            $partPath = Join-Path $OutputDir $partFileName

            Write-Host "  Creating part $partNumber`: $partFileName"

            # Create a zip file for this part
            $zipStream = [System.IO.File]::Create($partPath)
            $archive = New-Object System.IO.Compression.ZipArchive($zipStream, [System.IO.Compression.ZipArchiveMode]::Create)

            # Add the chunk to the zip
            $entry = $archive.CreateEntry("libxgboost.so.part$partNumber")
            $entryStream = $entry.Open()

            $buffer = New-Object byte[] 8192
            $totalRead = 0

            while ($totalRead -lt $maxBytes -and $fileStream.Position -lt $fileStream.Length) {
                $toRead = [Math]::Min($buffer.Length, $maxBytes - $totalRead)
                $toRead = [Math]::Min($toRead, $fileStream.Length - $fileStream.Position)
                $read = $fileStream.Read($buffer, 0, $toRead)
                $entryStream.Write($buffer, 0, $read)
                $totalRead += $read
            }

            $entryStream.Close()
            $archive.Dispose()
            $zipStream.Close()

            $parts += @{
                PartNumber = $partNumber
                FileName = $partFileName
                FilePath = $partPath
                Size = (Get-Item $partPath).Length
            }

            $partNumber++
        }
    }
    finally {
        $fileStream.Close()
    }

    Write-Host "  Created $($parts.Count) parts" -ForegroundColor Green
    return $parts
}

# Helper function to generate nuspec for a part package
function New-PartNuspec {
    param(
        [string]$XGBoostVersion,
        [string]$PartNumber,
        [string]$PartFilePath,
        [string]$OutputPath
    )

    $packageId = "libxgboost-$XGBoostVersion-linux-x64-part$PartNumber"

    $nuspecContent = @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>$packageId</id>
    <version>`$version`$</version>
    <authors>mdabros</authors>
    <description>XGBoost native library for Linux x64 (Part $PartNumber). This is a split part of the large libxgboost.so library.</description>
    <license type="expression">Apache-2.0</license>
    <projectUrl>https://github.com/mdabros/XGBoostSharp</projectUrl>
    <repository type="git" url="https://github.com/mdabros/XGBoostSharp" />
    <tags>xgboost native linux machine-learning</tags>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
  </metadata>
  <files>
    <file src="$PartFilePath" target="runtimes\linux-x64\native\$(Split-Path -Leaf $PartFilePath)" />
  </files>
</package>
"@

    Set-Content -Path $OutputPath -Value $nuspecContent -Encoding UTF8
    Write-Host "  Generated nuspec: $OutputPath" -ForegroundColor Green
}

# Helper function to generate the meta-package nuspec
function New-MetaNuspec {
    param(
        [string]$XGBoostVersion,
        [int]$PartCount,
        [string]$Version,
        [string]$OutputPath
    )

    $packageId = "libxgboost-$XGBoostVersion-linux-x64"

    # Build dependencies
    $dependencies = ""
    for ($i = 1; $i -le $PartCount; $i++) {
        $dependencies += "      <dependency id=`"libxgboost-$XGBoostVersion-linux-x64-part$i`" version=`"$Version`" />`n"
    }

    $nuspecContent = @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>$packageId</id>
    <version>`$version`$</version>
    <authors>mdabros</authors>
    <description>XGBoost native library for Linux x64. This meta-package combines multiple parts of the large libxgboost.so library.</description>
    <license type="expression">Apache-2.0</license>
    <projectUrl>https://github.com/mdabros/XGBoostSharp</projectUrl>
    <repository type="git" url="https://github.com/mdabros/XGBoostSharp" />
    <tags>xgboost native linux machine-learning</tags>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <readme>README.md</readme>
    <dependencies>
$dependencies    </dependencies>
  </metadata>
  <files>
    <file src="libxgboost-$XGBoostVersion-linux-x64.props" target="build\libxgboost-$XGBoostVersion-linux-x64.props" />
    <file src="libxgboost-$XGBoostVersion-linux-x64.props" target="buildTransitive\libxgboost-$XGBoostVersion-linux-x64.props" />
    <file src="libxgboost-$XGBoostVersion-linux-x64.targets" target="build\libxgboost-$XGBoostVersion-linux-x64.targets" />
    <file src="README-NATIVE.md" target="README.md" />
  </files>
</package>
"@

    Set-Content -Path $OutputPath -Value $nuspecContent -Encoding UTF8
    Write-Host "  Generated meta-package nuspec: $OutputPath" -ForegroundColor Green
}

# Helper function to generate the .targets file
function New-TargetsFile {
    param(
        [string]$XGBoostVersion,
        [int]$PartCount,
        [string]$OutputPath
    )

    $packageId = "libxgboost-$XGBoostVersion-linux-x64"

    # Build the part combination command
    $combineParts = ""
    for ($i = 1; $i -le $PartCount; $i++) {
        if ($i -eq 1) {
            $combineParts += "`$(OutDir)runtimes\linux-x64\native\libxgboost.so.part$i.zip"
        } else {
            $combineParts += " + `$(OutDir)runtimes\linux-x64\native\libxgboost.so.part$i.zip"
        }
    }

    # Build the delete parts command
    $deleteParts = ""
    for ($i = 1; $i -le $PartCount; $i++) {
        $deleteParts += " `$(OutDir)runtimes\linux-x64\native\libxgboost.so.part$i.zip"
    }

    $targetsContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Target Name="CombineXGBoostParts" AfterTargets="Build">
    <!-- Combine the parts (Windows) -->
    <Exec Condition="'`$(OS)' == 'Windows_NT' and Exists('`$(OutDir)runtimes\linux-x64\native\libxgboost.so.part1.zip')"
          Command="cmd.exe /c copy /b $combineParts `$(OutDir)runtimes\linux-x64\native\libxgboost.so.combined.zip" />

    <!-- Combine the parts (Unix) -->
    <Exec Condition="'`$(OS)' != 'Windows_NT' and Exists('`$(OutDir)runtimes/linux-x64/native/libxgboost.so.part1.zip')"
          Command="cat$($deleteParts.Replace('\', '/').Replace('$(OutDir)', '`$(OutDir)')) &gt; `$(OutDir)runtimes/linux-x64/native/libxgboost.so.combined.zip" />

    <!-- Extract the combined zip (Windows) -->
    <Exec Condition="'`$(OS)' == 'Windows_NT' and Exists('`$(OutDir)runtimes\linux-x64\native\libxgboost.so.combined.zip')"
          Command="powershell -Command &quot;Expand-Archive -Path '`$(OutDir)runtimes\linux-x64\native\libxgboost.so.combined.zip' -DestinationPath '`$(OutDir)runtimes\linux-x64\native\' -Force&quot;" />

    <!-- Extract the combined zip (Unix) -->
    <Exec Condition="'`$(OS)' != 'Windows_NT' and Exists('`$(OutDir)runtimes/linux-x64/native/libxgboost.so.combined.zip')"
          Command="unzip -o `$(OutDir)runtimes/linux-x64/native/libxgboost.so.combined.zip -d `$(OutDir)runtimes/linux-x64/native/" />

    <!-- Reconstruct the full libxgboost.so from parts -->
    <Exec Condition="Exists('`$(OutDir)runtimes\linux-x64\native\libxgboost.so.part1') or Exists('`$(OutDir)runtimes/linux-x64/native/libxgboost.so.part1')"
          Command="powershell -Command &quot;`$outPath = if ('`$(OS)' -eq 'Windows_NT') { '`$(OutDir)runtimes\linux-x64\native\libxgboost.so' } else { '`$(OutDir)runtimes/linux-x64/native/libxgboost.so' }; `$parts = Get-ChildItem -Path (Split-Path `$outPath) -Filter 'libxgboost.so.part*' | Sort-Object Name; `$fs = [System.IO.File]::Create(`$outPath); try { foreach (`$part in `$parts) { `$bytes = [System.IO.File]::ReadAllBytes(`$part.FullName); `$fs.Write(`$bytes, 0, `$bytes.Length); Remove-Item `$part.FullName } } finally { `$fs.Close() }&quot;" />

    <!-- Clean up temporary files (Windows) -->
    <Exec Condition="'`$(OS)' == 'Windows_NT' and Exists('`$(OutDir)runtimes\linux-x64\native\libxgboost.so.combined.zip')"
          Command="cmd.exe /c del `$(OutDir)runtimes\linux-x64\native\libxgboost.so.part*.zip `$(OutDir)runtimes\linux-x64\native\libxgboost.so.combined.zip" />

    <!-- Clean up temporary files (Unix) -->
    <Exec Condition="'`$(OS)' != 'Windows_NT' and Exists('`$(OutDir)runtimes/linux-x64/native/libxgboost.so.combined.zip')"
          Command="rm -f$($deleteParts.Replace('\', '/').Replace('$(OutDir)', '`$(OutDir)')) `$(OutDir)runtimes/linux-x64/native/libxgboost.so.combined.zip" />
  </Target>
</Project>
"@

    Set-Content -Path $OutputPath -Value $targetsContent -Encoding UTF8
    Write-Host "  Generated targets file: $OutputPath" -ForegroundColor Green
}

# Check if Linux library needs splitting
$linuxLibPath = Join-Path $repoRoot "native\linux-x64\libxgboost.so"
$needsSplitting = $false
$linuxParts = @()

if (Test-Path $linuxLibPath) {
    $fileSize = (Get-Item $linuxLibPath).Length / 1MB
    Write-Host "Linux library size: $([Math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan

    if ($fileSize -gt 200) {
        Write-Host "Library exceeds 200 MB - will be split into parts" -ForegroundColor Yellow
        $needsSplitting = $true

        # Create temp directory for parts
        $tempPartsDir = Join-Path $repoRoot "temp_parts"
        if (Test-Path $tempPartsDir) {
            Remove-Item -Path $tempPartsDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempPartsDir -Force | Out-Null

        # Split the file
        $linuxParts = Split-FileIntoParts -FilePath $linuxLibPath -MaxSizeMB $MaxPartSizeMB -OutputDir $tempPartsDir

        # Generate nuspec files for each part
        $pkgDir = Join-Path $repoRoot "pkg"
        foreach ($part in $linuxParts) {
            $nuspecPath = Join-Path $pkgDir "libxgboost-$XGBoostVersion-linux-x64-part$($part.PartNumber).nuspec"
            New-PartNuspec -XGBoostVersion $XGBoostVersion -PartNumber $part.PartNumber -PartFilePath $part.FilePath -OutputPath $nuspecPath
        }

        # Generate meta-package nuspec
        $metaNuspecPath = Join-Path $pkgDir "libxgboost-$XGBoostVersion-linux-x64-meta.nuspec"
        New-MetaNuspec -XGBoostVersion $XGBoostVersion -PartCount $linuxParts.Count -Version $Version -OutputPath $metaNuspecPath

        # Generate .props file for meta-package (same as regular linux package)
        $propsPath = Join-Path $pkgDir "libxgboost-$XGBoostVersion-linux-x64.props"
        if (-not (Test-Path $propsPath)) {
            $propsContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Content Include="`$(MSBuildThisFileDirectory)..\..\runtimes\linux-x64\native\libxgboost.so">
      <Link>libxgboost.so</Link>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <Visible>false</Visible>
    </Content>
  </ItemGroup>
</Project>
"@
            Set-Content -Path $propsPath -Value $propsContent -Encoding UTF8
        }

        # Generate .targets file
        $targetsPath = Join-Path $pkgDir "libxgboost-$XGBoostVersion-linux-x64.targets"
        New-TargetsFile -XGBoostVersion $XGBoostVersion -PartCount $linuxParts.Count -OutputPath $targetsPath

        Write-Host ""
    }
}

# Define the native package nuspec files
$packages = @(
    "libxgboost-osx-x64.nuspec",
    "libxgboost-win-x64.nuspec"
)

# Add Linux packages based on whether splitting was needed
if ($needsSplitting) {
    # Add part packages
    foreach ($part in $linuxParts) {
        $packages += "libxgboost-$XGBoostVersion-linux-x64-part$($part.PartNumber).nuspec"
    }
    # Add meta-package
    $packages += "libxgboost-$XGBoostVersion-linux-x64-meta.nuspec"
} else {
    # Add regular Linux package
    $packages += "libxgboost-linux-x64.nuspec"
}

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

    # Clean up temp parts directory if it exists
    if ($needsSplitting -and (Test-Path $tempPartsDir)) {
        Remove-Item -Path $tempPartsDir -Recurse -Force
    }

    exit 1
}

# Clean up temp parts directory
if ($needsSplitting -and (Test-Path $tempPartsDir)) {
    Write-Host "Cleaning up temporary parts directory..."
    Remove-Item -Path $tempPartsDir -Recurse -Force
}

Write-Host ""
Write-Host "All native packages built successfully!" -ForegroundColor Green
exit 0
