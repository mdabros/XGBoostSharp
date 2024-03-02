using System;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace XGBoostSharpTests;

public static partial class TestUtils
{
    public static float[][] DataTrain =>
        TrainingSet.Select(x => x[1..]).ToArray();

    public static float[] DataTrainLabels =>
        TrainingSet.Select(x => x[0]).ToArray();

    public static float[][] DataTest =>
        TestSet;

    public static bool ClassifierPredsCorrect(float[] preds)
    {
        using var parser = new TextFieldParser(@"../../../../test_files/predsclas.csv");
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        var row = 0;
        var predInd = 0;

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();

            for (var col = 0; col < fields.Length; col++)
            {
                var absDiff = Math.Abs(float.Parse(fields[col]) - preds[predInd]);
                if (absDiff > 0.01F)
                    return false;
                predInd += 1;
            }
            row += 1;
        }
        return true; // we haven't returned from a wrong prediction so everything is right
    }

    public static bool ClassifierPredsProbaCorrect(float[][] preds)
    {
        using var parser = new TextFieldParser(@"../../../../test_files/predsclasproba.csv");
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        var row = 0;

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();

            for (var col = 0; col < fields.Length; col++)
            {
                var absDiff = Math.Abs(float.Parse(fields[col]) - preds[row][col]);
                if (absDiff > 0.0001F)
                    return false;
            }
            row += 1;
        }
        return true; // we haven't returned from a wrong prediction so everything is right
    }

    public static bool RegressorPredsCorrect(float[] preds)
    {
        using var parser = new TextFieldParser(@"../../../../test_files//predsreg.csv");
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        var row = 0;
        var predInd = 0;

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();

            for (var col = 0; col < fields.Length; col++)
            {
                var absDiff = Math.Abs(float.Parse(fields[col]) - preds[predInd]);
                if (absDiff > 0.01F)
                    return false;
                predInd += 1;
            }
            row += 1;
        }
        return true; // we haven't returned from a wrong prediction so everything is right
    }

    public static bool AreEqual(float[][] arr1, float[][] arr2)
    {
        if (arr1.Length != arr2.Length) return false;
        if (arr1.Length == 0) return true;
        if (arr1[0].Length != arr2[0].Length) return false;
        for (var i = 0; i < arr1.Length; i++) for (var j = 0; j < arr1[i].Length; j++) if (arr1[i][j] != arr2[i][j]) return false;
        return true;
    }

    public static bool AreEqual(float[] arr, float[] other)
    {
        return arr.SequenceEqual(other);
    }
}
