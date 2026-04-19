using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace XGBoostSharp.Lib;

public class DMatrix : IDisposable
{
    readonly SafeDMatrixHandle m_safeDMatrixHandle;
    const float DefaultMissing = float.NaN;

    public IntPtr Handle => m_safeDMatrixHandle.DangerousGetHandle();

    public float[] Label
    {
        get => GetFloatInfo(Fields.label);
        set => SetFloatInfo(Fields.label, value);
    }

    /// <summary>
    /// Creates a DMatrix from a 2D array of float values with optional labels.
    /// </summary>
    /// <param name="data">The 2D array containing the feature data.</param>
    /// <param name="labels">Optional array of labels corresponding to the data rows. Default is null.</param>
    /// <param name="missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(float[][] data, float[] labels = null, float missing = DefaultMissing)
        : this(Flatten2DArray(data), (ulong)data.Length, (ulong)data[0].Length, labels, missing)
    {
    }

    /// <summary>
    /// Creates a DMatrix from a 2D array of float values with multi-output labels, where each row
    /// of <paramref name="labels"/> contains the target values for one sample.
    /// XGBoost infers the number of outputs from <c>labels.Length * labels[0].Length / nrows</c>.
    /// </summary>
    /// <param name="data">The 2D array containing the feature data.</param>
    /// <param name="labels">2D array of labels shaped <c>[n_samples, n_outputs]</c>.</param>
    /// <param name="missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(float[][] data, float[][] labels, float missing = DefaultMissing)
        : this(Flatten2DArray(data), (ulong)data.Length, (ulong)data[0].Length, labels, missing)
    {
    }

    /// <summary>
    /// Creates a DMatrix from a 1D array of float values with specified dimensions and optional labels.
    /// </summary>
    /// <param name="data1D">The 1D array containing the feature data in row-major order.</param>
    /// <param name="nrows">Number of rows in the matrix.</param>
    /// <param name="ncols">Number of columns in the matrix.</param>
    /// <param name="labels">Optional array of labels corresponding to the data rows. Default is null.</param>
    /// <param name="missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(float[] data1D, ulong nrows, ulong ncols, float[] labels = null, float missing = DefaultMissing)
    {
        var output = NativeMethods.XGDMatrixCreateFromMat(
            data1D, nrows, ncols, missing, out var handle);

        ThrowIfError(output);
        m_safeDMatrixHandle = new SafeDMatrixHandle(handle);

        if (labels != null)
        {
            Label = labels;
        }
    }

    /// <summary>
    /// Creates a DMatrix from a 1D array of float values with specified dimensions and multi-output
    /// labels, where each row of <paramref name="labels"/> contains the target values for one sample.
    /// XGBoost infers the number of outputs from <c>labels.Length * labels[0].Length / nrows</c>.
    /// </summary>
    /// <param name="data1D">The 1D array containing the feature data in row-major order.</param>
    /// <param name="nrows">Number of rows in the matrix.</param>
    /// <param name="ncols">Number of columns in the matrix.</param>
    /// <param name="labels">2D array of labels shaped <c>[n_samples, n_outputs]</c>.</param>
    /// <param name="missing">Value to be treated as missing in the dataset. Default is float.NaN.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public DMatrix(float[] data1D, ulong nrows, ulong ncols, float[][] labels, float missing = DefaultMissing)
        : this(data1D, nrows, ncols, labels != null ? Flatten2DArray(labels) : null, missing)
    {
    }

    /// <summary>
    /// Creates a DMatrix from a CSV file. The file path is passed to XGBoost as
    /// <c>filePath?format=csv</c> per the XGBoost URI convention.
    /// </summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <param name="labelColumn">
    /// Zero-based index of the column to use as the label. When <c>null</c> (default),
    /// no label column is inferred from the file; labels can be assigned afterwards via
    /// <see cref="Label"/>.
    /// </param>
    /// <param name="silent">If <c>true</c>, suppresses XGBoost loading messages.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public static DMatrix FromCsvFile(string filePath, int? labelColumn = null, bool silent = true)
    {
        var uri = labelColumn.HasValue
            ? filePath + "?format=csv&label_column=" + labelColumn.Value
            : filePath + "?format=csv";
        return FromFile(uri, silent);
    }

    /// <summary>
    /// Creates a DMatrix from a LIBSVM file. The file path is passed to XGBoost as
    /// <c>filePath?format=libsvm</c> per the XGBoost URI convention.
    /// </summary>
    /// <param name="filePath">Path to the LIBSVM file.</param>
    /// <param name="silent">If <c>true</c>, suppresses XGBoost loading messages.</param>
    /// <exception cref="DllFailException">Thrown when the native XGBoost library encounters an error during matrix creation.</exception>
    public static DMatrix FromLibSvmFile(string filePath, bool silent = true) =>
        FromFile(filePath + "?format=libsvm", silent);

    public void SetFeatureNames(string[] featureNames) => SetFeatureInfo(featureNames, Fields.feature_name);

    public string[] GetFeatureNames() => GetFeatureInfo(Fields.feature_name);

    public void SetFeatureTypes(string[] featureTypes) => SetFeatureInfo(featureTypes, Fields.feature_type);

    public string[] GetFeatureTypes() => GetFeatureInfo(Fields.feature_type);

    static DMatrix FromFile(string uri, bool silent)
    {
        var output = NativeMethods.XGDMatrixCreateFromFile(uri, silent ? 1 : 0, out var handle);
        ThrowIfError(output);
        return new DMatrix(new SafeDMatrixHandle(handle));
    }

    DMatrix(SafeDMatrixHandle handle)
    {
        m_safeDMatrixHandle = handle;
    }

    static float[] Flatten2DArray(float[][] data2D) =>
        data2D.SelectMany(row => row).ToArray();

    float[] GetFloatInfo(string field)
    {
        var output = NativeMethods.XGDMatrixGetFloatInfo(
            Handle, field, out var lengthULong, out var result);
        ThrowIfError(output);

        var length = (int)lengthULong;
        var floatInfo = new float[length];
        Marshal.Copy(result, floatInfo, 0, length);

        return floatInfo;
    }

    void SetFloatInfo(string field, float[] floatInfo)
    {
        var length = (ulong)floatInfo.Length;
        var output = NativeMethods.XGDMatrixSetFloatInfo(Handle, field, floatInfo, length);
        ThrowIfError(output);
    }

    void SetFeatureInfo(string[] featureInfo, string field)
    {
        var length = (ulong)featureInfo.Length;
        var output = NativeMethods.XGDMatrixSetStrFeatureInfo(Handle, field, featureInfo, length);
        ThrowIfError(output);
    }

    string[] GetFeatureInfo(string field)
    {
        var output = NativeMethods.XGDMatrixGetStrFeatureInfo(Handle, field, out var lengthULong, out var result);
        ThrowIfError(output);

        var length = (int)lengthULong;
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
        if (output != 0)
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
