using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TriangleNet.Voronoi;
using TriangleNet.Topology;
using OsgiViz.Island;
using OsgiViz.SoftwareArtifact;

namespace OsgiViz.Unity.Island
{

    public class IslandGO : MonoBehaviour
    {
        public List<Region> Regions { get; private set; }
        public CartographicIsland CartoIsland { get; set; }

        // GameObjects
        public GameObject Coast { get; set; }
        public GameObject ImportDock { get; set; }
        public GameObject ExportDock { get; set; }


        // Settings

        public ZoomLevel CurrentZoomLevel;

        public bool Selected;
        public bool Highlighted;
        public bool Visible;

        // Performance Settings
        private float BuildingsPerFrame = 50;

        void Awake()
        {
            Regions = new List<Region>();
            ImportDock = null;
            ExportDock = null;
            Coast = null;

            IslandVizInteraction.Instance.OnIslandSelect += OnSelection;
            IslandVizVisualization.Instance.OnIslandVisible += MakeIslandVisible;
            IslandVizVisualization.Instance.OnIslandInvisible += MakeIslandInvisible;
        }



        // ################
        // Selection Events
        // ################

        private void OnSelection (IslandGO island, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            if (island != this && island != null && selectionType == IslandVizInteraction.SelectionType.Select) 
            {
                if (selected && Selected) // Another island was selected while this island was selected.
                {
                    Selected = false;
                    IslandVizInteraction.Instance.OnIslandSelect(this, IslandVizInteraction.SelectionType.Select, false);
                }
                if (selected && Highlighted)
                {
                    Highlighted = false;
                    IslandVizInteraction.Instance.OnIslandSelect(this, IslandVizInteraction.SelectionType.Highlight, false);
                }
            }
            else if (island == this && selectionType == IslandVizInteraction.SelectionType.Select && Selected != selected) // This island was selected/deselected.
            {
                Selected = selected;
            }
            else if (island == null && selectionType == IslandVizInteraction.SelectionType.Select && Selected != selected) // All islands wer deselected.
            {
                Selected = selected;
                IslandVizInteraction.Instance.OnIslandSelect(this, IslandVizInteraction.SelectionType.Select, selected);
            }
            else if (island == this && selectionType == IslandVizInteraction.SelectionType.Highlight && selected != Highlighted)
            {
                Highlighted = selected;
            }
            else if (island == null && selectionType == IslandVizInteraction.SelectionType.Highlight && selected != Highlighted)
            {
                IslandVizInteraction.Instance.OnIslandSelect(this, IslandVizInteraction.SelectionType.Highlight, selected);
                Highlighted = selected;
            }
        }


        // ################
        // Physics
        // ################
        #region Physics

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "TableContent")
            {
                IslandVizVisualization.Instance.OnIslandVisible(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "TableContent")
            {
                IslandVizVisualization.Instance.OnIslandInvisible(this);
            }
        }

        #endregion



        // ################
        // Visible & Invisible
        // ################

        private void MakeIslandVisible (IslandGO island)
        {
            if (island == this)
            {
                Visible = true;
            }            
        }

        private void MakeIslandInvisible(IslandGO island)
        {
            if (island == this)
            {
                // Disable all children, i.e. make island invisible.
                foreach (var region in Regions)
                {
                    region.gameObject.SetActive(false);
                }
                Coast.SetActive(false);
                ImportDock.SetActive(false);
                ExportDock.SetActive(false);

                CurrentZoomLevel = ZoomLevel.None;

                Visible = false;
            }            
        }

        

        // ################
        // Zoom Level
        // ################

        #region Zoom Level

        public void ApplyZoomLevel (ZoomLevel newZoomLevel)
        {
            StartCoroutine(ApplyZoomLevelRoutine(newZoomLevel));
        }

        /// <summary>
        /// Apply the rules of all ZoomLevels to an island. 
        /// Call this to change the Zoomlevel of an island.
        /// </summary>
        /// <param name="newZoomLevel">The ZoomLevel that you want to apply to the island.</param>
        /// <returns></returns>
        public IEnumerator ApplyZoomLevelRoutine (ZoomLevel newZoomLevel)
        {
            if (CurrentZoomLevel == newZoomLevel || !Visible)
            {
                // Do nothing.
            }
            else if (newZoomLevel == ZoomLevel.Near)
            {
                yield return EnableIslandElements(true, true, true, true, true);
            }
            else if (newZoomLevel == ZoomLevel.Medium)
            {
                yield return EnableIslandElements(true, true, true, true, false);
            }
            else if (newZoomLevel == ZoomLevel.Far)
            {
                yield return EnableIslandElements(true, false, true, false, false);
            }
            CurrentZoomLevel = newZoomLevel;
        }
        
        public IEnumerator EnableIslandElements (bool enableCoast, bool enableDocks, bool enableRegions, bool enableRegionColliders, bool enableBuildings)
        {
            int counter = 0; // Counter for performance optimization.

            if (Coast.activeSelf != enableCoast)
                Coast.SetActive(enableCoast);

            if (ImportDock.activeSelf != enableDocks)
                ImportDock.SetActive(enableDocks);

            if (ExportDock.activeSelf != enableDocks)
                ExportDock.SetActive(enableDocks);

            foreach (var region in Regions)
            {
                if (region.gameObject.activeSelf != enableRegions)
                    region.gameObject.SetActive(enableRegions);

                if (region.GetComponent<MeshCollider>().enabled != enableRegionColliders)
                    region.GetComponent<MeshCollider>().enabled = enableRegionColliders;

                foreach (var building in region.getBuildings())
                {
                    if (building.gameObject.activeSelf != enableBuildings)
                    {
                        building.gameObject.SetActive(enableBuildings);
                        counter++;
                        if (counter >= BuildingsPerFrame) { counter = 0; yield return null; }
                    }
                }
            }
        }       

        #endregion




        // ################
        // Helper Functions
        // ################

        //Returns true if island does not contain a single CU. Returns false otherwise.
        public bool IsIslandEmpty()
        {
            foreach (Region reg in Regions)
                if (reg.getBuildings().Count > 0)
                    return false;

            return true;
        }

        public void AddRegion(Region reg)
        {
            Regions.Add(reg);
        }

    }
}
