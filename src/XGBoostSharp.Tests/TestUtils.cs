using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XGBoostSharp.Test;

public static partial class TestUtils
{
    public const float Delta = 0.0001F;

    public static float[][] DataTrain =>
        TrainingSet.Select(x => x[1..]).ToArray();

    public static float[] LabelsTrain =>
        TrainingSet.Select(x => x[0]).ToArray();

    public static float[][] DataTest =>
        TestSet;

    public static void AssertAreEqual(float[] expected, float[] actual)
    {
        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], Delta);
        }
    }

    public static void AssertAreEqual(float[][] expecteds, float[][] actuals)
    {
        Assert.AreEqual(expecteds.Length, actuals.Length);
        for (var row = 0; row < expecteds.Length; row++)
        {
            var expected = expecteds[row];
            var actual = actuals[row];
            Assert.AreEqual(expected.Length, actual.Length);
            for (var col = 0; col < expected.Length; col++)
            {
                Assert.AreEqual(expected[col], actual[col], Delta);
            }
        }
    }

    public static void AssertAreEqual(string[] expected, string[] actual)
    {
        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }

    public static void TracePredictions(float[] predictions)
    {
        for (var i = 0; i < predictions.Length; i++)
        {
            var prediction = predictions[i];
            Trace.WriteLine($"{prediction:F12}f,");
        }
    }

    public static void TracePredictions(float[][] predictions)
    {
        for (var r = 0; r < predictions.Length; r++)
        {
            var row = predictions[r];
            var sb = new StringBuilder();
            for (var c = 0; c < row.Length; c++)
            {
                var prediction = row[c];
                sb.Append($"{prediction:F12}f, ");
            }
            Trace.WriteLine($"[{sb}],");
        }
    }
}
