using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBRegressorTest
{
    const string TEST_FILE = "tmpfile.json";

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBRegressorTest_Predict()
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
    public void XGBRegressorTest_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = XGBRegressor.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.Predict(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_SaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var actual = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        using var sutLoaded = XGBRegressor.LoadFromFile(TEST_FILE);
        var expected = sutLoaded.Predict(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_DumpModelEx()
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
