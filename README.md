[![Build Status](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/mdabros/XGBoostSharp/actions/workflows/dotnet.yml)
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

// Save and load the classifier.
var modelFileName = "classifier.json";
classifier.SaveModelToFile(modelFileName)
var loadedClassifier = XGBClassifier.LoadClassifierFromFile(modelFileName);
```

## XGBoostRegressor
```csharp
// Create and fit a regressor.
using var regressor = new XGBRegressor(maxDepth: 3, learningRate: 0.1f, nEstimators: 100);
regressor.Fit(dataTrain, labelsTrain);

// make predictions.
var predictions = regressor.Predict(dataTest);

// Save and load the regressor.
var modelFileName = "regressor.json";
regressor.SaveModelToFile(modelFileName)
var loadedRegressor = XGBRegressor.LoadFromFile(modelFileName);
```



