using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public static class XGBOOST_NATIVE_METHODS
{
    const string DllLocation = @"runtimes\win-x64\native\xgboost.dll";

    static XGBOOST_NATIVE_METHODS() { DllLoader.LoadLibrary(DllLocation); }

    [DllImport(DllLocation)]
    public static extern string XGBGetLastError();

    [DllImport(DllLocation)]
    public static extern int XGDMatrixCreateFromMat(float[] data, ulong nrow, ulong ncol,
                                                    float missing, out IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGDMatrixFree(IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGDMatrixGetFloatInfo(IntPtr handle, string field,
                                                   out ulong len, out IntPtr result);

    [DllImport(DllLocation)]
    public static extern int XGDMatrixSetFloatInfo(IntPtr handle, string field,
                                                   float[] array, ulong len);

    [DllImport(DllLocation)]
    public static extern int XGBoosterCreate(IntPtr[] dmats,
                                             ulong len, out IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGBoosterFree(IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGBoosterSetParam(IntPtr handle, string name, string val);

    [DllImport(DllLocation)]
    public static extern int XGBoosterUpdateOneIter(IntPtr bHandle, int iter,
                                                    IntPtr dHandle);

    [DllImport(DllLocation)]
    public static extern int XGBoosterPredict(IntPtr bHandle, IntPtr dHandle,
                                              int optionMask, int ntreeLimit,
                                              out ulong predsLen, out IntPtr predsPtr);

    [DllImport(DllLocation)]
    public static extern int XGBoosterSaveModel(IntPtr bHandle, string fileName);

    [DllImport(DllLocation)]
    public static extern int XGBoosterLoadModel(IntPtr bHandle, string fileName);

    [DllImport(DllLocation)]
    public static extern int XGDMatrixCreateFromFile(string fname, int silent, out IntPtr DMtrxHandle);

    [DllImport(DllLocation)]
    public static extern int XGBoosterDumpModel(IntPtr handle, string fmap, int with_stats, out int out_len, out IntPtr dumpStr);
}
