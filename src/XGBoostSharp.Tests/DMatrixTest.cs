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
            FeatureType.Boolean];

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
}
