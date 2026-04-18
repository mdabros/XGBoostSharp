using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBClassifierMultiOutputTest
{
    const string TEST_FILE = "tmpfile_classifier_multioutput.json";

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBClassifierTest_PredictMultiOutput()
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
    public void XGBClassifierTest_PredictProbabilityMultiOutput()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictProbabilityMultiOutput(dataTrain);

        TestUtils.AssertAreEqual(TestUtils.ExpectedMultiOutputClassifierProbabilities, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_PredictProbabilityMultiOutput_MultiOutputTreeStrategy()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputBinary;

        using var sut = new XGBClassifier(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic,
            treeMethod: TreeMethod.Hist,
            multiStrategy: MultiStrategy.MultiOutputTree);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictProbabilityMultiOutput(dataTrain);

        TestUtils.AssertAreEqual(TestUtils.ExpectedMultiOutputTreeClassifierProbabilities, actual);
    }

    static XGBClassifier CreateSut() =>
        new(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Binary.Logistic);
}
