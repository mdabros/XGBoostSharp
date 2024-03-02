using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class XGBClassifierTests
{
    [TestMethod]
    public void Predict()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.DataTrainLabels;
        var dataTest = TestUtils.DataTest;

        var xgbc = new XGBClassifier();
        xgbc.Fit(dataTrain, labelsTrain);
        var preds = xgbc.Predict(dataTest);
        Assert.IsTrue(TestUtils.ClassifierPredsCorrect(preds));
    }

    [TestMethod]
    public void PredictProba()
    {
        var dataTrain = TestUtils.DataTrain;
        var labelsTrain = TestUtils.DataTrainLabels;
        var dataTest = TestUtils.DataTest;

        var xgbc = new XGBClassifier();
        xgbc.Fit(dataTrain, labelsTrain);
        var preds = xgbc.PredictProba(dataTest);
        Assert.IsTrue(TestUtils.ClassifierPredsProbaCorrect(preds));
    }
}
