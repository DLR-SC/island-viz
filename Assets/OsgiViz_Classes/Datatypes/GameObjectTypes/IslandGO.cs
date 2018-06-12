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

            #region clickable
            InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            if (ict == null)
                ict = gameObject.AddComponent<InteractableViaClickTouch>();

            ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            #endregion

            #region PdaInspectable
            PdaInspectable pi = gameObject.GetComponent<PdaInspectable>();
            if (pi == null)
                pi = gameObject.AddComponent<PdaInspectable>();
            #endregion

        }

        void Start()
        {
            
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
