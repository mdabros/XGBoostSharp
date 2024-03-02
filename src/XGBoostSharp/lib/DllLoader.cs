using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib
{

    public class DllLoader
    {

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)] public static extern IntPtr LoadLibrary(string lpFileName);
    }
}