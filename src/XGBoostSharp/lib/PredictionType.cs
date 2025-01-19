namespace XGBoostSharp.lib;

public static class PredictionType
{
#pragma warning disable IDE1006 // Naming Styles
    public const string type = nameof(type);
    public const string strict_shape = nameof(strict_shape);
    public const string iteration_begin = nameof(iteration_begin);
    public const string iteration_end = nameof(iteration_end);
    public const string training = nameof(training);
#pragma warning restore IDE1006 // Naming Styles

    // The actual values of the prediction type
    public const int TypeNormal = 0;
    public const int TypeOutputMargin = 1;
    public const int TypePredContribs = 2;
    public const int TypePredContribsApprox = 3;
    public const int TypePredInteractions = 4;
    public const int TypePredInteractionsApprox = 5;
    public const int TypePredLeaf = 6;
}
