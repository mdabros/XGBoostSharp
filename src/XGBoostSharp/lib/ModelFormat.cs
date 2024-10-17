namespace XGBoostSharp.lib;

public enum ModelFormat
{
    Json,
    Ubj
}

public static class ModelFormatExtensions
{
    public static string ToLowerString(this ModelFormat format)
    {
        return format.ToString().ToLower();
    }
}
