using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBRegressorMultiOutputTest
{
    const string TEST_FILE = "tmpfile_regressor_multioutput.json";
    const int NOutputs = 2;

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBRegressorTest_PredictMultiOutput_ShapeIsCorrect()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiOutput(dataTrain);

        Assert.AreEqual(dataTrain.Length, predictions.Length);
        foreach (var row in predictions)
        {
            Assert.AreEqual(NOutputs, row.Length);
        }
    }

    [TestMethod]
    public void XGBRegressorTest_PredictMultiOutput_UsingDMatrixDirectly()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = CreateSut();
        using var dMatrixTrain = new DMatrix(dataTrain, labelsTrain);
        sut.Fit(dMatrixTrain);

        using var dMatrixTest = new DMatrix(dataTrain);
        var predictions = sut.PredictMultiOutput(dMatrixTest);

        Assert.AreEqual(dataTrain.Length, predictions.Length);
        foreach (var row in predictions)
        {
            Assert.AreEqual(NOutputs, row.Length);
        }
    }

    [TestMethod]
    public void XGBRegressorTest_PredictMultiOutput_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictMultiOutput(dataTrain);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = XGBRegressor.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.PredictMultiOutput(dataTrain);

        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            TestUtils.AssertAreEqual(expected[i], actual[i]);
        }
    }

    [TestMethod]
    [DataRow(ModelFormat.Json)]
    [DataRow(ModelFormat.Ubj)]
    public void XGBRegressorTest_PredictMultiOutput_SaveAndLoadRaw(string format)
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictMultiOutput(dataTrain);
        var savedData = sut.SaveModelToByteArray(format);

        var sutLoaded = XGBRegressor.LoadFromByteArray(savedData);
        var actual = sutLoaded.PredictMultiOutput(dataTrain);

        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            TestUtils.AssertAreEqual(expected[i], actual[i]);
        }
    }

    static XGBRegressor CreateSut() =>
        new(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Reg.SquaredError);
}
