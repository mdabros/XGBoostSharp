using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class SaveLoadAndDumpTests
{
    const string TEST_FILE = "tmpfile.tmp";

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE)) File.Delete(TEST_FILE);
    }

    [TestMethod]
    public void TestClassifierSaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProba(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXgbModel.LoadClassifierFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProba(dataTest);

        TestUtils.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestRegressorSaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXgbModel.LoadRegressorFromFile(TEST_FILE);
        var actual = sutLoaded.Predict(dataTest);

        TestUtils.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestClassifierSaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBClassifier(10, 0.01f, 50);
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProba(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXgbModel.LoadClassifierFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProba(dataTest);

        TestUtils.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestRegressorSaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBRegressor();
        sut.Fit(dataTrain, labelsTrain);
        var actual = sut.Predict(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = BaseXgbModel.LoadRegressorFromFile(TEST_FILE);
        var expected = sutLoaded.Predict(dataTest);

        TestUtils.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestClassifierDump()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        var sut = new XGBClassifier();
        sut.Fit(dataTrain, labelsTrain);

        var preds1 = sut.PredictProba(dataTest);
        var actual = sut.DumpModelEx();

        Console.WriteLine("Model Dumped: " + actual);
    }
}
