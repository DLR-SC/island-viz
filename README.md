**<h1>IslandViz Virtual Reality</h1>**

![IslandViz Virtual Reality](./Documentation/Logo_IslandViz-1b_RGB.png)

[![Licence](https://img.shields.io/hexpm/l/plug.svg)](https://github.com/DLR-SC/island-viz/blob/master/LICENSE)

> Unity 2019.3.0f1

> Please note that the __revised branch__ is an experimental branch. As such, there is no support offered and features may be incomplete, broken or removed in the future!

---

![IslandViz Virtual Reality Screenshots](./Documentation/IslandVizScreenshots_2.PNG)

IslandViz is a software visualization tool which represents the architecture of OSGi-based software systems in virtual reality. Using an island metaphor, each module is represented as an independent island. The resulting island system is displayed on a virtual table where users can interact with the visualization and explore the island system. IslandViz allows users to get an initial overview of the complexity of a software system by examining the modules and their dependencies.


---

## Getting Started

1. Clone the IslandViz repo recursively.

> git clone --recurse-submodules https://github.com/DLR-SC/island-viz.git

2. Remove the Microsoft.Contracts.dll at quickgraph4unity/src/Assets/quickgraph4unity/Dependencies/CodeContracts (Silverlight)/ .

3. Open the project in Unity.

---

## Importing a OSGi-based Software System

The IslandViz can currently import a software system either as a Json file or by connecting to a Neo4J server. 

>More infos to come ...

---

## Controls

![HTC Vive Controller](./Documentation/HTCViveController.png)

| Button | User-Action | Result                                     |
|--------|-------------|--------------------------------------------|
| 1      | PRESS       | Undo last Action                           |
| 2      | TOUCH       | RaycastSelection: Highlight                |
| 2      | PRESS       | RaycastSelection: Select                   |
| 3      | PRESS       | TableHeightAdjuster; InverseMultiTouchInput|
| 4      | PRESS       | Screenshot                                 |

>More infos to come ...

---

## For Developers

This revised branch is currently not targeted at developers, as things are going to change a lot. However, if you still want to customize the IslandViz or work on your own tool, here are some helpful information:  

>Note that this only applies to the revised branch! The main branch currently has another architecture.

The IslandViz application consists of three basic parts: 

- __IslandVizData__ imports the software data (either from a Json or a Neo4J server) and converts the data to a Osgi project.
- __IslandVizVisualization__ creates and stores the islands and ports based on the Osgi project. Also handles all visualization-based events.
- __IslandVizInteraction__ handles all user input events.

### Input-Events:

| Name                               | Params | Description                                     |
|------------------------------------|------|-------------------------------------------------|
| __OnControllerTriggerDown__        | Hand | _Called when trigger button is pressed._        |
| __OnControllerTriggerUp__          | Hand | _Called when trigger button is released._       |
| __OnControllerTouchpadDown__       | Hand | _Called when touchpad button is pressed._       |
| __OnControllerTouchpadUp__         | Hand | _Called when touchpad button is released._      |
| __OnControllerTouchpadTouchDown__  | Hand | _Called when touchpad is touched._              |
| __OnControllerTouchpadTouchUp__    | Hand | _Called when touchpad is not touched anymore._  |
| __OnControllerGripDown__           | Hand | _Called when grip button is pressed._           |
| __OnControllerGripUp__             | Hand | _Called when grip button is released._          |
| __OnControllerMenuDown__           | Hand | _Called when menu button is pressed._           |
| __OnControllerMenuUp__             | Hand | _Called when menu button is released._          |

### Physics-Events:

| Name                               | Params             | Description                                     |
|------------------------------------|--------------------|-------------------------------------------------|
| __OnControllerEnter__              | Collider, Hand     | _Called when a controller entered a trigger._   |
| __OnControllerExit__               | Collider, Hand     | _Called when a controller exited a trigger._    |

### Selection-Events:
| Name                               | Params                          | Description                                                      |
|------------------------------------|---------------------------------|------------------------------------------------------------------|
| __OnIslandSelect__                 | Island, SelectionType, Bool     | _Called when an island GameObject was selected or deselected._   |
| __OnRegionSelect__                 | Region, SelectionType, Bool     | _Called when a region GameObject was selected or deselected._    |
| __OnBuildingSelect__               | Building, SelectionType, Bool   | _Called when a building GameObject was selected or deselected._  |
| __OnDockSelect__                   | Dock, SelectionType, Bool       | _Called when a dock GameObject was selected or deselected._      |
| __OnUIButtonSelected__             | Button, SelectionType, Bool     | _Called when a UI button was selected or deselected._            |
| __OnOtherSelected__                | GameObject, SelectionType, Bool | _Called when any other object was selected or deselected._       |


![IslandViz Virtual Reality Screenshots](./Documentation/IslandVizGraph.PNG)

New interactions or visualizations can easily be added by writing __Additional Components__. As seen in the sequence diagram, after the basic build routine is done, additional components are loaded. Current Additional components are e.g. TableHeightAdjuster.cs or Compass.cs. To enable a additional component you just have to add the script to the corresponding _component container_ GameObject in the Unity scene. 

![IslandViz Virtual Reality Screenshots](./Documentation/IslandVizSequence.PNG)

>More infos to come ...

---

## Dependencies
JsonObject, QuickGraph4Unity, SteamVR, Triangle.NET C# port.

> All of these are included as sub-modules and are obtained automatically when the IslandViz repo is cloned recursively.
