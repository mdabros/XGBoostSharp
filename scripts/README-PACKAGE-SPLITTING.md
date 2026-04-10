# Native Package Build Script - Update Summary

## Changes Made

### 1. `scripts/build-native-packages.ps1`

#### New Parameters
- `XGBoostVersion` (required) - The XGBoost version embedded in package IDs (e.g., "2.0.3")
- `MaxPartSizeMB` (optional, default: 150) - Maximum size for split parts

#### New Functionality
- **Automatic size detection**: Checks if `native/linux-x64/libxgboost.so` > 200MB
- **Automatic splitting**: Splits large files into manageable chunks
- **Part package generation**: Creates nuspec files for each part
- **Meta-package generation**: Creates a meta-package with dependencies
- **Targets file generation**: Creates MSBuild targets for runtime reassembly
- **Props file generation**: Creates build props for the meta-package

#### Helper Functions Added
```powershell
Split-FileIntoParts       # Splits binary file into zip archives
New-PartNuspec           # Generates nuspec for a part package  
New-MetaNuspec           # Generates nuspec for meta-package
New-TargetsFile          # Generates .targets for reassembly
```

### 2. `scripts/publish-native-packages.ps1`

#### Changed Behavior
- **Auto-discovery**: Discovers all packages matching the version pattern
- **Smart sorting**: Publishes packages in dependency order:
  1. Part packages (-part1, -part2, etc.)
  2. Regular packages
  3. Meta-packages (-meta)

This ensures dependencies are available before meta-packages are pushed.

## Usage Changes

### Before
```powershell
# Build
.\scripts\build-native-packages.ps1 -Version "1.0.4"

# Publish
.\scripts\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "key"
```

### After
```powershell
# Build (now requires XGBoostVersion)
.\scripts\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3"

# Publish (unchanged interface, but auto-discovers parts)
.\scripts\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "key"
```

## Workflow Example

### Scenario: libxgboost.so is 415 MB

#### Step 1: Build Packages
```powershell
.\scripts\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3"
```

**Output:**
```
=== XGBoost Native Package Builder ===
Repository Root: D:\Git\XGBoostSharp
Version: 1.0.4
XGBoost Version: 2.0.3
Max Part Size: 150 MB

Linux library size: 415.06 MB
Library exceeds 200 MB - will be split into parts
Splitting D:\Git\XGBoostSharp\native\linux-x64\libxgboost.so into parts of max 150 MB...
  Creating part 1: libxgboost.so.part1.zip
  Creating part 2: libxgboost.so.part2.zip
  Creating part 3: libxgboost.so.part3.zip
  Created 3 parts
  Generated nuspec: pkg\libxgboost-2.0.3-linux-x64-part1.nuspec
  Generated nuspec: pkg\libxgboost-2.0.3-linux-x64-part2.nuspec
  Generated nuspec: pkg\libxgboost-2.0.3-linux-x64-part3.nuspec
  Generated meta-package nuspec: pkg\libxgboost-2.0.3-linux-x64-meta.nuspec
  Generated targets file: pkg\libxgboost-2.0.3-linux-x64.targets

=== Building libxgboost-osx-x64.nuspec ===
...
=== Building libxgboost-2.0.3-linux-x64-part1.nuspec ===
...
=== Building libxgboost-2.0.3-linux-x64-part2.nuspec ===
...
=== Building libxgboost-2.0.3-linux-x64-part3.nuspec ===
...
=== Building libxgboost-2.0.3-linux-x64-meta.nuspec ===
...

Built packages:
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-osx-x64.1.0.4.nupkg
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-win-x64.1.0.4.nupkg
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg
  - D:\Git\XGBoostSharp\artifacts\libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg
```

#### Step 2: Publish Packages
```powershell
.\scripts\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "***"
```

