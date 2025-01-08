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

    public static float[] AggregateColumns(Array actualContributions,
        Func<float[], float> aggregate)
    {
        // Dimensions of an Array are named Rank in C#.
        if (actualContributions.Rank == 2)
        {
            var aggregatedContributions = new float[actualContributions.GetLength(0)];
            // We want to check that the sum of the contributions(contributions
            // + last column, which is bias) is equal to the prediction.
            for (var i = 0; i < actualContributions.GetLength(0); i++)
            {
                var currentContribution = new float[actualContributions.GetLength(1)];
                for (var j = 0; j < actualContributions.GetLength(1); j++)
                {
                    currentContribution[j] = (float)actualContributions.GetValue(i, j);
                }
                var aggregatedContribution = aggregate(currentContribution);
                aggregatedContributions[i] = aggregatedContribution;
            }
            return aggregatedContributions;
        }

        if (actualContributions.Rank == 3)
        {
            var aggregatedContributions = new float[actualContributions.GetLength(0)];
            for (var i = 0; i < actualContributions.GetLength(0); i++)
            {
                var currentContribution = new float[actualContributions.GetLength(1) * actualContributions.GetLength(2)];
                for (var j = 0; j < actualContributions.GetLength(1); j++)
                {
                    for (var k = 0; k < actualContributions.GetLength(2); k++)
                    {
                        currentContribution[j * actualContributions.GetLength(2) + k] = (float)actualContributions.GetValue(i, j, k);
                    }
                }
                var aggregatedContribution = aggregate(currentContribution);
                aggregatedContributions[i] = aggregatedContribution;
            }
            return aggregatedContributions;
        }
        throw new NotSupportedException("Only 2D and 3D arrays are supported.");
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
