namespace XGBoostSharp.Test;

public static partial class TestUtils
{
    // 20 samples, 2 features. Used for both multi-output regression and multi-label classification.
    public static float[][] DataTrainMultiOutput =>
        [
            [0.1f, 0.2f],
            [0.3f, 0.8f],
            [0.9f, 0.4f],
            [0.5f, 0.6f],
            [0.2f, 0.9f],
            [0.8f, 0.1f],
            [0.4f, 0.7f],
            [0.7f, 0.3f],
            [0.6f, 0.5f],
            [0.0f, 1.0f],
            [1.0f, 0.0f],
            [0.15f, 0.85f],
            [0.85f, 0.15f],
            [0.45f, 0.55f],
            [0.55f, 0.45f],
            [0.25f, 0.75f],
            [0.75f, 0.25f],
            [0.35f, 0.65f],
            [0.65f, 0.35f],
            [0.5f, 0.5f],
        ];

    // Multi-output regression labels: [x0 + x1, x0 - x1 + 1].
    public static float[][] LabelsTrainMultiOutputRegression =>
        [
            [0.3f,  1.9f],
            [1.1f,  1.5f],
            [1.3f,  1.5f],
            [1.1f,  1.9f],
            [1.1f,  1.3f],
            [0.9f,  1.7f],
            [1.1f,  1.7f],
            [1.0f,  1.4f],
            [1.1f,  2.1f],
            [1.0f,  1.0f],
            [1.0f,  2.0f],
            [1.0f,  1.3f],
            [1.0f,  1.7f],
            [1.0f,  1.9f],
            [1.0f,  2.1f],
            [1.0f,  1.5f],
            [1.0f,  1.5f],
            [1.0f,  1.7f],
            [1.0f,  1.3f],
            [1.0f,  2.0f],
        ];

    // Multi-label classification labels: [x0 >= 0.5 ? 1 : 0, x1 >= 0.5 ? 1 : 0].
    public static float[][] LabelsTrainMultiLabelBinary =>
        [
            [0f, 0f],
            [0f, 1f],
            [1f, 0f],
            [1f, 1f],
            [0f, 1f],
            [1f, 0f],
            [0f, 1f],
            [1f, 0f],
            [1f, 1f],
            [0f, 1f],
            [1f, 0f],
            [0f, 1f],
            [1f, 0f],
            [0f, 1f],
            [1f, 0f],
            [0f, 1f],
            [1f, 0f],
            [0f, 1f],
            [1f, 0f],
            [1f, 1f],
        ];
}
