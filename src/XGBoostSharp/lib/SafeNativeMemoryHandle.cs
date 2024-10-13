using System;
using System.Runtime.InteropServices;

public class SafeNativeMemoryHandle : SafeHandle
{
    public SafeNativeMemoryHandle() : base(IntPtr.Zero, true) { }

    public SafeNativeMemoryHandle(IntPtr handle) : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            // Ensure this actually frees what is allocated by XGBoosterFeatureScore.
            Marshal.FreeHGlobal(handle);
        }
        return true;
    }
}
