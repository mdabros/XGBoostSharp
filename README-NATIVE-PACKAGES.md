# Building and Publishing Native XGBoost Library Packages

This document describes how to build and publish the native XGBoost library NuGet packages for XGBoostSharp.

## Overview

XGBoostSharp depends on native XGBoost libraries for each platform (Linux, macOS, and Windows). These native libraries are packaged as separate NuGet packages:

- **libxgboost-2.0.3-linux-x64**: Native library for Linux x64
- **libxgboost-2.0.3-osx-x64**: Native library for macOS x64  
- **libxgboost-2.0.3-win-x64**: Native library for Windows x64

The `XGBoostSharp-cpu` meta-package references all three native library packages.

## Prerequisites

- PowerShell 5.1 or later
- NuGet CLI (`nuget.exe`) or .NET SDK (`dotnet.exe`)
- Internet connection (for downloading from XGBoost nightly builds)
- API key for publishing to NuGet.org or another NuGet feed

## Process

### 1. Download Native Libraries

The first step is to download the native libraries from the official XGBoost Python wheels.

```powershell
.\scripts\download-native-libs.ps1 -XGBoostVersion "2.0.3"
```

**Parameters:**
- `-XGBoostVersion`: The XGBoost version to download (e.g., "2.0.3")
- `-OutputDir`: Output directory for native libraries (default: "./native")
- `-TempDir`: Temporary directory for downloads (default: "temp_downloads")

**What it does:**
1. Downloads Python wheels for all three platforms from https://s3-us-west-2.amazonaws.com/xgboost-nightly-builds/
2. Extracts the native libraries from each wheel
3. Organizes them into platform-specific directories under `./native/`

**Output structure:**
```
native/
├── linux-x64/
│   └── libxgboost.so
├── osx-x64/
│   └── libxgboost.dylib
└── win-x64/
    └── xgboost.dll
```

### 2. Build NuGet Packages

Once the native libraries are downloaded, build the NuGet packages:

```powershell
.\scripts\build-native-packages.ps1 -Version "1.0.4"
```

**Parameters:**
- `-Version`: Version number for the packages (e.g., "1.0.4")
- `-OutputDir`: Output directory for built packages (default: "./artifacts")
- `-Configuration`: Build configuration (default: "Release")

**What it does:**
1. Reads the nuspec files from `./pkg/`
2. Packages the native libraries using `nuget pack`
3. Outputs `.nupkg` files to the artifacts directory

**Output:**
```
artifacts/
├── libxgboost-2.0.3-linux-x64.1.0.4.nupkg
├── libxgboost-2.0.3-osx-x64.1.0.4.nupkg
└── libxgboost-2.0.3-win-x64.1.0.4.nupkg
```

### 3. Publish to NuGet Feed

Finally, publish the packages to a NuGet feed:

```powershell
.\scripts\publish-native-packages.ps1 -Version "1.0.4" -ApiKey "your-api-key-here"
```

**Parameters:**
- `-Version`: Version of packages to publish (must match build version)
- `-ApiKey`: API key for the NuGet feed (required)
- `-Source`: NuGet feed URL (default: "https://api.nuget.org/v3/index.json")
- `-PackageDir`: Directory containing packages (default: "./artifacts")
- `-SkipDuplicate`: Skip packages that already exist on the feed

**Examples:**

Publish to NuGet.org:
```powershell
.\scripts\publish-native-packages.ps1 -Version "1.0.4" -ApiKey "oy2...abc123"
```

Publish to GitHub Packages:
```powershell
.\scripts\publish-native-packages.ps1 `
    -Version "1.0.4" `
    -Source "https://nuget.pkg.github.com/mdabros/index.json" `
    -ApiKey "ghp_..."
```

Publish to Azure Artifacts:
```powershell
.\scripts\publish-native-packages.ps1 `
    -Version "1.0.4" `
    -Source "https://pkgs.dev.azure.com/yourorg/_packaging/yourfeed/nuget/v3/index.json" `
    -ApiKey "your-azure-token"
```

