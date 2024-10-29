using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBClassifierTest
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
    public void XGBClassifierTest_Predict()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.Predict(dataTest);
        var expected = TestUtils.ExpectedClassifierPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_PredictProbability()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictProbability(dataTest);
        var expected = TestUtils.ExpectedClassifierProbabilityPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_SaveAndLoad()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProbability(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        var sutLoaded = XGBClassifier.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProbability(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(ModelFormat.Json)]
    [DataRow(ModelFormat.Ubj)]
    public void XGBClassifierTest_SaveAndLoadRaw(string format)
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProbability(dataTest);
        var savedData = sut.SaveModelToByteArray(format);

        var sutLoaded = XGBClassifier.LoadFromByteArray(savedData);
        var actual = sutLoaded.PredictProbability(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_SaveAndLoadWithParameters()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut(maxDepth: 10, learningRate: 0.01f, nEstimators: 50);
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.PredictProbability(dataTest);
        sut.SaveModelToFile(TEST_FILE);

        using var sutLoaded = XGBClassifier.LoadFromFile(TEST_FILE);
        var actual = sutLoaded.PredictProbability(dataTest);

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBClassifierTest_DumpModelEx()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;

        using var sut = CreateSut(maxDepth: 1, nEstimators: 3);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.DumpModelEx();
        var expected = TestUtils.ExpectedClassifierModelDump;

        TestUtils.AssertAreEqual(expected, actual);
    }

    // Specify all hyperparameters to make tests independant of changing
    // defaults.
    static XGBClassifier CreateSut(int nEstimators = 100,
        int maxDepth = 3, float learningRate = 0.1f)
    {
        return new XGBClassifier(
            nEstimators,
            maxDepth,
            maxLeaves: 0,
            maxBin: 256,
            growPolicy: GrowPolicy.DepthWise,
            learningRate,
            verbosity: 0,
            objective: Objective.Binary.Logistic,
            booster: BoosterType.Gbtree,
            treeMethod: TreeMethod.Auto,
            nThread: -1,
            gamma: 0,
            minChildWeight: 1,
            maxDeltaStep: 0,
            subsample: 1,
            samplingMethod: SamplingMethod.Uniform,
            colSampleByTree: 1,
            colSampleByLevel: 1,
            colSampleByNode: 1,
            regAlpha: 0,
            regLambda: 1,
            scalePosWeight: 1,
            baseScore: 0.5F,
            seed: 0,
            missing: float.NaN,
            numParallelTree: 0,
            importanceType: ImportanceType.Gain,
            device: Device.Cpu,
            validateParameters: false,
            numClass: 1);
    }
}
