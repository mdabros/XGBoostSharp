using System;
using System.Collections.Generic;
using System.Linq;
using XGBoostSharp.lib;

namespace XGBoostSharp;

public class XGBClassifier : XGBModelBase
{
    /// <summary>
    ///   Implementation of the Scikit-Learn API for XGBoostSharp
    /// </summary>
    /// <param name="maxDepth">
    ///   Maximum tree depth for base learners
    /// </param>
    /// <param name="learningRate">
    ///   Boosting learning rate (xgb's "eta")
    /// </param>
    /// <param name="nEstimators">
    ///   Number of boosted trees to fit
    /// </param>
    /// <param name="silent">
    ///   Whether to print messages while running boosting
    /// </param>
    /// <param name="objective">
    ///   Specify the learning task and the corresponding learning objective or
    ///   a custom objective function to be used(see note below)
    /// </param>
    /// <param name="nThread">
    ///   Number of parallel threads used to run xgboost
    /// </param>
    /// <param name="gamma">
    ///   Minimum loss reduction required to make a further partition on a leaf node of the tree
    /// </param>
    /// <param name="minChildWeight">
    ///   Minimum sum of instance weight(hessian) needed in a child
    /// </param>
    /// <param name="maxDeltaStep">
    ///   Maximum delta step we allow each tree's weight estimation to be
    /// </param>
    /// <param name="subsample">
    ///   Subsample ratio of the training instance
    /// </param>
    /// <param name="colSampleByTree">
    ///   Subsample ratio of columns when constructing each tree TODO prevent error for bigger range of vals
    /// </param>
    /// <param name="colSampleByLevel">
    ///   Subsample ratio of columns for each split, in each level TODO prevent error for bigger range of vals
    /// </param>
    /// <param name="regAlpha">
    ///   L1 regularization term on weights
    /// </param>
    /// <param name="regLambda">
    ///   L2 regularization term on weights
    /// </param>
    /// <param name="scalePosWeight">
    ///   Balancing of positive and negative weights
    /// </param>
    /// <param name="baseScore">
    ///   The initial prediction score of all instances, global bias
    /// </param>
    /// <param name="seed">
    ///   Random number seed
    /// </param>
    /// <param name="missing">
    ///   Value in the data which needs to be present as a missing value
    /// </param>
    public XGBClassifier(int maxDepth = 3, float learningRate = 0.1F, int nEstimators = 100,
          bool silent = true, string objective = "binary:logistic",
          int nThread = -1, float gamma = 0, int minChildWeight = 1,
          int maxDeltaStep = 0, float subsample = 1, float colSampleByTree = 1,
          float colSampleByLevel = 1, float regAlpha = 0, float regLambda = 1,
          float scalePosWeight = 1, float baseScore = 0.5F, int seed = 0,
          float missing = float.NaN, int numClass = 1)
    {
        m_parameters[ParameterNames.max_depth] = maxDepth;
        m_parameters[ParameterNames.learning_rate] = learningRate;
        m_parameters[ParameterNames.n_estimators] = nEstimators;
        m_parameters[ParameterNames.silent] = silent;
        m_parameters[ParameterNames.objective] = objective;
        m_parameters[ParameterNames.booster] = "gbtree";
        m_parameters[ParameterNames.tree_method] = "auto";

        m_parameters[ParameterNames.nthread] = nThread;
        m_parameters[ParameterNames.gamma] = gamma;
        m_parameters[ParameterNames.min_child_weight] = minChildWeight;
        m_parameters[ParameterNames.max_delta_step] = maxDeltaStep;
        m_parameters[ParameterNames.subsample] = subsample;
        m_parameters[ParameterNames.colsample_bytree] = colSampleByTree;
        m_parameters[ParameterNames.colsample_bylevel] = colSampleByLevel;
        m_parameters[ParameterNames.reg_alpha] = regAlpha;
        m_parameters[ParameterNames.reg_lambda] = regLambda;
        m_parameters[ParameterNames.scale_pos_weight] = scalePosWeight;

        m_parameters[ParameterNames.sample_type] = "uniform";
        m_parameters[ParameterNames.normalize_type] = "tree";
        m_parameters[ParameterNames.rate_drop] = 0f;
        m_parameters[ParameterNames.one_drop] = 0;
        m_parameters[ParameterNames.skip_drop] = 0f;

        m_parameters[ParameterNames.base_score] = baseScore;
        m_parameters[ParameterNames.seed] = seed;
        m_parameters[ParameterNames.missing] = missing;
        m_parameters[ParameterNames._Booster] = null;
        m_parameters[ParameterNames.num_class] = numClass;
    }

    public static XGBClassifier LoadFromFile(string fileName) =>
        new() { m_booster = new Booster(fileName) };

    public XGBClassifier(IDictionary<string, object> p_parameters) =>
        m_parameters = p_parameters;

    /// <summary>
    ///   Fit the gradient boosting model
    /// </summary>
    /// <param name="data">
    ///   Feature matrix
    /// </param>
    /// <param name="labels">
    ///   Labels
    /// </param>
    public void Fit(float[][] data, float[] labels)
    {
        using var train = new DMatrix(data, labels);
        m_booster = Train(m_parameters, train);
    }

    public void SetParameter(string parameterName, object parameterValue) =>
        m_parameters[parameterName] = parameterValue;

    /// <summary>
    ///   Predict using the gradient boosted model
    /// </summary>
    /// <param name="data">
    ///   Feature matrix to do predicitons on
    /// </param>
    /// <returns>
    ///   Predictions
    /// </returns>
    public float[] Predict(float[][] data)
    {
        using var dMatrix = new DMatrix(data);
        var predictions = m_booster.Predict(dMatrix).Select(v => v > 0.5f ? 1f : 0f).ToArray();
        return predictions;
    }

    public float[] PredictRaw(float[][] data)
    {
        using var dMatrix = new DMatrix(data);
        var predictions = m_booster.Predict(dMatrix);
        return predictions;
    }
    /// <summary>
    ///   Predict using the gradient boosted model
    /// </summary>
    /// <param name="data">
    ///   Feature matrix to do predicitons on
    /// </param>
    /// <returns>
    ///   The probabilities for each classification being the actual
    ///   classification for each row
    /// </returns>
    public float[][] PredictProbability(float[][] data)
    {
        using var dMatrix = new DMatrix(data);
        var predictions = m_booster.Predict(dMatrix);
        var classCount = (int)m_parameters[ParameterNames.num_class];
        var observationCount = predictions.Length / classCount;

        if (classCount < 2)
        {
            return predictions.Select(v => new[] { 1 - v, v }).ToArray();
        }

        var results = new float[observationCount][];
        for (var i = 0; i < observationCount; i++)
        {
            var p = new float[classCount];
            Array.Copy(predictions, i * classCount, p, 0, classCount);
            results[i] = p;
        }

        return results;
    }
}
