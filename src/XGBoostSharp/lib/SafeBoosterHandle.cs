using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

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
