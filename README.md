﻿[![Build Status](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml)
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

// make predictions.
var predictions = classifier.Predict(dataTest);
var probabilities = classifier.PredictProbability(dataTest);

// Get feature importance.
var featureImportance = classifier.GetFeatureImportance(ImportanceType.Weight);

// Save and load the classifier.
var modelFileName = "classifier.json";
classifier.SaveModelToFile(modelFileName)
var loadedClassifier = XGBClassifier.LoadFromFile(modelFileName);
```

## XGBoostRegressor
```csharp
// Create and fit a regressor.
using var regressor = new XGBRegressor(maxDepth: 3, learningRate: 0.1f, nEstimators: 100);
regressor.Fit(dataTrain, labelsTrain);

// make predictions.
var predictions = regressor.Predict(dataTest);

// Get feature importance.
var featureImportance = regressor.GetFeatureImportance(ImportanceType.Weight);

// Save and load the regressor.
var modelFileName = "regressor.json";
regressor.SaveModelToFile(modelFileName)
var loadedRegressor = XGBRegressor.LoadFromFile(modelFileName);
```

## Installation

1. Get the latest version of the managed packages from nuget.org.
   - https://www.nuget.org/packages/XGBoostSharp. Note that this requires a
     reference to one of the native packages (see below).
   - https://www.nuget.org/packages/XGBoostSharp-cpu: This comes with native
     packages for cpu for win-x64, linux-x64, and osx-x64.
2. If using the XGBoostSharp package the native packages can be installed
   separately from nuget.org.
   - x64 packages:
     - https://www.nuget.org/packages/libxgboost-2.0.3-win-x64/
     - https://www.nuget.org/packages/libxgboost-2.0.3-linux-x64/
     - https://www.nuget.org/packages/libxgboost-2.0.3-osx-x64/
   - ARM64 packages (note that these are not part of the XGBoostSharp-cpu
     package):
     - https://www.nuget.org/packages/libxgboost-2.0.3-osx-arm64/
