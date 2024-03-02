# Note: This package is no longer mantained

# XGBoostSharp.Net
.Net wrapper for [XGBoostSharp](https://github.com/dmlc/xgboost) based off the [Python API](https://xgboost.readthedocs.io/en/latest/python/index.html).

Available as a [NuGet package](https://www.nuget.org/packages/PicNet.XGBoostSharp/).

Notes: For tests, loading the dll doesn't seem to work when referencing as a shared project, but loading as a nuget package works. So the Tests will fail in the XGBoostSharp solution but will work in the XGBoostSharpTests Solution.

Did multitargeting for the library (only) based on this post:
https://weblog.west-wind.com/posts/2017/jun/22/multitargeting-and-porting-a-net-library-to-net-core-20#Running-Tests-in-Visual-Studio----One-Framework-at-a-Time
