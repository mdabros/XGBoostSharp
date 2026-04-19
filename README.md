[![Build Status](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/v/XGBoostSharp?color=orange)](https://www.nuget.org/packages/XGBoostSharp/)
[![downloads](https://img.shields.io/nuget/dt/XGBoostSharp)](https://www.nuget.org/packages/XGBoostSharp)
[![License](https://img.shields.io/github/license/mdabros/XGBoostSharp)](https://github.com/mdabros/XGBoostSharp/blob/master/LICENSE)

# XGBoostSharp
.Net wrapper for [XGBoost](https://github.com/dmlc/xgboost) based on the [Python API](https://xgboost.readthedocs.io/en/latest/python/index.html).
The main goal of the project is to provide a similar experience to the Python API, but in C#.

Currently supports `XGBoostClassifier` and `XGBoostRegressor`.

## XGBoostClassifier
```csharp
// Create and fit a classifier.
using var classifier = new XGBClassifier(maxDepth: 3, learningRate: 0.1f, nEstimators: 100);
classifier.Fit(dataTrain, labelsTrain);

// Predict.
var predictions = classifier.Predict(dataTest);
var probabilities = classifier.PredictProbability(dataTest);

// Predict multi-output. Binary class assignments shaped [n_samples, n_outputs].
var predictions = classifier.PredictMultiOutput(dataTest);
// Predict multi-output. Probabilities shaped [n_samples, n_outputs], values in [0, 1].
var probabilities = classifier.PredictProbabilityMultiOutput(dataTest);

// Get feature importance.
var featureImportance = classifier.GetFeatureImportance(ImportanceType.Weight);

// Save and load the classifier.
var modelFileName = "classifier.json";
classifier.SaveModelToFile(modelFileName);
var loadedClassifier = XGBClassifier.LoadFromFile(modelFileName);
```

## XGBoostRegressor
```csharp
// Create and fit a regressor.
using var regressor = new XGBRegressor(maxDepth: 3, learningRate: 0.1f, nEstimators: 100);
regressor.Fit(dataTrain, labelsTrain);

// Predict.
var predictions = regressor.Predict(dataTest);
// Predict multi-output. Outputs shaped [n_samples, n_outputs].
var predictions = regressor.PredictMultiOutput(dataTest);

// Get feature importance.
var featureImportance = regressor.GetFeatureImportance(ImportanceType.Weight);

// Save and load the regressor.
var modelFileName = "regressor.json";
regressor.SaveModelToFile(modelFileName)
var loadedRegressor = XGBRegressor.LoadFromFile(modelFileName);
```

## DMatrix
```csharp
// Create from a 2D float array.
using var dmatrix = new DMatrix(data, labels);

// Create from a 1D float array with explicit dimensions.
using var dmatrix = new DMatrix(data1D, nrows: 100, ncols: 4, labels);

// Load from a CSV file. Labels are not embedded in CSV, so assign them separately.
using var dmatrix = DMatrix.FromCsvFile("train.csv");
dmatrix.Label = labels;

// Load from a CSV file with a dedicated label column (0-based index).
using var dmatrix = DMatrix.FromCsvFile("train.csv", labelColumn: 0);

// Load from a LIBSVM file. Labels are read automatically from the file.
using var dmatrix = DMatrix.FromLibSvmFile("train.libsvm");

// Set feature names and types.
dmatrix.SetFeatureNames(["age", "fare", "pclass", "sibsp"]);
dmatrix.SetFeatureTypes([FeatureType.Integer, FeatureType.Float, FeatureType.Integer, FeatureType.Integer]);
```

## Installation

1. Get the latest version of the managed packages from nuget.org.
   - https://www.nuget.org/packages/XGBoostSharp. Note that this requires a
     reference to one of the native packages (see below).
   - https://www.nuget.org/packages/XGBoostSharp-cpu: This comes with native
     packages for cpu for win-x64, linux-x64, osx-x64, and osx-arm64.
   - https://www.nuget.org/packages/XGBoostSharp-cuda-windows: This comes with native
     packages for win-x64 and packages for CUDA 12.8 on Windows.
2. If using the XGBoostSharp package the native packages can be installed
   separately from nuget.org.
   - x64 packages:
     - https://www.nuget.org/packages/libxgboost-3.2.0-win-x64/
     - https://www.nuget.org/packages/libxgboost-3.2.0-linux-x64/
     - https://www.nuget.org/packages/libxgboost-3.2.0-osx-x64/
   - ARM64 packages:
     - https://www.nuget.org/packages/libxgboost-3.2.0-osx-arm64/
