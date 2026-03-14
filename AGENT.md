# XGBoostSharp Agent Instructions

## Project Overview

XGBoostSharp is a C# wrapper for [XGBoost](https://github.com/dmlc/xgboost) modelled after the
[Python scikit-learn API](https://xgboost.readthedocs.io/en/latest/python/index.html).
Solution: `XGBoostSharp.slnx` (root). Library: `netstandard2.0`. Tests: `net10.0`. C#: `14.0`.

## Repository Layout

```
src/
  XGBoostSharp/        # Main library (netstandard2.0)
    lib/               # P/Invoke, safe handles, types (namespace: XGBoostSharp.Lib)
      NativeMethods.cs   # All [DllImport] declarations
      Booster.cs         # Managed wrapper around the XGBooster C handle
      DMatrix.cs         # Managed wrapper around the XGDMatrix C handle
      ParameterNames.cs  # snake_case XGBoost parameter name constants
      ...
    XGBModelBase.cs    # Shared base class (IDisposable)
    XGBClassifier.cs   # High-level classifier
    XGBRegressor.cs    # High-level regressor
    Parameters.cs      # Strongly-typed C# PascalCase parameter constants
  XGBoostSharp.Tests/  # MSTest test project (net10.0)
    AssemblyInitializeCultureTest.cs  # Sets InvariantCulture for all test threads
    XGBClassifierTest.cs
    XGBRegressorTest.cs
    DMatrixTest.cs
    TestUtils.cs / TestUtils.Data.cs / TestUtils.Data.MultiLabel.cs
```

## Naming Conventions

* **C API names:** P/Invoke declarations in `NativeMethods.cs` MUST match XGBoost C API names
  exactly, including casing (e.g., `XGBoosterCreate`, `XGDMatrixSetFloatInfo`).
* **Parameter names:** Constants in `ParameterNames.cs` keep original `snake_case` XGBoost names.
  Suppress `IDE1006` with `#pragma warning disable IDE1006` in that file only.
* **C# public API:** All public types and members outside `ParameterNames.cs` use PascalCase.
* **Namespaces:** Low-level types → `XGBoostSharp.Lib`; high-level types → `XGBoostSharp`.

## Implementation Details

* **P/Invoke:** Use `[DllImport]` (not `LibraryImport`; library targets `netstandard2.0`).
  Native library name: `"xgboost"` — the runtime resolves the platform-specific extension.
* **Signature fidelity:** P/Invoke signatures MUST match the upstream C API exactly; do not change
  a signature unless the upstream C API changed.
  Reference: [C API header](https://xgboost.readthedocs.io/en/stable/dev/c__api_8h.html)
* **Error handling:** All XGBoost C API functions return `int` (0 = success). Check with
  `ThrowIfError`; surface failures as `DllFailException`.
* **Safe handles:** Wrap every native pointer in a `SafeHandle`-derived type
  (`SafeBoosterHandle`, `SafeDMatrixHandle`, `SafeBufferHandle`). Never store a raw `IntPtr`
  as a long-lived field.
* **High-level API:** `XGBClassifier` and `XGBRegressor` inherit `XGBModelBase`, implement
  `IDisposable`, and delegate all native operations to `Booster`.
* **`System.Text.Json`:** Used in `Booster.cs` to serialise JSON config structs passed to the C
  API (e.g., `XGBoosterPredictFromDMatrix`). Do not replace with an alternative serialiser.
* **Build config:** TFMs, `LangVersion 12.0`, analyzers, and `NoWarn` are all centralised in
  `src/Directory.Build.props`. Do not hard-code TFMs in individual project files.
* **Versioning:** `Nerdbank.GitVersioning` calculates versions from Git history. CI checkouts
  must use `fetch-depth: 0`.

## Code Style and Formatting

* **`.editorconfig`:** Strictly follow all rules defined there.
* **Indentation:** 4 spaces for C# and `.json` files; 2 spaces for `.csproj`, `.props`, and
  `.targets` files.
* **Line length:** Keep declaration lines around 100–120 characters; split long signatures across
  multiple lines.
* **`using` directives:** File-scoped; place at file scope (file-scoped namespaces throughout).
* **No unnecessary blank lines** within methods or between tightly related declarations.

## Documentation

* **XML summaries:** All public types and members MUST have XML `<summary>` documentation.
* **External links:** Include a `<see href="..."/>` link to the relevant XGBoost documentation
  page for any type or method that wraps an XGBoost concept.
* **`CS1591` suppressed:** Missing XML comment warnings are suppressed via `<NoWarn>` in
  `Directory.Build.props` and will not cause build errors.

## Testing

* **Framework:** MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`).
* **Class naming:** `[SubjectType]Test` (e.g., `XGBClassifierTest`, `DMatrixTest`).
* **Method naming:** `[SubjectType]_[Scenario]` (e.g., `XGBClassifierTest_Predict`).
* **Shared helpers:** Belong in `TestUtils.cs` and its partial-class companions.
* **Culture:** `AssemblyInitializeCultureTest.cs` sets `InvariantCulture` for all test threads.
* **Cleanup:** Use `[TestInitialize]`/`[TestCleanup]` to manage temporary files.

## XGBoost API References

Always align bindings and parameter names with the latest XGBoost release:

* C API header: <https://xgboost.readthedocs.io/en/stable/dev/c__api_8h.html>
* C API tutorial: <https://xgboost.readthedocs.io/en/stable/tutorials/c_api_tutorial.html>
* Python API (design reference): <https://xgboost.readthedocs.io/en/latest/python/index.html>
* Parameters: <https://xgboost.readthedocs.io/en/stable/parameter.html>

## Workflow

* **Format:** Run `dotnet format` from the repository root before every commit.
* **Build:** Run `dotnet build` from the repository root.
* **Test:** Run `dotnet test` from the repository root. All tests must pass.
* **Atomic commits:** Commit after each successful change or logical unit of work.

## Agent Instruction Source

* **Use AGENT.md:** Treat this file as the primary instruction source for this repository.
* **Apply by default:** Apply these instructions automatically during all work in this repository.
* **No repeated vendor prompts:** Do not repeatedly ask to copy preferences into vendor-specific
  instruction systems.
