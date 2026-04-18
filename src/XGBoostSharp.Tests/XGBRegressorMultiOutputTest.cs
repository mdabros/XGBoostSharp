using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp.Lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp.Test;

[TestClass]
public class XGBRegressorMultiOutputTest
{
    const string TEST_FILE = "tmpfile_regressor_multioutput.json";

    [TestInitialize, TestCleanup]
    public void Reset()
    {
        if (File.Exists(TEST_FILE))
        {
            File.Delete(TEST_FILE);
        }
    }

    [TestMethod]
    public void XGBRegressorTest_PredictMultiOutput()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = CreateSut();
        sut.Fit(dataTrain, labelsTrain);

        using var dMatrixTest = new DMatrix(dataTrain);
        var actual = sut.PredictMultiOutput(dMatrixTest);
        var expected = TestUtils.ExpectedMultiOutputRegressionPredictions;

        TestUtils.AssertAreEqual(expected, actual);
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
        var actual = sut.PredictMultiOutput(dMatrixTest);
        var expected = TestUtils.ExpectedMultiOutputRegressionPredictions;

        TestUtils.AssertAreEqual(expected, actual);
    }

    [TestMethod]
    public void XGBRegressorTest_PredictMultiOutput_MultiOutputTreeStrategy()
    {
        var dataTrain = TestUtils.DataTrainMultiOutput;
        var labelsTrain = TestUtils.LabelsTrainMultiOutputRegression;

        using var sut = new XGBRegressor(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Reg.SquaredError,
            treeMethod: TreeMethod.Hist,
            multiStrategy: MultiStrategy.MultiOutputTree);
        sut.Fit(dataTrain, labelsTrain);

        var actual = sut.PredictMultiOutput(dataTrain);

        TestUtils.AssertAreEqual(TestUtils.ExpectedMultiOutputTreeRegressionPredictions, actual);
    }

    static XGBRegressor CreateSut() =>
        new(nEstimators: 50, maxDepth: 3, learningRate: 0.3f,
            objective: Objective.Reg.SquaredError);
}
