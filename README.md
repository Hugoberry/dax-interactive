# dax-interactive
An extension for Polyglot Notebooks that adds a custom kernel for querying PowerBI/SSAS and other flavours of Tabular models via DAX.
[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/Hugoberry/dax-interactive/HEAD)
## How to use it
Follow the 3 steps:
1. Add the nugget reference
2. Connect to the your tabular model
3. Run the DAX queries referencing the above kernel
```cs
#r "nuget: Dax.Interactive,*-*"
#!connect dax  --kernel-name model "Provider=MSOLAP;Data Source=powerbi://api.powerbi.com/v1.0/myorg/YOUR_WORKSPACE;initial catalog=YPUR_MODEL;"
#!dax-model
EVALUATE TOPN(5,Dimension)
```
![screenshot of Polyglot Notebook with DAX kernel](image.png)