## Complete Workflow Example

Here's a complete example workflow to download, build, and publish version 1.0.4:

```powershell
# Step 1: Download native libraries from XGBoost 2.0.3 release
.\scripts\download-native-libs.ps1 -XGBoostVersion "2.0.3"

# Step 2: Build NuGet packages version 1.0.4
.\scripts\build-native-packages.ps1 -Version "1.0.4"

# Step 3: Publish to NuGet.org
.\scripts\publish-native-packages.ps1 -Version "1.0.4" -ApiKey $env:NUGET_API_KEY
```

## Package Structure

Each native library package has the following structure:

```
libxgboost-2.0.3-{platform}.{version}.nupkg
├── runtimes/
│   └── {platform}/
│       └── native/
│           └── {library file}
├── build/
│   └── libxgboost-2.0.3-{platform}.props
├── buildTransitive/
│   └── libxgboost-2.0.3-{platform}.props
└── README.md
```

The `.props` files ensure that the native library is automatically copied to the output directory when a project references the package.

## Version Management

The native library packages use a separate versioning scheme from the XGBoost version:

- **XGBoost version**: The version of XGBoost itself (e.g., "2.0.3") - embedded in the package ID
- **Package version**: The version of the NuGet package (e.g., "1.0.4") - used for updates/fixes

This allows you to:
1. Update the packaging or build process without changing XGBoost version
2. Publish multiple package versions for the same XGBoost release
3. Maintain compatibility with the XGBoost release schedule

## Updating XGBoost Version

When a new XGBoost version is released:

1. Update the package IDs in the nuspec files
2. Update the dependency versions in `XGBoostSharp-cpu.nuspec`
3. Download the new native libraries
4. Build and publish with version 1.0.0 (or appropriate version)

## Troubleshooting

### Download fails
- Verify the XGBoost version exists at https://s3-us-west-2.amazonaws.com/xgboost-nightly-builds/
- Check your internet connection
- Verify the wheel naming patterns haven't changed

### Build fails
- Ensure native libraries exist in `./native/` directories
- Verify nuget.exe or dotnet.exe is in your PATH
- Check the nuspec files for syntax errors

### Publish fails
- Verify your API key is correct and has push permissions
- Check if the package version already exists (use `-SkipDuplicate`)
- Ensure the feed URL is correct

## CI/CD Integration

These scripts can be easily integrated into CI/CD pipelines:

### GitHub Actions Example

```yaml
name: Build and Publish Native Packages

on:
  push:
    tags:
      - 'native-v*'

jobs:
  build-and-publish:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3

      - name: Download native libraries
        run: .\scripts\download-native-libs.ps1 -XGBoostVersion "2.0.3"

      - name: Build packages
        run: .\scripts\build-native-packages.ps1 -Version ${{ github.ref_name }}

      - name: Publish to NuGet
        run: |
          .\scripts\publish-native-packages.ps1 `
            -Version ${{ github.ref_name }} `
            -ApiKey ${{ secrets.NUGET_API_KEY }} `
            -SkipDuplicate
```

### Azure DevOps Example

```yaml
trigger:
  tags:
    include:
      - native-v*

pool:
  vmImage: 'windows-latest'

steps:
- task: PowerShell@2
  displayName: 'Download native libraries'
  inputs:
    filePath: 'scripts/download-native-libs.ps1'
    arguments: '-XGBoostVersion "2.0.3"'

- task: PowerShell@2
  displayName: 'Build packages'
  inputs:
    filePath: 'scripts/build-native-packages.ps1'
    arguments: '-Version $(Build.SourceBranchName)'

- task: PowerShell@2
  displayName: 'Publish packages'
  inputs:
    filePath: 'scripts/publish-native-packages.ps1'
    arguments: '-Version $(Build.SourceBranchName) -ApiKey $(NuGetApiKey) -SkipDuplicate'
```

## License

The native XGBoost libraries are licensed under the Apache License 2.0. See the [XGBoost project](https://github.com/dmlc/xgboost) for details.
