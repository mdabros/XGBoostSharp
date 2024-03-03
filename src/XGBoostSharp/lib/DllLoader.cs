using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public static class DllLoader
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibrary(string lpFileName);
}
