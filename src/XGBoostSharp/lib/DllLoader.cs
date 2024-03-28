using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public static class DllLoader
{
    public static void LoadNativeLibrary()
    {
        var libraryPath = GetLibraryPath();
        LoadLibrary(libraryPath);
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibrary(string lpFileName);

    static string GetLibraryPath()
    {
        return RuntimeInformation.OSDescription switch
        {
            var os when os.Contains("Windows") => @"runtimes\win-x64\native\xgboost.dll",
            var os when os.Contains("Linux") => @"runtimes/linux-x64/native/libxgboost.so",
            var os when os.Contains("Darwin") => @"runtimes/osx-x64/native/libxgboost.dylib",
            _ => throw new PlatformNotSupportedException("x64 for windows, Linux, and OSX is supported")
        };
    }
}
