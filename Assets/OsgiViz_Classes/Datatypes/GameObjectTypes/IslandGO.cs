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
        public ZoomLevel CurrentZoomLevel;

        private CartographicIsland island;
        private List<Region> regions;
        private GameObject coast;
        private GameObject importDock;
        private GameObject exportDock;


        void Awake()
        {
            regions = new List<Region>();
            importDock = null;
            exportDock = null;
            coast = null;

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


        // ################
        // Events
        // ################

        
        /// <summary>
        /// Called when the island GameObject is enabled.
        /// </summary>
        public IslandEnabled OnIslandVisible;
        /// <summary>
        /// Called when the island GameObject is disabled.
        /// </summary>
        public IslandDisabled OnIslandInvisible;


        // ################
        // Delegates
        // ################

        /// <summary>
        /// Called when the island GameObject is enabled.
        /// </summary>
        public delegate void IslandEnabled();
        /// <summary>
        /// Called when the island GameObject is disabled.
        /// </summary>
        public delegate void IslandDisabled();
        




        // ################
        // Physics
        // ################
        #region Physics

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "TableContent")
            {
                IslandVizVisualization.Instance.MakeIslandVisible(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "TableContent")
            {
                IslandVizVisualization.Instance.MakeIslandInvisible(this);
            }
        }

        #endregion



        


        /// <summary>
        /// This Method contains and applies the rules of all ZoomLevels to an island. 
        /// Call this to change the Zoomlevel of an island.
        /// </summary>
        /// <param name="newZoomLevel">The ZoomLevel that you want to apply to the island.</param>
        /// <returns></returns>
        public IEnumerator ApplyZoomLevel(ZoomLevel newZoomLevel)
        {
            if (CurrentZoomLevel == newZoomLevel)
            {
                // Do nothing.
            }
            else if (newZoomLevel == ZoomLevel.Near)
            {
                yield return NearZoomLevel();
            }
            else if (newZoomLevel == ZoomLevel.Medium)
            {
                yield return MediumZoomLevel();
            }
            else if (newZoomLevel == ZoomLevel.Far)
            {
                yield return FarZoomLevel();
            }
            CurrentZoomLevel = newZoomLevel;
        }


        public IEnumerator NearZoomLevel()
        {
            // Disable region colliders & enable buildings.
            foreach (var region in regions)
            {
                if (region.GetComponent<MeshCollider>().enabled)
                    region.GetComponent<MeshCollider>().enabled = true; // TODO?

                foreach (var building in region.getBuildings())
                {
                    if (!building.gameObject.activeSelf)
                        building.gameObject.SetActive(true);
                }
                yield return null;
            }

            // Disable island collider.
            if (GetComponent<CapsuleCollider>().enabled)
                GetComponent<CapsuleCollider>().enabled = false;
        }


        public IEnumerator MediumZoomLevel()
        {
            // NEAR -> MEDIUM 
            if (CurrentZoomLevel == ZoomLevel.Near)
            {
                foreach (var region in regions)
                {
                    foreach (var building in region.getBuildings())
                    {
                        if (building.gameObject.activeSelf)
                            building.gameObject.SetActive(false);
                    }
                    yield return null;
                }
            }
            // FAR -> MEDIUM
            else
            {
                // Enable Docks.
                if (!importDock.activeSelf)
                {
                    importDock.SetActive(true);
                    exportDock.SetActive(true);
                }                

                // Enable region colliders.
                foreach (var region in regions)
                {
                    if (!region.GetComponent<MeshCollider>().enabled)
                        region.GetComponent<MeshCollider>().enabled = true;
                }

                // Disable island collider.
                if (GetComponent<CapsuleCollider>().enabled)
                    GetComponent<CapsuleCollider>().enabled = false;
            }
        }


        public IEnumerator FarZoomLevel ()
        {
            // Hide Docks.
            if (importDock.activeSelf)
            {
                importDock.SetActive(false);
                exportDock.SetActive(false);
            }   

            // Disable region colliders & hide buildings.
            foreach (var region in regions)
            {
                if (region.GetComponent<MeshCollider>().enabled)
                    region.GetComponent<MeshCollider>().enabled = false;

                foreach (var building in region.getBuildings())
                {
                    if (building.gameObject.activeSelf)
                        building.gameObject.SetActive(false);
                }
                yield return null;
            }

            // Enable island collider.
            GetComponent<CapsuleCollider>().enabled = true;
        }





        private void handleActivationDeactivation(Hand hand)
        {
            string contentText = "";
            contentText += "<b><color=green>Name</b></color>";
            contentText += "\n";
            contentText += island.getName();
            contentText += "\n";
            contentText += "<b><color=green>#Packages</b></color>";
            contentText += "\n";
            contentText += island.getPackages().Count;


            gameObject.GetComponent<PdaInspectable>().sendContentToPda(contentText);
        }






        //Returns true if island does not contain a single CU. Returns false otherwise.
        public bool isIslandEmpty()
        {
            foreach (Region reg in regions)
                if (reg.getBuildings().Count > 0)
                    return false;

            return true;
        }

        public GameObject getCoast()
        {
            return coast;
        }
        public GameObject getImportDock()
        {
            return importDock;
        }
        public GameObject getExportDock()
        {
            return exportDock;
        }
        public List<Region> getRegions()
        {
            return regions;
        }
        public CartographicIsland getIslandStructure()
        {
            return island;
        }

        public void addRegion(Region reg)
        {
            regions.Add(reg);
        }

        public void setCoast(GameObject c)
        {
            coast = c;
        }
        public void setImportDock(GameObject i)
        {
            importDock = i;
        }
        public void setExportDock(GameObject e)
        {
            exportDock = e;
        }
        public void setIslandStructure(CartographicIsland i)
        {
            island = i;
        }

    }
}
