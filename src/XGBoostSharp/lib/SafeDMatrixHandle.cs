using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class SafeDMatrixHandle : SafeHandle
{
    public SafeDMatrixHandle() : base(IntPtr.Zero, true) { }

    public SafeDMatrixHandle(IntPtr handle) : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.XGDMatrixFree(handle);
        return true;
    }
}
