using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using static XGBoostSharp.Lib.ParameterNames;

namespace XGBoostSharp.Lib;

public class Booster : IDisposable
{
    readonly SafeBoosterHandle m_safeBoosterHandle;
    const int NormalPrediction = 0;  // optionMask value for XGBoosterPredict
    int m_numClass = 1;

    public IntPtr Handle => m_safeBoosterHandle.DangerousGetHandle();

    public Booster(IDictionary<string, object> parameters, DMatrix train)
    {
        var dmats = new[] { train.Handle };
        var length = unchecked((ulong)dmats.Length);
        ThrowIfError(NativeMethods.XGBoosterCreate(dmats, length, out var handle));
        m_safeBoosterHandle = new SafeBoosterHandle(handle);

        SetParameters(parameters);
    }

    public Booster(DMatrix train)
    {
        var dmats = new[] { train.Handle };
        var length = unchecked((ulong)dmats.Length);
        ThrowIfError(NativeMethods.XGBoosterCreate(dmats, length, out var handle));
        m_safeBoosterHandle = new SafeBoosterHandle(handle);
    }

    public Booster(string fileName)
    {
        ThrowIfError(NativeMethods.XGBoosterCreate(null, 0, out var handle));
        ThrowIfError(NativeMethods.XGBoosterLoadModel(handle, fileName));
        m_safeBoosterHandle = new SafeBoosterHandle(handle);
    }

    public Booster(byte[] bytes)
    {
        using var handle = new SafeBufferHandle(bytes.Length);
        Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
        ThrowIfError(NativeMethods.XGBoosterCreate(null, 0, out var boosterHandle));
        ThrowIfError(NativeMethods.XGBoosterLoadModelFromBuffer(boosterHandle,
            handle.DangerousGetHandle(), bytes.Length));
        m_safeBoosterHandle = new SafeBoosterHandle(boosterHandle);
    }

    public byte[] SaveRaw(string rawFormat = ModelFormat.Ubj)
    {
        ulong outLen;
        IntPtr outDptr;
        var config = JsonSerializer.SerializeToUtf8Bytes(new { format = rawFormat });
        ThrowIfError(NativeMethods.XGBoosterSaveModelToBuffer(Handle, config, out outLen, out outDptr));

        var length = unchecked((int)outLen);
        var buffer = new byte[length];
        Marshal.Copy(outDptr, buffer, 0, length);

        return buffer;
    }

    public void Update(DMatrix train, int iteration) =>
        ThrowIfError(NativeMethods.XGBoosterUpdateOneIter(
            Handle, iteration, train.Handle));

    public float[] Predict(DMatrix test)
    {
        ulong predsLen;
        IntPtr predsPtr;
        ThrowIfError(NativeMethods.XGBoosterPredict(
            Handle, test.Handle, NormalPrediction,
                ntreeLimit: 0, training: 0, out predsLen, out predsPtr));
        return GetPredictionsArray(predsPtr, predsLen);
    }

    public Array Predict(
        DMatrix data,
        bool outputMargin = false,
        bool predLeaf = false,
        bool predContribs = false,
        bool approxContribs = false,
        bool predInteractions = false,
        bool training = false,
        (int, int) iterationRange = default,
        bool strictShape = false)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var args = new Dictionary<string, object>
        {
            { PredictionType.type, PredictionType.TypeNormal },
            { PredictionType.training, training },
            { PredictionType.iteration_begin, iterationRange.Item1 },
            { PredictionType.iteration_end, iterationRange.Item2 },
            { PredictionType.strict_shape, strictShape },
        };

        void AssignType(int t)
        {
            if ((int)args[PredictionType.type] != PredictionType.TypeNormal)
            {
                throw new InvalidOperationException("One type of prediction at a time.");
            }
            args[PredictionType.type] = t;
        }

        if (outputMargin) AssignType(PredictionType.TypeOutputMargin);
        if (predContribs)
        {
            AssignType(approxContribs
            ? PredictionType.TypePredContribsApprox
            : PredictionType.TypePredContribs);
        }

        if (predInteractions)
        {
            AssignType(approxContribs
            ? PredictionType.TypePredInteractionsApprox
            : PredictionType.TypePredInteractions);
        }

        if (predLeaf) AssignType(PredictionType.TypePredLeaf);

        var configBytes = JsonSerializer.SerializeToUtf8Bytes(args);

        ThrowIfError(NativeMethods.XGBoosterPredictFromDMatrix(
            Handle, data.Handle, configBytes, out var shapePtr, out var dims, out var predsPtr));

