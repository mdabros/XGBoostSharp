using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class DMatrix : IDisposable
{
    readonly IntPtr m_handle;
    readonly float m_missing = -1.0F; // arbitrary value used to represent a missing value

    public IntPtr Handle
    {
        get { return m_handle; }
    }

    public float[] Label
    {
        get { return GetFloatInfo("label"); }
        set { SetFloatInfo("label", value); }
    }

    public DMatrix(float[][] data, float[] labels = null)
        : this(Flatten2DArray(data), unchecked((ulong)data.Length), unchecked((ulong)data[0].Length), labels)
    {
    }

    public DMatrix(float[] data1D, ulong nrows, ulong ncols, float[] labels = null)
    {
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixCreateFromMat(data1D, nrows, ncols, m_missing, out m_handle);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());

        if (labels != null)
        {
            Label = labels;
        }
    }

    static float[] Flatten2DArray(float[][] data2D) =>
        data2D.SelectMany(row => row).ToArray();

    float[] GetFloatInfo(string field)
    {
        ulong lengthULong;
        IntPtr result;
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixGetFloatInfo(m_handle, field, out lengthULong, out result);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());

        var length = unchecked((int)lengthULong);
        var floatInfo = new float[length];
        var floatBytes = new byte[length * 4];
        Marshal.Copy(result, floatBytes, 0, floatBytes.Length);

        for (var i = 0; i < length; i++)
        {
            floatInfo[i] = BitConverter.ToSingle(floatBytes, i * 4);
        }

        return floatInfo;
    }

    void SetFloatInfo(string field, float[] floatInfo)
    {
        var length = (ulong)floatInfo.Length;
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixSetFloatInfo(m_handle, field, floatInfo, length);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
    }

    void DisposeManagedResources()
    {
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixFree(m_handle);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
    }

    #region Dispose
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Dispose(bool disposing) executes in two distinct scenarios.
    // If disposing equals true, the method has been called directly
    // or indirectly by a user's code. Managed and unmanaged resources
    // can be disposed.
    // If disposing equals false, the method has been called by the 
    // runtime from inside the finalizer and you should not reference 
    // other objects. Only unmanaged resources can be disposed.
    protected virtual void Dispose(bool disposing)
    {
        // Dispose only if we have not already disposed.
        if (!m_disposed)
        {
            // If disposing equals true, dispose all managed and unmanaged resources.
            // I.e. dispose managed resources only if true, unmanaged always.
            if (disposing)
            {
                DisposeManagedResources();
            }

            // Call the appropriate methods to clean up unmanaged resources here.
            // If disposing is false, only the following code is executed.
        }
        m_disposed = true;
    }

    volatile bool m_disposed = false;
    #endregion
}
