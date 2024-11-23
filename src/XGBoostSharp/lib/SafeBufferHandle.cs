using System;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class SafeBufferHandle : SafeHandle
{
    public SafeBufferHandle() : base(IntPtr.Zero, true) { }

    public SafeBufferHandle(int size) : base(IntPtr.Zero, true)
    {
        SetHandle(Marshal.AllocHGlobal(size));
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
        }
        return true;
    }
}
