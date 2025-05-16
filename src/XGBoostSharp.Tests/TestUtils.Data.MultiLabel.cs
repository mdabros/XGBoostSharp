namespace XGBoostSharp.Test;

public static partial class TestUtils
{
    public static float[][] DataTrainMultiLabel =>
        [
            [1.0f, 2.0f],
            [1.1f, 2.2f],
            [2.0f, 1.0f],
            [2.1f, 1.1f],
            [3.0f, 3.0f],
            [3.1f, 3.1f],
        ];

    public static float[] LabelsTrainMultiLabel =>
        [0, 0, 1, 1, 2, 2];
}
