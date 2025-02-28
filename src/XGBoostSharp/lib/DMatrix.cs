using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace XGBoostSharp.lib;

public class DMatrix : IDisposable
{
    readonly SafeDMatrixHandle m_safeDMatrixHandle;
    const float DefaultMissing = float.NaN;

    public IntPtr Handle => m_safeDMatrixHandle.DangerousGetHandle();

    public float[] Label
    {
        get { return GetFloatInfo(Fields.label); }
        set { SetFloatInfo(Fields.label, value); }
    }

    /// <summary>
    /// Creates a DMatrix from a 2D array of float values with optional labels.
    /// </summary>
    /// <param name="data">The 2D array containing the feature data.</param>
    /// <param name="labels">Optional array of labels corresponding to the data rows. Default is null.</param>
    /// <param name="m_missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(float[][] data, float[] labels = null, float m_missing = DefaultMissing)
        : this(
            Flatten2DArray(data),
            unchecked((ulong)data.Length),
            unchecked((ulong)data[0].Length),
            labels,
            m_missing
        )
    { }

    /// <summary>
    /// Creates a DMatrix from a 1D array of float values with specified dimensions and optional labels.
    /// </summary>
    /// <param name="data1D">The 1D array containing the feature data in row-major order.</param>
    /// <param name="nrows">Number of rows in the matrix.</param>
    /// <param name="ncols">Number of columns in the matrix.</param>
    /// <param name="labels">Optional array of labels corresponding to the data rows. Default is null.</param>
    /// <param name="m_missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(
        float[] data1D,
        ulong nrows,
        ulong ncols,
        float[] labels = null,
        float m_missing = DefaultMissing
    )
    {
        var output = NativeMethods.XGDMatrixCreateFromMat(
            data1D,
            nrows,
            ncols,
            m_missing,
            out var handle
        );

        ThrowIfError(output);
        m_safeDMatrixHandle = new SafeDMatrixHandle(handle);

        if (labels != null)
        {
            Label = labels;
        }
    }

    static float[][] ReplaceNullValues(float?[][] data, float m_missing) =>
        data.Select(r => ReplaceNullValues(r, m_missing)).ToArray();

    static float[] ReplaceNullValues(float?[] labels, float m_missing) =>
        labels?.Select(val => val ?? m_missing).ToArray();

    static float[] Flatten2DArray(float[][] data2D) =>
        data2D.SelectMany(row => row).ToArray();

    float[] GetFloatInfo(string field)
    {
        ulong lengthULong;
        IntPtr result;
        var output = NativeMethods.XGDMatrixGetFloatInfo(Handle,
            field, out lengthULong, out result);

        ThrowIfError(output);

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
        var output = NativeMethods.XGDMatrixSetFloatInfo(Handle, field, floatInfo, length);
        ThrowIfError(output);
    }

    public void SetFeatureNames(string[] featureNames) => SetFeatureInfo(featureNames, Fields.feature_name);

    public string[] GetFeatureNames() => GetFeatureInfo(Fields.feature_name);

    public void SetFeatureTypes(string[] featureTypes) => SetFeatureInfo(featureTypes, Fields.feature_type);

    public string[] GetFeatureTypes() => GetFeatureInfo(Fields.feature_type);

    void SetFeatureInfo(string[] featureInfo, string field)
    {
        var length = (ulong)featureInfo.Length;
        var output = NativeMethods.XGDMatrixSetStrFeatureInfo(
            Handle, field, featureInfo, length);
        ThrowIfError(output);
    }

    string[] GetFeatureInfo(string field)
    {
        var output = NativeMethods.XGDMatrixGetStrFeatureInfo(
            Handle, field, out var lengthULong, out var result);
        ThrowIfError(output);

        var length = unchecked((int)lengthULong);
        var featureInfo = new string[length];
        for (var i = 0; i < length; i++)
        {
            IntPtr strPtr = Marshal.ReadIntPtr(result, i * IntPtr.Size);
            featureInfo[i] = Marshal.PtrToStringAnsi(strPtr);
        }

        return featureInfo;
    }

    static void ThrowIfError(int output)
    {
        if (output == -1)
        {
            throw new DllFailException(NativeMethods.XGBGetLastError());
        }
    }

    void DisposeManagedResources()
    {
        m_safeDMatrixHandle?.Dispose();
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
