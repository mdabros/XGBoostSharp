using System;
using System.Collections.Generic;
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

    public static void AssertAreEqual(
        Dictionary<string, float> expected,
        Dictionary<string, float> actual)
    {
        Assert.AreEqual(expected.Count, actual.Count);
        foreach (var key in expected.Keys)
        {
            Assert.IsTrue(actual.ContainsKey(key));
            Assert.AreEqual(expected[key], actual[key], Delta);
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

    public static float[] AggregateColumns(Array actualContributions, Func<float[], float> aggregate)
    {
        if (actualContributions.Rank != 2 && actualContributions.Rank != 3)
        {
            throw new NotSupportedException("Only 2D and 3D arrays are supported.");
        }

        var aggregatedContributions = new float[actualContributions.GetLength(0)];
        for (var i = 0; i < actualContributions.GetLength(0); i++)
        {
            var currentContribution = new List<float>();
            for (var j = 0; j < actualContributions.GetLength(1); j++)
            {
                if (actualContributions.Rank == 2)
                {
                    currentContribution.Add((float)actualContributions.GetValue(i, j));
                }
                else
                {
                    for (var k = 0; k < actualContributions.GetLength(2); k++)
                    {
                        currentContribution.Add((float)actualContributions.GetValue(i, j, k));
                    }
                }
            }
            aggregatedContributions[i] = aggregate([.. currentContribution]);
        }
        return aggregatedContributions;
    }

    public static float[] FlattenArray(Array array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        var result = new List<float>();
        FlattenArrayRecursive(array, result);
        return result.ToArray();
    }

    static void FlattenArrayRecursive(Array array, List<float> result)
    {
        foreach (var item in array)
        {
            if (item is Array subArray)
            {
                FlattenArrayRecursive(subArray, result);
            }
            else
            {
                result.Add(Convert.ToSingle(item));
            }
        }
    }
}