        return ProcessPredictions(predsPtr, shapePtr, dims);
    }

    static Array ProcessPredictions(IntPtr predsPtr, IntPtr shapePtr, ulong dims)
    {
        var shape = new long[dims];
        Marshal.Copy(shapePtr, shape, 0, (int)dims);

        var length = shape.Aggregate(1L, (acc, val) => acc * val);

        if (length > int.MaxValue)
        {
            throw new InvalidOperationException(
                "The length of the predictions array exceeds the maximum allowed size.");
        }

        var predictions = new float[length];
        Marshal.Copy(predsPtr, predictions, 0, (int)length);

        /*
         * The prediction response can be anything from a 1D float array to a 4D array.
         * It all depends on the options you pass.
         * https://xgboost.readthedocs.io/en/stable/prediction.html#prediction-options
         * This means that the return type is determined at runtime. We can't use generics.
         */
        return dims switch
        {
            1 => predictions,
            2 => Reshape(predictions, (int)shape[0], (int)shape[1]),
            3 => Reshape(predictions, (int)shape[0], (int)shape[1], (int)shape[2]),
            4 => Reshape(predictions, (int)shape[0], (int)shape[1], (int)shape[2], (int)shape[3]),
            _ => throw new InvalidOperationException("Unsupported number of dimensions.")
        };
    }

    static float[,] Reshape(float[] array, int dim1, int dim2)
    {
        var result = new float[dim1, dim2];
        Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(float));
        return result;
    }

    static float[,,] Reshape(float[] array, int dim1, int dim2, int dim3)
    {
        var result = new float[dim1, dim2, dim3];
        Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(float));
        return result;
    }

    static float[,,,] Reshape(float[] array, int dim1, int dim2, int dim3, int dim4)
    {
        var result = new float[dim1, dim2, dim3, dim4];
        Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(float));
        return result;
    }

    public static float[] GetPredictionsArray(IntPtr predsPtr, ulong predsLen)
    {
        var length = unchecked((int)predsLen);
        var preds = new float[length];
        for (var i = 0; i < length; i++)
        {
            var floatBytes = new byte[4];
            for (var b = 0; b < 4; b++)
            {
                floatBytes[b] = Marshal.ReadByte(predsPtr, 4 * i + b);
            }
            preds[i] = BitConverter.ToSingle(floatBytes, 0);
        }
        return preds;
    }

    public void SetParameters(IDictionary<string, object> parameters)
    {
        // support internationalization i.e. support floats with commas (e.g. 0,5F)
        var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        foreach (var kvp in parameters)
        {
            var valueAsString = kvp.Value switch
            {
                int intValue => intValue.ToString(),
                float floatValue => floatValue.ToString(nfi),
                bool boolValue => boolValue.ToString(),
                string stringValue => stringValue,
                _ => throw new ArgumentException($"Unsupported parameter type: {kvp.Value.GetType()}")
            };

            SetParameter(kvp.Key, valueAsString);
        }

        if (parameters.TryGetValue(num_class, out var value))
        {
            m_numClass = (int)value;
            SetParameter(num_class, m_numClass.ToString());
        }
    }

    public static void PrintParameters(IDictionary<string, object> parameters)
    {
        foreach (var kvp in parameters)
        {
            var valueAsString = kvp.Value switch
            {
                int intValue => intValue.ToString(),
                float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
                bool boolValue => boolValue.ToString(),
                string stringValue => stringValue,
                _ => throw new ArgumentException($"Unsupported parameter type: {kvp.Value.GetType()}")
            };

            Console.WriteLine($"{kvp.Key}: {valueAsString}");
        }
    }

    public void SetParameter(string name, string val) =>
        ThrowIfError(NativeMethods.XGBoosterSetParam(Handle, name, val));

    public void Save(string fileName) =>
        ThrowIfError(NativeMethods.XGBoosterSaveModel(Handle, fileName));

    public string[] DumpModelEx(string fmap, int with_stats)
    {
        int length;
        IntPtr treePtr;
        var intptrSize = IntPtr.Size;
        ThrowIfError(NativeMethods.XGBoosterDumpModel(Handle,
            fmap, with_stats, out length, out treePtr));
        var trees = new string[length];
        var handle2 = GCHandle.Alloc(treePtr, GCHandleType.Pinned);

        for (var i = 0; i < length; i++)
        {
            var ipt1 = Marshal.ReadIntPtr(Marshal.ReadIntPtr(handle2.AddrOfPinnedObject()), intptrSize * i);
            var s = Marshal.PtrToStringAnsi(ipt1);
            trees[i] = $"booster[{i}]\n{s}";
        }
        handle2.Free();
        return trees;
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
        m_safeBoosterHandle?.Dispose();
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
