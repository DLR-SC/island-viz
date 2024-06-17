[![](https://img.shields.io/badge/Unity-2021.3.11f1-blue.svg?style=flat)](https://unity3d.com/get-unity/download/archive)
[![](https://img.shields.io/badge/Entites-0.51.1-blue.svg?style=flat)](https://docs.unity3d.com/Packages/com.unity.entities@0.16/manual/index.html)
[![](https://img.shields.io/badge/Neo4J-4.0.0+-blue.svg?style=flat)](https://github.com/neo4j/neo4j-dotnet-driver)

# IslandViz DOTS

Data-oriented re-implementation of the OSGi software structure visualization tool [IslandViz VR](https://github.com/DLR-SC/island-viz) using Unity's new data-oriented technology stack (DOTS) and OpenXR. Unlike the legacy IslandViz, IslandViz DOTS is not restricted to OSGi software structures and is able do visualize almost any structure stored in a Neo4j database.

![](./header.PNG)

## Features

|  | Description |
| --- | --- | 
| __Neo4j Interface__ | Import nodes & relationships from a Neo4j database. |
| __Custom Data__ | Define custom node labels & relationship types to visualize any (hierarchical graph) data as islands. |
| __FD-Layout__ | Automatic island layout with a realtime force-directed (FD) graph layout. |
| __Data-Oriented__ | Clean and reusable code with Unity's entity-component-system (ECS). |
| __DOTS__ | High performance due to the use of ECS, C# Job System, Burst Compiler, and Hybrid Renderer (see [Unity DOTS](https://unity.com/dots)).|
| __2D/3D View__ | Explore the data with either a VR-based 3D ar a desktop-based 2D representation. |
| __OpenXR__ | With [OpenXR](https://www.khronos.org/OpenXR/) we can targets a wide range of AR/VR devices without having to port or re-write code. |
| __Voice Input__ | Select individual island components by saying "select X". |
| __Plugins__ | Expand the base IslandViz functionality with plugins. |

---

__See the wiki page for more information.__ :muscle: