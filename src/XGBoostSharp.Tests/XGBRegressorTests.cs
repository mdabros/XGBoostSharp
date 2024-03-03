using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class XGBRegressorTests
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
    public void XGBRegressorTests_Predict()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.Predict(dataTest);
        var expected = TestUtils.ExpectedRegressionPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTests_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXGBModel.LoadRegressorFromFile(TEST_FILE);
        var actual = sutLoaded.Predict(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTests_SaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var actual = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        using var sutLoaded = BaseXGBModel.LoadRegressorFromFile(TEST_FILE);
        var expected = sutLoaded.Predict(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTests_DumpModelEx()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;

        using var sut = new XGBRegressor(maxDepth: 1, nEstimators: 3);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.DumpModelEx();
        var expected = TestUtils.ExpectedRegressorModelDump;

        TestUtils.AssertAreEqual(expected, actual);
    }
}