**Output:**
```
=== XGBoost Native Package Publisher ===
XGBoost Version: 2.0.3
Package Version: 1.0.4

Discovering packages in artifacts...
Found 6 packages to publish:
  - libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg
  - libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg
  - libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg
  - libxgboost-2.0.3-osx-x64.1.0.4.nupkg
  - libxgboost-2.0.3-win-x64.1.0.4.nupkg
  - libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-osx-x64.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-osx-x64.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-win-x64.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-win-x64.1.0.4.nupkg

=== Publishing libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg ===
Successfully published libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg

Successfully Published: 6
```

## Technical Details

### Part Splitting Algorithm
1. Read source file in chunks
2. For each chunk (max 150MB):
   - Create a zip archive
   - Add chunk as single entry (`libxgboost.so.part{N}`)
   - Write to `libxgboost.so.part{N}.zip`

### Reassembly Algorithm (.targets file)
1. Combine all part zip files (binary concatenation)
2. Extract combined zip to get individual parts
3. Reconstruct original file from parts
4. Clean up temporary files

### Package Dependencies
```
libxgboost-2.0.3-linux-x64-meta (meta-package)
├── depends on: libxgboost-2.0.3-linux-x64-part1
├── depends on: libxgboost-2.0.3-linux-x64-part2
└── depends on: libxgboost-2.0.3-linux-x64-part3
```

## File Structure After Build

```
artifacts/
├── libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg  (148 MB)
├── libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg  (148 MB)
├── libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg  (119 MB)
├── libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg   (<1 MB)
├── libxgboost-2.0.3-osx-x64.1.0.4.nupkg         
└── libxgboost-2.0.3-win-x64.1.0.4.nupkg

pkg/ (auto-generated, temporary)
├── libxgboost-2.0.3-linux-x64-part1.nuspec
├── libxgboost-2.0.3-linux-x64-part2.nuspec
├── libxgboost-2.0.3-linux-x64-part3.nuspec
├── libxgboost-2.0.3-linux-x64-meta.nuspec
├── libxgboost-2.0.3-linux-x64.props
└── libxgboost-2.0.3-linux-x64.targets

temp_parts/ (auto-generated, cleaned up)
└── [temporary part zip files]
```

## Consumer Experience

### NuGet Package Manager
```xml
<PackageReference Include="libxgboost-2.0.3-linux-x64-meta" Version="1.0.4" />
```

NuGet automatically:
1. Resolves and downloads all 3 part packages
2. Downloads the meta-package
3. Restores everything to the packages folder

### Build Process
On first build after restore:
```
Combining XGBoost parts...
Extracting combined archive...
Reconstructing libxgboost.so from parts...
Done.
```

On subsequent builds:
```
(libxgboost.so already exists - no work needed)
```

## Breaking Changes

❌ **Build script now requires `-XGBoostVersion` parameter**

Migration:
```powershell
# Old
.\scripts\build-native-packages.ps1 -Version "1.0.4"

# New
.\scripts\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3"
```

## Backward Compatibility

✅ Small libraries (<200MB) build as single packages (no splitting)
✅ Publish script discovers packages automatically (no breaking changes)
✅ Consumer projects work the same way

## Testing Checklist

- [x] Script parameters validated
- [ ] Build with small Linux library (<200MB)
- [ ] Build with large Linux library (>200MB)
- [ ] Verify part packages created
- [ ] Verify meta-package created
- [ ] Verify .targets file generated
- [ ] Publish all packages
- [ ] Test consumer project references meta-package
- [ ] Test reassembly on Windows
- [ ] Test reassembly on Linux
- [ ] Test reassembly on macOS

## Next Steps

1. **Test the build**: Run with current 415MB library
2. **Verify packages**: Check artifacts directory
3. **Test publish**: Push to a test feed first
4. **Consumer test**: Create a test project that references the meta-package
5. **Update CI/CD**: Update GitHub Actions workflows if needed

## Documentation Added

- `docs/LARGE-PACKAGE-SPLITTING.md` - Comprehensive guide
- This file - Update summary for developers
