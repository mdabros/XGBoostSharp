using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using static XGBoostSharp.lib.ParameterNames;

namespace XGBoostSharp.lib;

public class Booster : IDisposable
{
    readonly IntPtr m_handle;
    const int NormalPrediction = 0;  // optionMask value for XGBoosterPredict
    int m_numClass = 1;

    public IntPtr Handle => m_handle;

    public Booster(IDictionary<string, object> parameters, DMatrix train)
    {
        var dmats = new[] { train.Handle };
        var length = unchecked((ulong)dmats.Length);
        ThrowIfError(NativeMethods.XGBoosterCreate(dmats, length, out m_handle));

        SetParameters(parameters);
    }

    public Booster(DMatrix train)
    {
        var dmats = new[] { train.Handle };
        var length = unchecked((ulong)dmats.Length);
        var output = NativeMethods.XGBoosterCreate(dmats, length, out m_handle);
        ThrowIfError(output);
    }

    public Booster(string fileName)
    {
        IntPtr tempPtr;
        ThrowIfError(NativeMethods.XGBoosterCreate(null, 0, out tempPtr));
        ThrowIfError(NativeMethods.XGBoosterLoadModel(tempPtr, fileName));
        m_handle = tempPtr;
    }

    public void Update(DMatrix train, int iteration) =>
        ThrowIfError(NativeMethods.XGBoosterUpdateOneIter(Handle, iteration, train.Handle));

    public float[] Predict(DMatrix test)
    {
        ulong predsLen;
        IntPtr predsPtr;
        ThrowIfError(NativeMethods.XGBoosterPredict(
            m_handle, test.Handle, NormalPrediction, ntreeLimit: 0, training: 0, out predsLen, out predsPtr));
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
        // support internationalisation i.e. support floats with commas (e.g. 0,5F)
        var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

        SetParameter(max_depth, ((int)parameters[max_depth]).ToString());
        SetParameter(learning_rate, ((float)parameters[learning_rate]).ToString(nfi));
        SetParameter(n_estimators, ((int)parameters[n_estimators]).ToString());
        SetParameter(silent, ((bool)parameters[silent]).ToString());
        SetParameter(objective, (string)parameters[objective]);
        SetParameter(booster, (string)parameters[booster]);
        SetParameter(tree_method, (string)parameters[tree_method]);

        SetParameter(nthread, ((int)parameters[nthread]).ToString());
        SetParameter(gamma, ((float)parameters[gamma]).ToString(nfi));
        SetParameter(min_child_weight, ((int)parameters[min_child_weight]).ToString());
        SetParameter(max_delta_step, ((int)parameters[max_delta_step]).ToString());
        SetParameter(subsample, ((float)parameters[subsample]).ToString(nfi));
        SetParameter(colsample_bytree, ((float)parameters[colsample_bytree]).ToString(nfi));
        SetParameter(colsample_bylevel, ((float)parameters[colsample_bylevel]).ToString(nfi));
        SetParameter(reg_alpha, ((float)parameters[reg_alpha]).ToString(nfi));
        SetParameter(reg_lambda, ((float)parameters[reg_lambda]).ToString(nfi));
        SetParameter(scale_pos_weight, ((float)parameters[scale_pos_weight]).ToString(nfi));

        SetParameter(base_score, ((float)parameters[base_score]).ToString(nfi));
        SetParameter(seed, ((int)parameters[seed]).ToString());
        SetParameter(missing, ((float)parameters[missing]).ToString(nfi));

        SetParameter(sample_type, (string)parameters[sample_type]);
        SetParameter(normalize_type, (string)parameters[normalize_type]);
        SetParameter(rate_drop, ((float)parameters[rate_drop]).ToString(nfi));
        SetParameter(one_drop, ((int)parameters[one_drop]).ToString());
        SetParameter(skip_drop, ((float)parameters[skip_drop]).ToString(nfi));

        if (parameters.TryGetValue(num_class, out var value))
        {
            m_numClass = (int)value;
            SetParameter(num_class, m_numClass.ToString());
        }
    }

    public static void PrintParameters(IDictionary<string, object> parameters)
    {
        Console.WriteLine($"{nameof(max_depth)}: {(int)parameters[max_depth]}");
        Console.WriteLine($"{nameof(learning_rate)}: {(float)parameters[learning_rate]}");
        Console.WriteLine($"{nameof(n_estimators)}: {(int)parameters[n_estimators]}");
        Console.WriteLine($"{nameof(silent)}: {(bool)parameters[silent]}");
        Console.WriteLine($"{nameof(objective)}: {(string)parameters[objective]}");
        Console.WriteLine($"{nameof(booster)}: {(string)parameters[booster]}");
        Console.WriteLine($"{nameof(tree_method)}: {(string)parameters[tree_method]}");

        Console.WriteLine($"{nameof(nthread)}: {(int)parameters[nthread]}");
        Console.WriteLine($"{nameof(gamma)}: {(float)parameters[gamma]}");
        Console.WriteLine($"{nameof(min_child_weight)}: {(int)parameters[min_child_weight]}");
        Console.WriteLine($"{nameof(max_delta_step)}: {(int)parameters[max_delta_step]}");
        Console.WriteLine($"{nameof(subsample)}: {(float)parameters[subsample]}");
        Console.WriteLine($"{nameof(colsample_bytree)}: {(float)parameters[colsample_bytree]}");
        Console.WriteLine($"{nameof(colsample_bylevel)}: {(float)parameters[colsample_bylevel]}");
        Console.WriteLine($"{nameof(reg_alpha)}: {(float)parameters[reg_alpha]}");
        Console.WriteLine($"{nameof(reg_lambda)}: {(float)parameters[reg_lambda]}");
        Console.WriteLine($"{nameof(scale_pos_weight)}: {(float)parameters[scale_pos_weight]}");

        Console.WriteLine($"{nameof(base_score)}: {(float)parameters[base_score]}");
        Console.WriteLine($"{nameof(seed)}: {(int)parameters[seed]}");
        Console.WriteLine($"{nameof(missing)}: {(float)parameters[missing]}");

        Console.WriteLine($"{nameof(sample_type)}: {(float)parameters[sample_type]}");
        Console.WriteLine($"{nameof(normalize_type)}: {(float)parameters[normalize_type]}");
        Console.WriteLine($"{nameof(rate_drop)}: {(float)parameters[rate_drop]}");
        Console.WriteLine($"{nameof(one_drop)}: {(int)parameters[one_drop]}");
        Console.WriteLine($"{nameof(skip_drop)}: {(float)parameters[skip_drop]}");
    }

    public void SetParameter(string name, string val) =>
        ThrowIfError(NativeMethods.XGBoosterSetParam(m_handle, name, val));

    public void Save(string fileName) =>
        NativeMethods.XGBoosterSaveModel(m_handle, fileName);

    public string[] DumpModelEx(string fmap, int with_stats)
    {
        int length;
        IntPtr treePtr;
        var intptrSize = IntPtr.Size;
        NativeMethods.XGBoosterDumpModel(m_handle, fmap, with_stats, out length, out treePtr);
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

    void DisposeManagedResources() =>
        NativeMethods.XGBoosterFree(m_handle);

    static void ThrowIfError(int output)
    {
        if (output == -1)
        {
            throw new DllFailException(NativeMethods.XGBGetLastError());
        }
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
