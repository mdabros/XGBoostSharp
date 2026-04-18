using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBClassifierMultiOutputTest
{
    const string TEST_FILE = "tmpfile_classifier_multioutput.json";
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
    public void XGBClassifierTest_PredictMultiOutput_ShapeIsCorrect()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictMultiOutput(dataTrain);
        var expected = TestUtils.ExpectedMultiOutputClassifierPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiOutput_ValuesAreBinary()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiOutput(dataTrain);

        foreach (var row in predictions)
        {
            foreach (var value in row)
            {
                Assert.IsTrue(value == 0f || value == 1f,
                    $"Expected 0 or 1, got {value}");
            }
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictProbabilityMultiOutput_ShapeIsCorrect()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var probabilities = sut.PredictProbabilityMultiOutput(dataTrain);

        Assert.HasCount(dataTrain.Length, probabilities);
        foreach (var row in probabilities)
        {
            Assert.HasCount(NOutputs, row);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictProbabilityMultiOutput_ProbabilitiesInRange()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var probabilities = sut.PredictProbabilityMultiOutput(dataTrain);

        foreach (var row in probabilities)
        {
            foreach (var p in row)
            {
                Assert.IsTrue(p >= 0f && p <= 1f,
                    $"Probability {p} is out of [0, 1] range.");
            }
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiOutput_UsingDMatrixDirectly()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        using var dMatrixTrain = new DMatrix(dataTrain, labelsTrain);
        sut.Fit(dMatrixTrain);

        using var dMatrixTest = new DMatrix(dataTrain);
        var actual = sut.PredictMultiOutput(dMatrixTest);
        var expected = TestUtils.ExpectedMultiOutputClassifierPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiOutput_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictMultiOutput(dataTrain);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = XGBClassifier.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.PredictMultiOutput(dataTrain);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(ModelFormat.Json)]
    [DataRow(ModelFormat.Ubj)]
    public void XGBClassifierTest_PredictMultiOutput_SaveAndLoadRaw(string format)
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictProbabilityMultiOutput(dataTrain);
        var savedData = sut.SaveModelToByteArray(format);

        var sutLoaded = XGBClassifier.LoadFromByteArray(savedData);
        var actual = sutLoaded.PredictProbabilityMultiOutput(dataTrain);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiOutput_MultiOutputTreeStrategy()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = new XGBClassifier(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic,
            treeMethod: TreeMethod.Hist,
            multiStrategy: MultiStrategy.MultiOutputTree);
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiOutput(dataTrain);

        TestUtils.AssertShape(predictions, dataTrain.Length, NOutputs);
    }

    static XGBClassifier CreateSut() =>
        new(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic);
}
