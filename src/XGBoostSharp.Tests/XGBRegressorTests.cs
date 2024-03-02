using Microsoft.VisualStudio.TestTools.UnitTesting;
using XGBoostSharp;

namespace XGBoostSharpTests;

[TestClass]
public class XGBRegressorTests
{
    [TestMethod]
    public void Predict()
    {
        var dataTrain = TestUtils.GetRegressorDataTrain();
        var labelsTrain = TestUtils.GetRegressorLabelsTrain();
        var dataTest = TestUtils.GetRegressorDataTest();

        var xgbr = new XGBRegressor();
        xgbr.Fit(dataTrain, labelsTrain);
        var preds = xgbr.Predict(dataTest);
        Assert.IsTrue(TestUtils.RegressorPredsCorrect(preds));
    }
}
