using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class DMatrix : IDisposable
{
    bool m_disposed = false;
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
        ulong lenULong;
        IntPtr result;
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixGetFloatInfo(m_handle, field, out lenULong, out result);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());

        var len = unchecked((int)lenULong);
        var floatInfo = new float[len];
        for (var i = 0; i < len; i++)
        {
            var floatBytes = new byte[4];
            floatBytes[0] = Marshal.ReadByte(result, 4 * i + 0);
            floatBytes[1] = Marshal.ReadByte(result, 4 * i + 1);
            floatBytes[2] = Marshal.ReadByte(result, 4 * i + 2);
            floatBytes[3] = Marshal.ReadByte(result, 4 * i + 3);
            var f = BitConverter.ToSingle(floatBytes, 0);
            floatInfo[i] = f;
        }
        return floatInfo;
    }

    void SetFloatInfo(string field, float[] floatInfo)
    {
        var len = (ulong)floatInfo.Length;
        var output = XGBOOST_NATIVE_METHODS.XGDMatrixSetFloatInfo(m_handle, field, floatInfo, len);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
    }

    // Dispose pattern from MSDN documentation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Dispose pattern from MSDN documentation
    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        var output = XGBOOST_NATIVE_METHODS.XGDMatrixFree(m_handle);
        if (output == -1)
            throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());

        m_disposed = true;
    }
}
