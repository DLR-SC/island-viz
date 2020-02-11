using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using TriangleNet.Voronoi;
using TriangleNet.Topology;
using OsgiViz.Island;
using OsgiViz.SoftwareArtifact;
using OSGI_Datatypes.ComposedTypes;

namespace OsgiViz.Unity.Island
{

    public class IslandGO : MonoBehaviour
    {
        public List<Region> Regions { get; private set; }
        public GameObject Coast { get; set; }
        public GameObject ImportDock { get; set; }
        public GameObject ExportDock { get; set; }
        public CartographicIsland CartoIsland { get; set; }

        public Bundle Bundle { get; set; }

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
                // Enable all children, i.e. make island visible.
                foreach (var region in Regions)
                {
                    region.gameObject.SetActive(true);
                }
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
                Visible = false;
            }            
        }

        

        // ################
        // Zoom Level
        // ################

        #region Zoom Level

        public void ApplyZoomLevel (ZoomLevel newZoomLevel)
        {
            if (!gameObject.activeSelf)
                return;
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

            foreach (var region in Regions)
            {
                // Enable buildings.
                foreach (var building in region.getBuildings())
                {
                    if (!building.gameObject.activeSelf)
                    {
                        building.gameObject.SetActive(true);
                        if (HistoryNavigation.Instance.historyHighlightActive)
                        {
                            if (building.gameObject.transform.Find("ChangeIndicator") != null)
                            {
                                building.gameObject.transform.Find("ChangeIndicator").gameObject.SetActive(true);
                            }
                        }

                        counter++;
                        if (counter >= BuildingsPerFrame)
                        {
                            counter = 0;
                            yield return null;
                        }
                    }
                }
            }
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
            }
            // FAR -> MEDIUM
            else
            {
                //Disable changeIndikator if in HistoryViz (meaning IslandController_Script is present)
                if (gameObject.GetComponent<IslandController_Script>() != null)
                {
                    gameObject.GetComponent<IslandController_Script>().changeIndikator.SetActive(false);
                }

                // Enable Docks.
                if (!ImportDock.activeSelf)
                {
                    if (ImportDock.GetComponent<DependencyDock>().HasArrows())
                    {
                        ImportDock.SetActive(true);
                    }
                    if (ExportDock.GetComponent<DependencyDock>().HasArrows())
                    {
                        ExportDock.SetActive(true);
                    }
                }                

                // Enable region colliders.
                foreach (var region in Regions)
                {
                    if (!region.GetComponent<MeshCollider>().enabled)
                        region.GetComponent<MeshCollider>().enabled = true;
                }
                //Disable Island Collider

                if (GetComponent<MeshCollider>() != null)
                {
                    GetComponent<MeshCollider>().enabled = false;

                }
                else if (GetComponent<SphereCollider>() != null)
                {
                    GetComponent<SphereCollider>().enabled = false;
                }
            }
        }
        
        public IEnumerator ApplyFarZoomLevel ()
        {
            int counter = 0;
            //Enable ChangeStatus Indikator if in HistoryViz (meaning IslandController_Script is present)
            if (gameObject.GetComponent<IslandController_Script>() != null)
            {
                ChangeStatus cs = gameObject.GetComponent<IslandController_Script>().changeStatus;
                if ((cs.Equals(ChangeStatus.changedElement) || cs.Equals(ChangeStatus.newElement) ||cs.Equals(ChangeStatus.changedInnerElement)) && HistoryNavigation.Instance.historyHighlightActive)
                {
                    gameObject.GetComponent<IslandController_Script>().changeIndikator.SetActive(true);
                }
            }

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
            if (GetComponent<MeshCollider>() != null)
            {
                GetComponent<MeshCollider>().enabled = true;

            }
            else if (GetComponent<SphereCollider>() != null)
            {
                GetComponent<SphereCollider>().enabled = true;
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

        public void SetRegions(List<Region> regs)
        {
            Regions = regs;
        }

        public void ResetRegions()
        {
            Regions = new List<Region>();
        }
    }
}
