
[![Licence](https://img.shields.io/hexpm/l/plug.svg)](https://github.com/DLR-SC/island-viz/blob/master/LICENSE)


Dependencies: 
JsonObject, QuickGraph4Unity, SteamVR, Triangle.NET C# port.

All of these are included as sub-modules and are obtained automatically when the IslandViz repo is cloned recursively:
"git clone --recurse-submodules https://github.com/DLR-SC/island-viz.git"

Additionally, you must remove the Microsoft.Contracts.dll at quickgraph4unity/src/Assets/quickgraph4unity/Dependencies/CodeContracts (Silverlight)/ .
 
Tested with Unity 2018.4.12f1
