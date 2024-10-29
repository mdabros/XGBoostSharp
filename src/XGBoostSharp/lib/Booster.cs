using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using static XGBoostSharp.lib.ParameterNames;

namespace XGBoostSharp.lib;

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

