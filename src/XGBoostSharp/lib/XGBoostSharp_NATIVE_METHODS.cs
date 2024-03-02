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
    public static extern int XGBoostSharperCreate(IntPtr[] dmats,
                                             ulong len, out IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperFree(IntPtr handle);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperSetParam(IntPtr handle, string name, string val);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperUpdateOneIter(IntPtr bHandle, int iter,
                                                    IntPtr dHandle);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperPredict(IntPtr bHandle, IntPtr dHandle,
                                              int optionMask, int ntreeLimit,
                                              out ulong predsLen, out IntPtr predsPtr);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperSaveModel(IntPtr bHandle, string fileName);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperLoadModel(IntPtr bHandle, string fileName);

    [DllImport(DllLocation)]
    public static extern int XGDMatrixCreateFromFile(string fname, int silent, out IntPtr DMtrxHandle);

    [DllImport(DllLocation)]
    public static extern int XGBoostSharperDumpModel(IntPtr handle, string fmap, int with_stats, out int out_len, out IntPtr dumpStr);
}
