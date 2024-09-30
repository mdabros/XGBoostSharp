﻿using System.Collections.Generic;
using XGBoostSharp.lib;
using static XGBoostSharp.Parameters;

namespace XGBoostSharp;

public class XGBRegressor : XGBModelBase
{
    public XGBRegressor(
        int nEstimators = 100,
        int maxDepth = 6,
        int maxLeaves = 0,
        int maxBin = 256,
        string growPolicy = GrowPolicy.DepthWise,
        float learningRate = 0.3f,
        int verbosity = Verbosity.Silent,
        string objective = Objective.Reg.SquaredError,
        string booster = BoosterType.Gbtree,
        string treeMethod = TreeMethod.Auto,
        int nThread = -1,
        float gamma = 0,
        int minChildWeight = 1,
        int maxDeltaStep = 0,
        float subsample = 1,
        string samplingMethod = SamplingMethod.Uniform,
        float colSampleByTree = 1,
        float colSampleByLevel = 1,
        float colSampleByNode = 1,
        float regAlpha = 0,
        float regLambda = 1,
        float scalePosWeight = 1,
        float baseScore = 0.5f,
        int seed = 0,
        float missing = float.NaN,
        int numParallelTree = 1,
        string importanceType = ImportanceType.Gain,
        string device = Device.Cpu,
        bool validateParameters = false)
    {

        m_parameters[ParameterNames.n_estimators] = nEstimators;
        m_parameters[ParameterNames.max_depth] = maxDepth;
        m_parameters[ParameterNames.max_leaves] = maxLeaves;
        m_parameters[ParameterNames.max_bin] = maxBin;
        m_parameters[ParameterNames.grow_policy] = growPolicy;
        m_parameters[ParameterNames.learning_rate] = learningRate;
        m_parameters[ParameterNames.verbosity] = verbosity;
        m_parameters[ParameterNames.objective] = objective;
        m_parameters[ParameterNames.booster] = booster;
        m_parameters[ParameterNames.tree_method] = treeMethod;

        m_parameters[ParameterNames.nthread] = nThread;
        m_parameters[ParameterNames.gamma] = gamma;
        m_parameters[ParameterNames.min_child_weight] = minChildWeight;
        m_parameters[ParameterNames.max_delta_step] = maxDeltaStep;
        m_parameters[ParameterNames.subsample] = subsample;
        m_parameters[ParameterNames.sampling_method] = samplingMethod;
        m_parameters[ParameterNames.colsample_bytree] = colSampleByTree;
        m_parameters[ParameterNames.colsample_bylevel] = colSampleByLevel;
        m_parameters[ParameterNames.colsample_byNode] = colSampleByNode;
        m_parameters[ParameterNames.reg_alpha] = regAlpha;
        m_parameters[ParameterNames.reg_lambda] = regLambda;
        m_parameters[ParameterNames.scale_pos_weight] = scalePosWeight;
        m_parameters[ParameterNames.base_score] = baseScore;
        m_parameters[ParameterNames.seed] = seed;
        m_parameters[ParameterNames.missing] = missing;
        m_parameters[ParameterNames.numParallelTree] = numParallelTree;
        m_parameters[ParameterNames.importance_type] = importanceType;
        m_parameters[ParameterNames.device] = device;
        m_parameters[ParameterNames.validate_parameters] = validateParameters;

        // For DART only.
        m_parameters[ParameterNames.sample_type] = "uniform";
        m_parameters[ParameterNames.normalize_type] = "tree";
        m_parameters[ParameterNames.rate_drop] = 0f;
        m_parameters[ParameterNames.one_drop] = 0;
        m_parameters[ParameterNames.skip_drop] = 0f;
    }

    public static XGBRegressor LoadFromFile(string fileName) =>
        new() { m_booster = new Booster(fileName) };

    public XGBRegressor(IDictionary<string, object> p_parameters) =>
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
        using var test = new DMatrix(data);
        return m_booster.Predict(test);
    }
}
