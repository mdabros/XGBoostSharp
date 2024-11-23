using System;
using System.Collections.Generic;
using XGBoostSharp.lib;

namespace XGBoostSharp;

public abstract class XGBModelBase : IDisposable
{
    protected IDictionary<string, object> m_parameters = new Dictionary<string, object>();
    protected Booster m_booster;

    protected Booster Train(IDictionary<string, object> parameters, DMatrix dTrain)
    {
        var iterations = (int)m_parameters[ParameterNames.n_estimators];
        var booster = new Booster(parameters, dTrain);
        for (var i = 0; i < iterations; i++)
        {
            booster.Update(dTrain, i);
        }
        return booster;
    }

    // Note that file name extension decides which format the model is saved as.
    // Options are *.json and *.ubj. *.json is recommended.
    // See https://xgboost.readthedocs.io/en/stable/tutorials/saving_model.html
    public void SaveModelToFile(string fileName) =>
        m_booster.Save(fileName);

    public byte[] SaveModelToByteArray(string format) =>
        m_booster.SaveRaw(format);

    public string[] DumpModelEx(string fmap = "", int with_stats = 0) =>
        m_booster.DumpModelEx(fmap, with_stats);

    void DisposeManagedResources()
    {
        m_booster?.Dispose();
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
