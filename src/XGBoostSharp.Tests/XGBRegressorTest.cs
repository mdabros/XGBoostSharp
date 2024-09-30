using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static XGBoostSharp.Parameters;

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

        using var sut = CreateSut();
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

        using var sut = CreateSut();
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

        using var sut = CreateSut();
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

        using var sut = CreateSut(maxDepth: 1, nEstimators: 3);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.DumpModelEx();
        var expected = TestUtils.ExpectedRegressorModelDump;

        TestUtils.AssertAreEqual(expected, actual);
    }

    // Specify all hyperparameters to make tests independant of changing
    // defaults.
    static XGBRegressor CreateSut(int nEstimators = 100,
        int maxDepth = 3, float learningRate = 0.1f)
    {
        return new XGBRegressor(
            nEstimators,
            maxDepth,
            maxLeaves: 0,
            maxBin: 256,
            growPolicy: GrowPolicy.DepthWise,
            learningRate,
            verbosity: 0,
            objective: Objective.Reg.SquaredError,
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
            validateParameters: false);
    }
}
