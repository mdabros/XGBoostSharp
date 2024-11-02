using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

// Relevant links XGBoost:
// https://github.com/dmlc/xgboost/blob/7a6121669097745f57b8aaad1dd3a162fef96612/jvm-packages/xgboost4j/src/main/java/ml/dmlc/xgboost4j/java/XGBoostJNI.java#L105
// https://github.com/dmlc/xgboost/blob/7a6121669097745f57b8aaad1dd3a162fef96612/src/c_api/c_api.cc#L895
// https://xgboost.readthedocs.io/en/stable/tutorials/c_api_tutorial.html

// Relevant links for cross-platform DllImport:
// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
// https://github.com/dotnet/runtime/issues/8295
// https://github.com/MonoGame/MonoGame/issues/5339#issuecomment-353751447
// https://github.com/dotnet/coreclr/pull/19373
// https://learn.microsoft.com/en-us/samples/dotnet/samples/dllmap-demo/
// https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading

public static class NativeMethods
{
    // We can use `xgboost` as dll name for all platforms since .NET handles the
    // platform-specific naming and file extension (e.g., .dll, .so, .dylib)
    // when searching for the dll.
    // See https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading.
    const string XGBoostNtvDllName = "xgboost";

    [DllImport(XGBoostNtvDllName)]
    public static extern string XGBGetLastError();

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGDMatrixCreateFromMat(
        float[] data, ulong nrow, ulong ncol,
        float missing, out SafeDMatrixHandle handle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGDMatrixFree(IntPtr handle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGDMatrixGetFloatInfo(
        SafeDMatrixHandle handle, string field,
        out ulong len, out SafeBufferHandle result);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGDMatrixSetFloatInfo(
        SafeDMatrixHandle handle, string field,
        float[] array, ulong len);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterCreate(
        SafeDMatrixHandle[] dmats,
        ulong len, out SafeBoosterHandle handle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterFree(IntPtr handle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterSetParam(
        SafeBoosterHandle handle, string name, string val);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterUpdateOneIter(
        SafeBoosterHandle bHandle, int iter,
        SafeDMatrixHandle dHandle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterPredict(
        SafeBoosterHandle bHandle,
        SafeDMatrixHandle dHandle,
        int optionMask, int ntreeLimit,
        int training, // Only relevant for DART training. See https://github.com/dmlc/xgboost/issues/5601.
        out ulong predsLen, out IntPtr predsPtr);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterSaveModel(
        SafeBoosterHandle bHandle, string fileName);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterLoadModel(
        SafeBoosterHandle bHandle, string fileName);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterLoadModelFromBuffer(
        IntPtr bHandle, IntPtr buffer, int length);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterSaveModelToBuffer(
        SafeBoosterHandle bHandle, byte[] jsonConfig, out ulong outLen,
        out SafeBufferHandle outDptr);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGDMatrixCreateFromFile(
        string fname, int silent,
        out SafeDMatrixHandle DMtrxHandle);

    [DllImport(XGBoostNtvDllName)]
    public static extern int XGBoosterDumpModel(
        SafeBoosterHandle handle, string fmap,
        int with_stats, out int out_len,
        out IntPtr dumpStr);
}
