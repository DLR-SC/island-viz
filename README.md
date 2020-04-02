**<h1>IslandViz Virtual Reality</h1>**

![IslandViz Virtual Reality](./Documentation/Logo_IslandViz-1b_RGB.png)

[![](https://img.shields.io/badge/Unity-2019.3.0f1-blue.svg?style=flat)](https://unity3d.com/get-unity/download/archive)
[![Licence](https://img.shields.io/hexpm/l/plug.svg)](https://github.com/DLR-SC/island-viz/blob/master/LICENSE)

> Please note that the __revised branch__ is an experimental branch. As such, there is no support offered and features may be incomplete, broken or removed in the future!

---

IslandViz is a software visualization tool which represents the architecture of OSGi-based software systems in virtual reality. Using an island metaphor, each module is represented as an independent island. The resulting island system is displayed on a virtual table where users can interact with the visualization and explore the island system. IslandViz allows users to get an initial overview of the complexity of a software system by examining the modules and their dependencies.

![IslandViz Virtual Reality Screenshots](./Documentation/IslandVizScreenshots_2.PNG)




---

## Getting Started

1. Clone the IslandViz repo recursively.

> git clone --recurse-submodules https://github.com/DLR-SC/island-viz.git

2. Delete the _Dependencies_ folder at \Assets\QuickGraph4Unity\src\Assets\quickgraph4unity\ .

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
| __OnControllerButtonEvent__        | Button, PressType, Hand | _Called when a button of a controller is pressed, released or touched._        |

>__Buttons__: Trigger, Menu, Touchpad, Grip

>__PressTypes__: PressDown, PressUp, TouchDown, TouchUp


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

>__SelectionTypes__: Select, Highlight 

[![](https://mermaid.ink/img/eyJjb2RlIjoiZ3JhcGggVERcbiAgc3ViZ3JhcGggTWFpbiBBcHBsaWNhdGlvblxuICAgICAgSXNsYW5kVml6QmVoYXZpb3VyLS0-SXNsYW5kVml6SW50ZXJhY3Rpb25cbiAgICAgIElzbGFuZFZpekJlaGF2aW91ci0tPklzbGFuZFZpekRhdGFcbiAgICAgIElzbGFuZFZpekJlaGF2aW91ci0tPklzbGFuZFZpelZpc3VhbGl6YXRpb25cbiAgZW5kXG4gICAgc3ViZ3JhcGggQWRkaXRpb25hbCBDb21wb25lbnRzXG4gICAgICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-VmlzdWFsaXphdGlvbkNvbXBvbmVudHNcbiAgICAgIElzbGFuZFZpekRhdGEtLT5EYXRhQ29tcG9uZW50c1xuICAgICAgSXNsYW5kVml6SW50ZXJhY3Rpb24tLT5JbnRlcmFjdGlvbkNvbXBvbmVudHNcblx0ZW5kIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifSwidXBkYXRlRWRpdG9yIjpmYWxzZX0)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoiZ3JhcGggVERcbiAgc3ViZ3JhcGggTWFpbiBBcHBsaWNhdGlvblxuICAgICAgSXNsYW5kVml6QmVoYXZpb3VyLS0-SXNsYW5kVml6SW50ZXJhY3Rpb25cbiAgICAgIElzbGFuZFZpekJlaGF2aW91ci0tPklzbGFuZFZpekRhdGFcbiAgICAgIElzbGFuZFZpekJlaGF2aW91ci0tPklzbGFuZFZpelZpc3VhbGl6YXRpb25cbiAgZW5kXG4gICAgc3ViZ3JhcGggQWRkaXRpb25hbCBDb21wb25lbnRzXG4gICAgICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-VmlzdWFsaXphdGlvbkNvbXBvbmVudHNcbiAgICAgIElzbGFuZFZpekRhdGEtLT5EYXRhQ29tcG9uZW50c1xuICAgICAgSXNsYW5kVml6SW50ZXJhY3Rpb24tLT5JbnRlcmFjdGlvbkNvbXBvbmVudHNcblx0ZW5kIiwibWVybWFpZCI6eyJ0aGVtZSI6ImRlZmF1bHQifSwidXBkYXRlRWRpdG9yIjpmYWxzZX0)

The IslandViz can be extended by writing __Additional Components__. As seen in the sequence diagram, after the basic build routine is done, additional components are loaded. Current Additional components are e.g. TableHeightAdjuster.cs or RaycastSelection.cs. To enable a additional component you just have to add the script to the corresponding _component container_ GameObject in the Unity scene. 

[![](https://mermaid.ink/img/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG5yZWN0IHJnYigwLCAwLCAyNTUsIDAuMilcbiAgTm90ZSBvdmVyIElzbGFuZFZpekJlaGF2aW91cixJc2xhbmRWaXpEYXRhIDogTWFpbiBGdW5jdGlvbmFsaXR5XG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpEYXRhOiBDb25zdHJ1Y3RPc2dpUHJvamVjdFxuXHRJc2xhbmRWaXpEYXRhLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuICBJc2xhbmRWaXpCZWhhdmlvdXItPj4rSXNsYW5kVml6VmlzdWFsaXphdGlvbjogQ29uc3RydWN0VmlzdWFsaXphdGlvblxuICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuZW5kXG5cbnJlY3QgcmdiKDAsIDAsIDI1NSwgMC4wNSlcbiAgTm90ZSBvdmVyIElzbGFuZFZpekJlaGF2aW91cixJc2xhbmRWaXpEYXRhIDogT3B0aW9uYWwgRnVuY3Rpb25hbGl0eVxuICBJc2xhbmRWaXpCZWhhdmlvdXItPj4rSXNsYW5kVml6RGF0YTogSW5pdEFkZGl0aW9uYWxEYXRhQ29tcG9uZW50c1xuICBsb29wIGZvcmVhY2ggY29tcG9uZW50XG4gIElzbGFuZFZpekRhdGEtPj5Jc2xhbmRWaXpEYXRhOiBJbml0QWRkaXRpb25hbENvbXBvbmVudFxuXHRlbmRcbiAgSXNsYW5kVml6RGF0YS0tPj4tSXNsYW5kVml6QmVoYXZpb3VyOiB5aWVsZCByZXR1cm5cblxuXG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpWaXN1YWxpemF0aW9uOiBJbml0QWRkaXRpb25hbFZpc3VhbGl6YXRpb25Db21wb25lbnRzXG4gIGxvb3AgZm9yZWFjaCBjb21wb25lbnRcbiAgSXNsYW5kVml6VmlzdWFsaXphdGlvbi0-PklzbGFuZFZpelZpc3VhbGl6YXRpb246IEluaXRBZGRpdGlvbmFsQ29tcG9uZW50XG5cdGVuZFxuICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuXG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpJbnRlcmFjdGlvbjogSW5pdEFkZGl0aW9uYWxJbnRlcmFjdGlvbkNvbXBvbmVudHNcbiAgbG9vcCBmb3JlYWNoIGNvbXBvbmVudFxuICBJc2xhbmRWaXpJbnRlcmFjdGlvbi0-PklzbGFuZFZpekludGVyYWN0aW9uOiBJbml0QWRkaXRpb25hbENvbXBvbmVudFxuXHRlbmRcbiAgSXNsYW5kVml6SW50ZXJhY3Rpb24tLT4-LUlzbGFuZFZpekJlaGF2aW91cjogeWllbGQgcmV0dXJuXG5lbmQiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG5yZWN0IHJnYigwLCAwLCAyNTUsIDAuMilcbiAgTm90ZSBvdmVyIElzbGFuZFZpekJlaGF2aW91cixJc2xhbmRWaXpEYXRhIDogTWFpbiBGdW5jdGlvbmFsaXR5XG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpEYXRhOiBDb25zdHJ1Y3RPc2dpUHJvamVjdFxuXHRJc2xhbmRWaXpEYXRhLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuICBJc2xhbmRWaXpCZWhhdmlvdXItPj4rSXNsYW5kVml6VmlzdWFsaXphdGlvbjogQ29uc3RydWN0VmlzdWFsaXphdGlvblxuICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuZW5kXG5cbnJlY3QgcmdiKDAsIDAsIDI1NSwgMC4wNSlcbiAgTm90ZSBvdmVyIElzbGFuZFZpekJlaGF2aW91cixJc2xhbmRWaXpEYXRhIDogT3B0aW9uYWwgRnVuY3Rpb25hbGl0eVxuICBJc2xhbmRWaXpCZWhhdmlvdXItPj4rSXNsYW5kVml6RGF0YTogSW5pdEFkZGl0aW9uYWxEYXRhQ29tcG9uZW50c1xuICBsb29wIGZvcmVhY2ggY29tcG9uZW50XG4gIElzbGFuZFZpekRhdGEtPj5Jc2xhbmRWaXpEYXRhOiBJbml0QWRkaXRpb25hbENvbXBvbmVudFxuXHRlbmRcbiAgSXNsYW5kVml6RGF0YS0tPj4tSXNsYW5kVml6QmVoYXZpb3VyOiB5aWVsZCByZXR1cm5cblxuXG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpWaXN1YWxpemF0aW9uOiBJbml0QWRkaXRpb25hbFZpc3VhbGl6YXRpb25Db21wb25lbnRzXG4gIGxvb3AgZm9yZWFjaCBjb21wb25lbnRcbiAgSXNsYW5kVml6VmlzdWFsaXphdGlvbi0-PklzbGFuZFZpelZpc3VhbGl6YXRpb246IEluaXRBZGRpdGlvbmFsQ29tcG9uZW50XG5cdGVuZFxuICBJc2xhbmRWaXpWaXN1YWxpemF0aW9uLS0-Pi1Jc2xhbmRWaXpCZWhhdmlvdXI6IHlpZWxkIHJldHVyblxuXG4gIElzbGFuZFZpekJlaGF2aW91ci0-PitJc2xhbmRWaXpJbnRlcmFjdGlvbjogSW5pdEFkZGl0aW9uYWxJbnRlcmFjdGlvbkNvbXBvbmVudHNcbiAgbG9vcCBmb3JlYWNoIGNvbXBvbmVudFxuICBJc2xhbmRWaXpJbnRlcmFjdGlvbi0-PklzbGFuZFZpekludGVyYWN0aW9uOiBJbml0QWRkaXRpb25hbENvbXBvbmVudFxuXHRlbmRcbiAgSXNsYW5kVml6SW50ZXJhY3Rpb24tLT4-LUlzbGFuZFZpekJlaGF2aW91cjogeWllbGQgcmV0dXJuXG5lbmQiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)

>More infos to come ...

---

## Dependencies
JsonObject, QuickGraph4Unity, SteamVR, Triangle.NET C# port.

> All of these are included as sub-modules and are obtained automatically when the IslandViz repo is cloned recursively.
