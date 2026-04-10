# Large Package Splitting - Automated Workflow

## Overview

The build script now automatically handles splitting large native libraries (>200MB) into multiple parts to comply with NuGet.org's 250MB package size limit.

## How It Works

### Detection
When `build-native-packages.ps1` runs, it checks if `native/linux-x64/libxgboost.so` exceeds 200MB.

### Automatic Splitting (if needed)
If the library is too large:

1. **Splits the file** into 150MB chunks (configurable via `-MaxPartSizeMB`)
2. **Creates part packages**:
   - `libxgboost-{version}-linux-x64-part1.{version}.nupkg`
   - `libxgboost-{version}-linux-x64-part2.{version}.nupkg`
   - etc.

3. **Creates a meta-package**:
   - `libxgboost-{version}-linux-x64-meta.{version}.nupkg`
   - Contains dependencies on all part packages
   - Includes a `.targets` file that reassembles the library at build time

### Runtime Reassembly
When a consumer project references the meta-package:

1. NuGet restores all part packages (via dependencies)
2. The `.targets` file runs after build:
   - Combines the zip parts
   - Extracts the combined file
   - Reconstructs `libxgboost.so` from the parts
   - Cleans up temporary files

## Usage

### Building Packages

```powershell
# Build all native packages (auto-splits if needed)
.\scripts\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3"

# Custom part size (default: 150MB)
.\scripts\build-native-packages.ps1 -Version "1.0.4" -XGBoostVersion "2.0.3" -MaxPartSizeMB 100
```

### Publishing Packages

```powershell
# Publish all packages (parts published before meta-package)
.\scripts\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "your-key"

# With skip-duplicate for re-runs
.\scripts\publish-native-packages.ps1 -XGBoostVersion "2.0.3" -Version "1.0.4" -ApiKey "your-key" -SkipDuplicate
```

The publish script automatically:
- Discovers all packages in the artifacts directory
- Publishes part packages first
- Publishes the meta-package last (ensuring dependencies are available)

## Package Structure

### Part Package (each part)
```
libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg
├── runtimes/
│   └── linux-x64/
│       └── native/
│           └── libxgboost.so.part1.zip
└── [metadata]
```

### Meta-Package
```
libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg
├── build/
│   ├── libxgboost-2.0.3-linux-x64.props
│   └── libxgboost-2.0.3-linux-x64.targets
├── buildTransitive/
│   └── libxgboost-2.0.3-linux-x64.props
├── [dependencies on all part packages]
└── [metadata]
```

## Generated Files

During the build process, these files are auto-generated in the `pkg/` directory:

- `libxgboost-{version}-linux-x64-part1.nuspec`
- `libxgboost-{version}-linux-x64-part2.nuspec`
- `libxgboost-{version}-linux-x64-meta.nuspec`
- `libxgboost-{version}-linux-x64.props`
- `libxgboost-{version}-linux-x64.targets`

**Note:** These are temporary and should not be committed to the repository.

## Troubleshooting

### "Part packages not found" during publish

**Cause:** Part packages were not built successfully.

**Fix:** Check the build output for errors. Ensure `native/linux-x64/libxgboost.so` exists.

### "Dependency resolution failed" in consumer project

**Cause:** Part packages not available on the feed when meta-package was installed.

**Fix:** 
1. Ensure part packages are published before the meta-package
2. The publish script handles this automatically via sorting

### Build takes longer than expected

**Cause:** Splitting and zipping large files is I/O intensive.

**Fix:** This is normal for large libraries (~400MB+). Consider:
- Using an SSD for build operations
- Adjusting `-MaxPartSizeMB` (larger parts = fewer packages but larger uploads)

### ".targets file not executing"

**Cause:** MSBuild not finding or executing the targets.

**Fix:**
1. Verify the meta-package is referenced (not the part packages directly)
2. Check that `build/` and `buildTransitive/` folders contain the targets
3. Clean and rebuild the consumer project

## Cross-Platform Considerations

The `.targets` file handles both Windows and Unix systems:

- **Windows**: Uses `cmd.exe /c copy /b` for binary concatenation
- **Linux/macOS**: Uses `cat` for concatenation
- **Extraction**: PowerShell on Windows, `unzip` on Unix

## Performance Impact

**First build after package restore:**
- ~3-5 seconds overhead for part reassembly (415MB file)

**Subsequent builds:**
- Near-zero overhead (libxgboost.so already exists)

## Package Size Limits

| Package Type | Size Limit | Actual Size (typical) |
|-------------|-----------|----------------------|
| Regular package | 250 MB | Varies by platform |
| Part package | ~150 MB | 145-150 MB each |
| Meta-package | ~1 MB | < 1 MB (metadata only) |

## Examples

### Linux library = 415 MB

**Output:**
- `libxgboost-2.0.3-linux-x64-part1.1.0.4.nupkg` (148 MB)
- `libxgboost-2.0.3-linux-x64-part2.1.0.4.nupkg` (148 MB)
- `libxgboost-2.0.3-linux-x64-part3.1.0.4.nupkg` (119 MB)
- `libxgboost-2.0.3-linux-x64-meta.1.0.4.nupkg` (< 1 MB)

### Linux library = 180 MB

**Output:**
- `libxgboost-2.0.3-linux-x64.1.0.4.nupkg` (180 MB)

(No splitting needed)

## Migration from Manual Process

The automated process replaces these manual steps:

~~1. Open 7-Zip and split the file manually~~
~~2. Rename the parts~~
~~3. Edit nuspec files by hand~~
~~4. Update version numbers in multiple files~~
~~5. Manually create .targets file~~

Now:
1. Run `build-native-packages.ps1` ✅

## Future Enhancements

Potential improvements:
- [ ] Parallel zip creation for faster splitting
- [ ] Incremental builds (skip splitting if parts exist)
- [ ] Part verification (checksums)
- [ ] Support for other large platforms (if needed)
