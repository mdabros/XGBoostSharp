# XGBoostSharp Agent Instructions

## Project Overview

XGBoostSharp is a C# wrapper for [XGBoost](https://github.com/dmlc/xgboost), modelled closely after
the [Python scikit-learn API](https://xgboost.readthedocs.io/en/latest/python/index.html).
The library targets `netstandard2.0`; the test project targets `net8`.

## Repository Layout

```
src/
  XGBoostSharp/           # Main library (netstandard2.0)
    lib/                  # Low-level layer: P/Invoke, safe handles, types
      NativeMethods.cs    # All DllImport declarations for libxgboost
      Booster.cs          # Managed wrapper around the XGBooster C handle
      DMatrix.cs          # Managed wrapper around the XGDMatrix C handle
      SafeBoosterHandle.cs
      SafeDMatrixHandle.cs
      SafeBufferHandle.cs
      DllFailException.cs
      ParameterNames.cs   # XGBoost parameter name constants (snake_case)
      Fields.cs           # DMatrix field name constants
      PredictionType.cs
      ModelFormat.cs
      FeatureType.cs
    XGBModelBase.cs       # Shared base class for classifiers and regressors
    XGBClassifier.cs      # High-level classifier (mirrors Python XGBClassifier)
    XGBRegressor.cs       # High-level regressor (mirrors Python XGBRegressor)
    Parameters.cs         # Strongly-typed parameter constants (C# PascalCase)
  XGBoostSharp.Tests/     # MSTest test project (net8)
    XGBClassifierTest.cs
    XGBRegressorTest.cs
    DMatrixTest.cs
    TestUtils.cs
    TestUtils.Data.cs
    TestUtils.Data.MultiLabel.cs
```

## Naming Conventions

- **XGBoost C API names:** All P/Invoke declarations in `NativeMethods.cs` MUST match the
  XGBoost C API names exactly, including casing (e.g., `XGBoosterCreate`, `XGDMatrixSetFloatInfo`).
- **XGBoost parameter names:** Constants in `ParameterNames.cs` keep the original `snake_case`
  names from the XGBoost C API (e.g., `learning_rate`, `max_depth`). Suppress `IDE1006` with
  `#pragma warning disable IDE1006` in that file only.
- **C# public API:** All public types and members outside of `ParameterNames.cs` follow standard
  C# PascalCase (e.g., `XGBClassifier`, `Predict`, `MaxDepth`).
- **Namespaces:** Low-level types live in `XGBoostSharp.Lib`; high-level types live in `XGBoostSharp`.

## Architecture and Implementation Details

### P/Invoke Layer (`NativeMethods.cs`)

- Use `[DllImport]` for all P/Invoke declarations (the library targets `netstandard2.0`,
  which does not support `LibraryImport`).
- The native library name constant is `"xgboost"`. The .NET runtime resolves the platform-specific
  file name and extension automatically
  (see [Native library loading](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading)).
- P/Invoke signatures MUST match the upstream XGBoost C API exactly. Do not change a signature
  unless the upstream C API changed.
  Reference: [XGBoost C API header](https://xgboost.readthedocs.io/en/stable/dev/c__api_8h.html)
- All XGBoost C API functions return `int` (0 = success). Check the return value with
  `ThrowIfError` and surface failures as `DllFailException`.

### Safe Handle Pattern

- Use `SafeHandle`-derived types for every native handle:
  - `SafeBoosterHandle` wraps the XGBooster pointer and calls `XGBoosterFree` on release.
  - `SafeDMatrixHandle` wraps the XGDMatrix pointer and calls `XGDMatrixFree` on release.
  - `SafeBufferHandle` wraps unmanaged memory buffers.
- Never store a raw `IntPtr` as a long-lived field; always wrap it in the appropriate safe handle.
- Reference: [SafeHandle best practices](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices)

### High-Level API (`XGBClassifier`, `XGBRegressor`)

- The public API mirrors the [Python XGBoost scikit-learn API](https://xgboost.readthedocs.io/en/latest/python/index.html).
- Constructor parameters use C# naming conventions (PascalCase) and map to XGBoost parameter
  constants defined in `ParameterNames.cs`.
- Both classes inherit from `XGBModelBase` and delegate to `Booster` for all native operations.
- Implement `IDisposable` via the base class to ensure native resources are released.

## Code Style and Formatting

- **Adhere to `.editorconfig`:** Strictly follow all formatting rules defined in `.editorconfig`.
- **Indentation:** 4 spaces for C# files; 2 spaces for `.csproj`, `.props`, `.targets`, and
  `.json` files.
- **Line length:** Keep declaration lines around 100–120 characters; split long signatures across
  multiple lines.
- **No unnecessary blank lines:** Avoid adding extra empty lines within methods or between tightly
  related declarations.
- **`using` directives:** Place all `using` directives at the file scope (file-scoped namespaces
  are used throughout).

## XML Documentation

- All public types and members MUST have XML `<summary>` documentation.
- For `NativeMethods.cs` methods and for any type that wraps an XGBoost concept, include a
  `<see href="..."/>` link to the relevant XGBoost documentation page.
- `GenerateDocumentationFile` is enabled for the library project; missing docs will produce
  compiler warnings.

## Testing

- The test project uses **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`).
- Test class naming: `[SubjectType]Test` (e.g., `XGBClassifierTest`, `DMatrixTest`).
- Test method naming: `[SubjectType]_[Scenario]` (e.g.,
  `XGBClassifierTest_Predict`, `DMatrixTest_SetLabel`).
- Shared test data and helper methods belong in `TestUtils.cs` and its partial-class companions.
- Clean up any temporary files in `[TestInitialize]` and `[TestCleanup]` methods.

## Source of Truth for XGBoost API

Always align bindings and parameter names with the latest XGBoost release. Key references:

- C API header: <https://xgboost.readthedocs.io/en/stable/dev/c__api_8h.html>
- C API tutorial: <https://xgboost.readthedocs.io/en/stable/tutorials/c_api_tutorial.html>
- Python API (design reference): <https://xgboost.readthedocs.io/en/latest/python/index.html>
- XGBoost parameters: <https://xgboost.readthedocs.io/en/stable/parameter.html>

## Workflow

- **Format before committing:** Run `dotnet format` from the `src/` directory before every commit.
- **Build:** Run `dotnet build src/` to verify the solution builds cleanly.
- **Tests:** Run `dotnet test src/` to execute all tests. All tests must pass before a change is
  considered complete.
- **Atomic commits:** Commit after each successful change or logical unit of work.

## Agent Instruction Source

- Treat this `AGENT.md` as the primary instruction source for this repository.
- Apply these instructions automatically during all work in this repository.
- Do not repeatedly ask to copy preferences into vendor-specific instruction systems.