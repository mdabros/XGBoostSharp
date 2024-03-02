﻿using System;
using System.Collections.Generic;
using XGBoostSharp.lib;

namespace XGBoostSharp;

public class BaseXgbModel : IDisposable
{
    protected IDictionary<string, object> parameters = new Dictionary<string, object>();
    protected Booster booster;

    public void SaveModelToFile(string fileName)
    {
        booster.Save(fileName);
    }

    public static XGBClassifier LoadClassifierFromFile(string fileName)
    {
        return new XGBClassifier { booster = new Booster(fileName) };
    }

    public static XGBRegressor LoadRegressorFromFile(string fileName)
    {
        return new XGBRegressor { booster = new Booster(fileName) };
    }

    public string[] DumpModelEx(string fmap = "",
      int with_stats = 0,
      string format = "json")
    {
        return booster.DumpModelEx(fmap, with_stats, format);
    }

    void DisposeManagedResources()
    {
        if (booster != null)
        {
            booster.Dispose();
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
    virtual protected void Dispose(bool disposing)
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
