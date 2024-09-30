namespace XGBoostSharp;

public static class Parameters
{
    /// <summary>
    /// Controls a way new nodes are added to the tree. Currently supported only
    /// if tree_method is set to hist or approx.
    /// </summary>
    public static class GrowPolicy
    {
        /// <summary>
        /// split at nodes closest to the root.
        /// </summary>
        public const string DepthWise = "depthwise";
        /// <summary>
        /// split at nodes with highest loss change
        /// </summary>
        public const string Lossguide = "lossguide";
    }

    /// <summary>
    /// BoosterType parameters.
    /// </summary>
    public static class BoosterType
    {
        /// <summary>
        /// Tree booster.
        /// </summary>
        public const string Gbtree = "gbtree";
        /// <summary>
        /// Linear booster.
        /// </summary>
        public const string Gbliner = "gblinear";
        /// <summary>
        /// Dart booster.
        /// </summary>
        public const string Dart = "dart";
    }

    /// <summary>
    /// Tree construction algorithm used in XGBoost.
    /// </summary>
    public static class TreeMethod
    {
        /// <summary>
        /// Use heuristic to choose the fastest method.
        /// </summary>
        public const string Auto = "auto";
        /// <summary>
        /// Exact greedy algorithm.
        /// </summary>
        public const string Exact = "exact";
        /// <summary>
        /// Approximate greedy algorithm using quantile sketch and gradient
        /// histogram.
        /// </summary>
        public const string Approx = "approx";
        /// <summary>
        /// Fast histogram optimized approximate greedy algorithm.
        /// </summary>
        public const string Hist = "hist";
        /// <summary>
        /// GPU implementation of hist algorithm.
        /// </summary>
        public const string GpuHist = "gpu_hist";
    }

    /// <summary>
    /// Sampling method.
    /// </summary>
    public static class SamplingMethod
    {
        /// <summary>
        /// Simple uniform sampling.
        /// </summary>
        public const string Uniform = "uniform";
        /// <summary>
        /// Gradient-based sampling.
        /// </summary>
        public const string GradientBased = "gradient_based";
    }

    /// <summary>
    /// Feature importance type.
    /// </summary>
    public static class ImportanceType
    {
        /// <summary>
        /// The average gain of the feature when it is used in trees.
        /// </summary>
        public const string Gain = "gain";
        /// <summary>
        /// The number of times a feature is used to split the data across all
        /// trees.
        /// </summary>
        public const string Weight = "weight";
        /// <summary>
        /// The average coverage of the feature when it is used in trees.
        /// </summary>
        public const string Cover = "cover";
        /// <summary>
        /// The total gain of the feature when it is used in trees.
        /// </summary>
        public const string TotalGain = "total_gain";
        /// <summary>
        /// The total coverage of the feature when it is used in trees.
        /// </summary>
        public const string TotalCover = "total_cover";
    }

    /// <summary>
    /// Device to use for training.
    /// </summary>
    public static class Device
    {
        /// <summary>
        /// Use CPU for training.
        /// </summary>
        public const string Cpu = "cpu";
        /// <summary>
        /// Use GPU for training.
        /// </summary>
        public const string Gpu = "gpu";
    }

    public static class Verbosity
    {
        public const int Silent = 0;
        public const int Warning = 1;
        public const int Info = 2;
        public const int Debug = 3;
    }

    /// <summary>
    /// Objective parameters.
    /// </summary>
    public static class Objective
    {
        /// <summary>
        /// Regression objectives.
        /// </summary>
        public static class Reg
        {
            /// <summary>
            /// Regression with squared loss.
            /// </summary>
            public const string SquaredError = "reg:squarederror";
            /// <summary>
            /// Regression with squared log loss.
            /// </summary>
            public const string SquaredLogError = "reg:squaredlogerror";
            /// <summary>
            /// Logistic regression, output probability.
            /// </summary>
            public const string Logistic = "reg:logistic";
            /// <summary>
            /// Regression with Pseudo Huber loss.
            /// </summary>
            public const string PseudoHuberError = "reg:pseudohubererror";
            /// <summary>
            /// Regression with L1 error.
            /// </summary>
            public const string AbsoluteError = "reg:absoluteerror";
            /// <summary>
            /// Quantile loss, also known as pinball loss.
            /// </summary>
            public const string QuantileError = "reg:quantileerror";
            /// <summary>
            /// Gamma regression with log-link.
            /// </summary>
            public const string Gamma = "reg:gamma";
            /// <summary>
            /// Tweedie regression with log-link.
            /// </summary>
            public const string Tweedie = "reg:tweedie";
        }

        /// <summary>
        /// Binary classification objectives.
        /// </summary>
        public static class Binary
        {
            /// <summary>
            /// Logistic regression for binary classification, output
            /// probability.
            /// </summary>
            public const string Logistic = "binary:logistic";
            /// <summary>
            /// Logistic regression for binary classification, output score
            /// before logistic transformation.
            /// </summary>
            public const string LogitRaw = "binary:logitraw";
            /// <summary>
            /// Hinge loss for binary classification.
            /// </summary>
            public const string Hinge = "binary:hinge";
        }

        /// <summary>
        /// Count objectives.
        /// </summary>
        public static class Count
        {
            /// <summary>
            /// Poisson regression for count data, output mean of Poisson
            /// distribution.
            /// </summary>
            public const string Poisson = "count:poisson";
        }

        /// <summary>
        /// Survival analysis objectives.
        /// </summary>
        public static class Survival
        {
            /// <summary>
            /// Cox regression for right censored survival time data.
            /// </summary>
            public const string Cox = "survival:cox";
            /// <summary>
            /// Accelerated failure time model for censored survival time data.
            /// </summary>
            public const string Aft = "survival:aft";
        }

        /// <summary>
        /// Multiclass classification objectives.
        /// </summary>
        public static class Multi
        {
            /// <summary>
            /// Multiclass classification using the softmax objective.
            /// </summary>
            public const string Softmax = "multi:softmax";
            /// <summary>
            /// Same as softmax, but output a vector of ndata * nclass.
            /// </summary>
            public const string Softprob = "multi:softprob";
        }

        /// <summary>
        /// Ranking objectives.
        /// </summary>
        public static class Rank
        {
            /// <summary>
            /// Use LambdaMART to perform pair-wise ranking where Normalized
            /// Discounted Cumulative Gain (NDCG) is maximized.
            /// </summary>
            public const string Ndcg = "rank:ndcg";
            /// <summary>
            /// Use LambdaMART to perform pair-wise ranking where Mean Average
            /// Precision (MAP) is maximized.
            /// </summary>
            public const string Map = "rank:map";
            /// <summary>
            /// Use LambdaRank to perform pair-wise ranking using the ranknet
            /// objective.
            /// </summary>
            public const string Pairwise = "rank:pairwise";
        }
    }
}
