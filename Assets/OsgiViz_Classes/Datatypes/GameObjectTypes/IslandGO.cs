using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TriangleNet.Voronoi;
using TriangleNet.Topology;
using OsgiViz.Island;

namespace OsgiViz.Unity.Island
{

    public class IslandGO : MonoBehaviour
    {
        public List<Region> Regions { get; private set; }
        public GameObject Coast { get; set; }
        public GameObject ImportDock { get; set; }
        public GameObject ExportDock { get; set; }
        public CartographicIsland CartoIsland { get; set; }

        public ZoomLevel CurrentZoomLevel;

        public bool Selected;
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
        }



        // ################
        // Selection Events
        // ################

        private void OnSelection (IslandGO island, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            if (island != this && selectionType == IslandVizInteraction.SelectionType.Select && selected) // Another island was selected while this island was selected.
            {
                if (Selected)
                {
                    Selected = false;
                    IslandVizInteraction.Instance.OnIslandSelect(this, IslandVizInteraction.SelectionType.Select, false);
                }
            }
            else if (island == this && selectionType == IslandVizInteraction.SelectionType.Select) // This island was selected/deselected.
            {
                Selected = selected;
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
                MakeIslandVisible();
                IslandVizVisualization.Instance.OnIslandVisible(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "TableContent")
            {
                MakeIslandInvisible();
                IslandVizVisualization.Instance.OnIslandInvisible(this);
            }
        }

        #endregion



        // ################
        // Visible & Invisible
        // ################

        private void MakeIslandVisible ()
        {
            // Enable all children, i.e. make island visible.
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            Visible = true;
        }

        private void MakeIslandInvisible()
        {
            // Disable all children, i.e. make island invisible.
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            Visible = false;
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
            if (CurrentZoomLevel == newZoomLevel)
            {
                // Do nothing.
            }
            else if (newZoomLevel == ZoomLevel.Near)
            {
                yield return ApplyNearZoomLevel();
            }
            else if (newZoomLevel == ZoomLevel.Medium)
            {
                yield return ApplyMediumZoomLevel();
            }
            else if (newZoomLevel == ZoomLevel.Far)
            {
                yield return ApplyFarZoomLevel();
            }
            CurrentZoomLevel = newZoomLevel;
        }
        
        public IEnumerator ApplyNearZoomLevel()
        {
            int counter = 0;

            // Disable region colliders & enable buildings.
            foreach (var region in Regions)
            {
                if (region.GetComponent<MeshCollider>().enabled)
                    region.GetComponent<MeshCollider>().enabled = true; // TODO?

                foreach (var building in region.getBuildings())
                {
                    if (!building.gameObject.activeSelf)
                    {
                        building.gameObject.SetActive(true);

                        counter++;
                        if (counter >= BuildingsPerFrame)
                        {
                            counter = 0;
                            yield return null;
                        }
                    }
                }
            }

            //// Disable island collider.
            //if (GetComponent<MeshCollider>().enabled)
            //    GetComponent<MeshCollider>().enabled = false;
        }
        
        public IEnumerator ApplyMediumZoomLevel()
        {
            int counter = 0;

            // NEAR -> MEDIUM 
            if (CurrentZoomLevel == ZoomLevel.Near)
            {
                foreach (var region in Regions)
                {
                    foreach (var building in region.getBuildings())
                    {
                        if (building.gameObject.activeSelf)
                        {
                            building.gameObject.SetActive(false);

                            counter++;
                            if (counter >= BuildingsPerFrame)
                            {
                                counter = 0;
                                yield return null;
                            }
                        }
                    }
                }
                //if (!GetComponent<MeshCollider>().enabled)
                //    GetComponent<MeshCollider>().enabled = true;                
            }
            // FAR -> MEDIUM
            else
            {
                // Enable Docks.
                if (!ImportDock.activeSelf)
                {
                    ImportDock.SetActive(true);
                    ExportDock.SetActive(true);
                }                

                // Enable region colliders.
                foreach (var region in Regions)
                {
                    if (!region.GetComponent<MeshCollider>().enabled)
                        region.GetComponent<MeshCollider>().enabled = true;
                }

                //if (!GetComponent<MeshCollider>().enabled)
                //    GetComponent<MeshCollider>().enabled = true;
            }
        }
        
        public IEnumerator ApplyFarZoomLevel ()
        {
            int counter = 0;

            // Hide Docks.
            if (ImportDock.activeSelf)
            {
                ImportDock.SetActive(false);
                ExportDock.SetActive(false);
            }   

            // Disable region colliders & hide buildings.
            foreach (var region in Regions)
            {
                if (region.GetComponent<MeshCollider>().enabled)
                    region.GetComponent<MeshCollider>().enabled = false;
                
                foreach (var building in region.getBuildings())
                {
                    if (building.gameObject.activeSelf)
                    {
                        building.gameObject.SetActive(false);

                        counter++;
                        if (counter >= BuildingsPerFrame)
                        {
                            counter = 0;
                            yield return null;
                        }
                    }
                }
            }

            // Enable island collider.
            //GetComponent<MeshCollider>().enabled = true;
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
