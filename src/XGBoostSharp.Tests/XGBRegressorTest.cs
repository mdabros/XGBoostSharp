using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.lib;
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
    [DataRow(ModelFormat.Json)]
    [DataRow(ModelFormat.Ubj)]
    public void XGBRegressorTest_SaveAndLoadRaw(string format)
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var expected = sut.Predict(dataTest);
        var savedData = sut.SaveModelToByteArray(format);

        var sutLoaded = XGBRegressor.LoadFromByteArray(savedData);
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
    public void XGBRegressorTest_UsingDMatrixDirectly()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        using var dMatrixTrain = new DMatrix(dataTrain, labelsTrain);
        sut.Fit(dMatrixTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var actual = sut.Predict(dMatrixTest);
        var expected = TestUtils.ExpectedRegressionPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictionContributions()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        using var dMatrixTrain = new DMatrix(dataTrain, labelsTrain);
        sut.Fit(dMatrixTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var actualContributions = sut.Predict(dMatrixTest, predContribs: true);
        var expected = sut.Predict(dMatrixTest);

        // We want to check that the sum of the contributions(contributions +
        // last column, which is bias) is equal to the prediction.
        var actual = TestUtils.AggregateColumns(actualContributions,
            floats => floats.Sum());

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictionContributionsApprox()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var actualContributions = sut.Predict(dMatrixTest, predContribs: true, approxContribs: true);
        var expected = sut.Predict(dMatrixTest);

        Assert.AreEqual(2, actualContributions.Rank);

        // We want to check that the sum of the contributions(contributions +
        // last column, which is bias) is equal to the prediction.
        var actual = TestUtils.AggregateColumns(actualContributions,
            floats => floats.Sum());

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictionInteractions()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var actualInteractions = sut.Predict(dMatrixTest, predInteractions: true);
        var expected = sut.Predict(dMatrixTest);

        Assert.AreEqual(3, actualInteractions.Rank);

        var actual = TestUtils.AggregateColumns(actualInteractions,
            floats => floats.Sum());

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictionLeaf()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var actualInteractions = sut.Predict(dMatrixTest, predLeaf: true);
        var actualLeafs = sut.Predict(dMatrixTest, predLeaf: true);

        Assert.AreEqual(2, actualLeafs.Rank);

        var expected = TestUtils.ExpectedRegressorPredictionLeafs;

        var actual = TestUtils.AggregateColumns(actualLeafs,
            floats => floats.Sum());

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictWithStrictShape()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var predictionResult = sut.Predict(dMatrixTest, strictShape: true);

        Assert.AreEqual(2, predictionResult.Rank);

        var expected = TestUtils.ExpectedRegressionPredictions;

        var actual = TestUtils.FlattenArray(predictionResult);
        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictWithOutputMargin()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;
        var dataTest = TestUtils.DataTest;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTest);
        var predictionResult = sut.Predict(dMatrixTest, strictShape: true, outputMargin: true);

        var expected = TestUtils.ExpectedRegressionPredictions;

        Assert.AreEqual(2, predictionResult.Rank);

        var actual = TestUtils.FlattenArray(predictionResult);
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

    [DataTestMethod]
    [DataRow(ImportanceType.Weight, new[] { 49f, 499f, 86f })]
    [DataRow(ImportanceType.Gain, new[] { 2.31486464f, 0.454449239f, 0.500057459f })]
    [DataRow(ImportanceType.Cover, new[] { 409.326538f, 439.118225f, 323.4535f })]
    [DataRow(ImportanceType.TotalGain, new[] { 113.428368f, 226.770172f, 43.00494f })]
    [DataRow(ImportanceType.TotalCover, new[] { 20057f, 219120f, 27817f })]
    public void XGBRegressor_GetFeatureScore(string importanceType, float[] featureScores)
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.LabelsTrain;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.GetFeatureScore(importanceType);

        var featureNames = new[] { "f0", "f1", "f2" };
        var expected = featureNames.Zip(featureScores, (name, score) => new { name, score })
                                   .ToDictionary(x => x.name, x => x.score);

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
