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

            //#region clickable
            //InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            //if (ict == null)
            //    ict = gameObject.AddComponent<InteractableViaClickTouch>();

            //ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            //#endregion

            //#region PdaInspectable
            //PdaInspectable pi = gameObject.GetComponent<PdaInspectable>();
            //if (pi == null)
            //    pi = gameObject.AddComponent<PdaInspectable>();
            //#endregion
        }
        


        private void handleActivationDeactivation(Hand hand)
        {
            string contentText = "";
            contentText += "<b><color=green>Name</b></color>";
            contentText += "\n";
            contentText += package.getName();
            contentText += "\n";
            contentText += "<b><color=green>#Compilation units</b></color>";
            contentText += "\n";
            contentText += package.getCuCount();

            gameObject.GetComponent<PdaInspectable>().sendContentToPda(contentText);
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
