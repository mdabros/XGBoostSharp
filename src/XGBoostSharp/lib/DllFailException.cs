using System;

namespace XGBoostSharp.lib;

class DllFailException : Exception
{
    public DllFailException()
    {
    }

    public DllFailException(string message) : base(message)
    {
    }
}
