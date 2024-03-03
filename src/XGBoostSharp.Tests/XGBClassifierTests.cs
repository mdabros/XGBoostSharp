using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class XGBClassifierTests
{
    [TestMethod]
    public void XGBClassifierTests_Predict()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.Predict(dataTest);
        var expected = TestUtils.ExpectedClassifierPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTests_PredictProbability()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictProbability(dataTest);
        var expected = TestUtils.ExpectedClassifierProbabilityPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }
}
