using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class XGBClassifierTests
{
    const string TEST_FILE = "tmpfile.tmp";

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBClassifierTests_Predict()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBClassifier();
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

        using var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictProbability(dataTest);
        var expected = TestUtils.ExpectedClassifierProbabilityPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTests_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProbability(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXGBModel.LoadClassifierFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProbability(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTests_SaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBClassifier(10, 0.01f, 50);
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProbability(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        using var sutLoaded = BaseXGBModel.LoadClassifierFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProbability(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTests_DumpModelEx()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;

        using var sut = new XGBClassifier(maxDepth: 1, nEstimators: 3);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.DumpModelEx();
        var expected = TestUtils.ExpectedClassifierModelDump;

        TestUtils.AssertAreEqual(expected, actual);
    }
}
