using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBClassifierMultiLabelTest
{
    const string TEST_FILE = "tmpfile_classifier_multilabel.json";
    const int NLabels = 2;

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiLabel_ShapeIsCorrect()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiLabel(dataTrain);

        Assert.AreEqual(dataTrain.Length, predictions.Length);
        foreach (var row in predictions)
        {
            Assert.AreEqual(NLabels, row.Length);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiLabel_ValuesAreBinary()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiLabel(dataTrain);

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
    public void XGBClassifierTest_PredictProbabilityMultiLabel_ShapeIsCorrect()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var probabilities = sut.PredictProbabilityMultiLabel(dataTrain);

        Assert.AreEqual(dataTrain.Length, probabilities.Length);
        foreach (var row in probabilities)
        {
            Assert.AreEqual(NLabels, row.Length);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictProbabilityMultiLabel_ProbabilitiesInRange()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var probabilities = sut.PredictProbabilityMultiLabel(dataTrain);

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
    public void XGBClassifierTest_PredictMultiLabel_UsingDMatrixDirectly()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        using var dMatrixTrain = new DMatrix(dataTrain, labelsTrain);
        sut.Fit(dMatrixTrain);

        using var dMatrixTest = new DMatrix(dataTrain);
        var predictions = sut.PredictMultiLabel(dMatrixTest);

        Assert.AreEqual(dataTrain.Length, predictions.Length);
        foreach (var row in predictions)
        {
            Assert.AreEqual(NLabels, row.Length);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiLabel_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictMultiLabel(dataTrain);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = XGBClassifier.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.PredictMultiLabel(dataTrain);

        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            TestUtils.AssertAreEqual(expected[i], actual[i]);
        }
    }

    [TestMethod]
    [DataRow(ModelFormat.Json)]
    [DataRow(ModelFormat.Ubj)]
    public void XGBClassifierTest_PredictMultiLabel_SaveAndLoadRaw(string format)
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);
        var expected = sut.PredictProbabilityMultiLabel(dataTrain);
        var savedData = sut.SaveModelToByteArray(format);

        var sutLoaded = XGBClassifier.LoadFromByteArray(savedData);
        var actual = sutLoaded.PredictProbabilityMultiLabel(dataTrain);

        Assert.AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            TestUtils.AssertAreEqual(expected[i], actual[i]);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiLabel_MultiOutputTreeStrategy()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiLabelBinary;

        using var sut = new XGBClassifier(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic,
            treeMethod: TreeMethod.Hist,
            multiStrategy: MultiStrategy.MultiOutputTree);
        sut.Fit(dataTrain, labelsTrain);

        var predictions = sut.PredictMultiLabel(dataTrain);

        Assert.AreEqual(dataTrain.Length, predictions.Length);
        foreach (var row in predictions)
        {
            Assert.AreEqual(NLabels, row.Length);
        }
    }

    static XGBClassifier CreateSut() =>
        new(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic);
}
