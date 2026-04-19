using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;

namespace XGBoostSharp.Test;

[TestClass]
public class DMatrixTest
{
    readonly float[][] m_dataTrain = [[1, 2, 3, 4]];
    readonly float[] m_labelsTrain = [1];

    [TestMethod]
    public void DMatrix_SetFeatureNames()
    {
        string[] featureNames = ["f1", "f2", "f3", "f4"];

        var sut = new DMatrix(m_dataTrain, m_labelsTrain);
        sut.SetFeatureNames(featureNames);

        var featureNamesFromDMatrix = sut.GetFeatureNames();
        TestUtils.AssertAreEqual(featureNames, featureNamesFromDMatrix);
    }

    [TestMethod]
    public void DMatrix_SetFeatureTypes()
    {
        string[] featureTypes = [FeatureType.Categorical,
            FeatureType.Float,
            FeatureType.Integer,
            FeatureType.Boolean,];

        var sut = new DMatrix(m_dataTrain, m_labelsTrain);
        sut.SetFeatureTypes(featureTypes);

        var featureTypesFromDMatrix = sut.GetFeatureTypes();
        TestUtils.AssertAreEqual(featureTypes, featureTypesFromDMatrix);
    }

    [TestMethod]
    public void DMatrix_GetAndSetLabel()
    {
        var sut = new DMatrix(m_dataTrain, m_labelsTrain);
        var expected = new float[] { 0, 1, 0, 1 };
        sut.Label = expected;

        var actual = sut.Label;
        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void DMatrix_FromCsvFile_LoadsMatrixWithLabel()
    {
        using var sut = DMatrix.FromCsvFile("TestData/test.csv");

        // CSV format does not embed labels, so assign one explicitly
        sut.Label = [1f];
        var labels = sut.Label;

        Assert.HasCount(1, labels);
        Assert.AreEqual(1f, labels[0], TestUtils.Delta);
    }

    [TestMethod]
    public void DMatrix_FromCsvFile_WithLabelColumn_LoadsMatrixWithLabel()
    {
        using var sut = DMatrix.FromCsvFile("TestData/test.csv", labelColumn: 0);

        var labels = sut.Label;
        Assert.HasCount(1, labels);
        Assert.AreEqual(1f, labels[0], TestUtils.Delta);
    }

    [TestMethod]
    public void DMatrix_FromLibSvmFile_LoadsMatrixWithLabel()
    {
        using var sut = DMatrix.FromLibSvmFile("TestData/test.libsvm");

        var labels = sut.Label;
        Assert.HasCount(1, labels);
        Assert.AreEqual(1f, labels[0], TestUtils.Delta);
    }
}
