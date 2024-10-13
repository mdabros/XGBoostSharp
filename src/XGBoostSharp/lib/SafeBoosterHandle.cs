using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

// See: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
// See: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/unmanaged
public class SafeBoosterHandle : SafeHandle
{
    public SafeBoosterHandle() : base(IntPtr.Zero, true) { }

    public SafeBoosterHandle(IntPtr handle) : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.XGBoosterFree(handle);
        return true;
    }
}
