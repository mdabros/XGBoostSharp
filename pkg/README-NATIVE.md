# XGBoost Native Library Package

This package contains the native XGBoost library for use with XGBoostSharp.

## About

XGBoost is an optimized distributed gradient boosting library designed to be highly efficient, flexible and portable.

This package is automatically generated from the official XGBoost Python wheels and contains the native library required for XGBoostSharp to function on the target platform.

## Platform Support

This package is platform-specific. Make sure to install the correct package for your target platform:

- **libxgboost-2.0.3-linux-x64**: For Linux x64 systems
- **libxgboost-2.0.3-osx-x64**: For macOS x64 systems  
- **libxgboost-2.0.3-win-x64**: For Windows x64 systems

## Usage

This package is typically referenced as a dependency by the `XGBoostSharp-cpu` meta-package. You generally don't need to install it directly.

If you want to install it directly, simply add a package reference:

```xml
<PackageReference Include="libxgboost-2.0.3-{platform}" Version="x.x.x" />
```

The native library will automatically be copied to your output directory during build.

## License

This package includes XGBoost native libraries which are licensed under the Apache License 2.0.

## Links

- XGBoostSharp: https://github.com/mdabros/XGBoostSharp
- XGBoost Project: https://github.com/dmlc/xgboost
