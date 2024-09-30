namespace XGBoostSharp;

/// <summary>
/// Controls a way new nodes are added to the tree.
/// Currently supported only if tree_method is set to hist or approx.
/// </summary>
public static class GrowPolicy
{
    /// <summary>
    /// split at nodes closest to the root.
    /// </summary>
    public const string DepthWise = "depthwise";
    /// <summary>
    /// split at nodes with highest loss change
    /// </summary>
    public const string Lossguide = "lossguide";
}
