using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TriangleNet.Voronoi;
using OsgiViz.SoftwareArtifact;

namespace OsgiViz.Unity.Island
{
    public class Region : MonoBehaviour
    {
        private Package package;
        private IslandGO parentIsland;
        private List<Building> buildings;
        private MeshFilter regionArea;


        private void Awake()
        {
            buildings = new List<Building>();
        }
        



        public Package getPackage()
        {
            return package;
        }
        public IslandGO getParentIsland()
        {
            return parentIsland;
        }
        public List<Building> getBuildings()
        {
            return buildings;
        }
        public MeshFilter getRegionMesh()
        {
            return regionArea;
        }

        public void addBuilding(Building cuGO)
        {
            buildings.Add(cuGO);
        }

        public void setPackage(Package pckg)
        {
            package = pckg;
        }
        public void setParentIsland(IslandGO parent)
        {
            parentIsland = parent;
        }
        public void setRegionArea(MeshFilter area)
        {
            regionArea = area;
        }

    }
}
