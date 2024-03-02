using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class XGBOOST_NATIVE_METHODS
{
    private const string dllLocation = @"runtimes\win-x64\native\xgboost.dll";

    static XGBOOST_NATIVE_METHODS() { DllLoader.LoadLibrary(dllLocation); }

    [DllImport(dllLocation)]
    public static extern string XGBGetLastError();

    [DllImport(dllLocation)]
    public static extern int XGDMatrixCreateFromMat(float[] data, ulong nrow, ulong ncol,
                                                    float missing, out IntPtr handle);

    [DllImport(dllLocation)]
    public static extern int XGDMatrixFree(IntPtr handle);

    [DllImport(dllLocation)]
    public static extern int XGDMatrixGetFloatInfo(IntPtr handle, string field,
                                                   out ulong len, out IntPtr result);

    [DllImport(dllLocation)]
    public static extern int XGDMatrixSetFloatInfo(IntPtr handle, string field,
                                                   float[] array, ulong len);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperCreate(IntPtr[] dmats,
                                             ulong len, out IntPtr handle);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperFree(IntPtr handle);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperSetParam(IntPtr handle, string name, string val);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperUpdateOneIter(IntPtr bHandle, int iter,
                                                    IntPtr dHandle);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperPredict(IntPtr bHandle, IntPtr dHandle,
                                              int optionMask, int ntreeLimit,
                                              out ulong predsLen, out IntPtr predsPtr);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperSaveModel(IntPtr bHandle, string fileName);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperLoadModel(IntPtr bHandle, string fileName);

    [DllImport(dllLocation)]
    public static extern int XGDMatrixCreateFromFile(string fname, int silent, out IntPtr DMtrxHandle);

    [DllImport(dllLocation)]
    public static extern int XGBoostSharperDumpModel(IntPtr handle, string fmap, int with_stats, out int out_len, out IntPtr dumpStr);
}
