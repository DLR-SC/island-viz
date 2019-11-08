// #########################
// This class is deprecated!
// #########################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.Unity.MainThreadConstructors;
using OsgiViz.Unity.Island;
using OsgiViz.Core;

namespace OsgiViz
{

    public class SceneController : MonoBehaviour
    {

        private MainThreadConstructor mtc;
        private DockGOConstructor dockConstructor;
        private IslandGOConstructor islandConstructor;
        InverseMultiTouchController viewportController;
        private List<IslandGO> islandList;
        private int currentIDX;
        private GameObject hologramCenter;
        private GameObject dataManager;



        void Start()
        {
            mtc = GetComponent<MainThreadConstructor>();
            dockConstructor = null;
            islandConstructor = null;
            viewportController = null;
            islandList = null;
            hologramCenter = null;
        }

        private void MainConstructorFinished()
        {
            dataManager = GameObject.Find("DataManager");
            dockConstructor = mtc.getDockConstructor();
            islandConstructor = mtc.getIslandGOConstructor();
            viewportController = GameObject.Find("MapNavigationArea").GetComponent<InverseMultiTouchController>();
            RecenterViewport();
            islandList = islandConstructor.getIslandGOs();
            currentIDX = 0;
            hologramCenter = GameObject.Find("HologramCenter");
        }

        void Update()
        {
            //TODO: Refactor into separate object and consider islands radius
            if (islandList != null)
            {
                currentIDX = currentIDX % islandList.Count;
                float absoluteDistanceSquared = (islandList[currentIDX].gameObject.transform.position - hologramCenter.transform.position).sqrMagnitude;
                if (absoluteDistanceSquared > GlobalVar.CurrentZoom * GlobalVar.CurrentZoom)
                    islandList[currentIDX].gameObject.gameObject.SetActive(false);
                else
                    islandList[currentIDX].gameObject.gameObject.SetActive(true);

                currentIDX++;
            }
        }

        //The following functions are accessed from the PDA.
        public IEnumerator ShowAllServices()
        {
            GameObject serviceSliceContainer =  dataManager.GetComponent<GlobalContainerHolder>().ServiceSliceContainer;
            GameObject dConnectionContainer  =  dataManager.GetComponent<GlobalContainerHolder>().DownwardConnectionContainer;

            foreach (Transform child in serviceSliceContainer.transform)
            {
                ServiceSlice currentSlice = child.GetComponent<ServiceSlice>();
                foreach (Transform sliceObj in currentSlice.transform)
                {
                    foreach (Transform sliceObjChild in sliceObj.transform)
                    {
                        if(sliceObjChild.gameObject.tag != "Highlight" && sliceObjChild.gameObject.tag != "TextLabel")
                            sliceObjChild.gameObject.SetActive(true);
                    }
                    if (sliceObj.gameObject.tag != "Highlight" && sliceObj.gameObject.tag != "TextLabel")
                        sliceObj.gameObject.SetActive(true);
                }
                yield return null;
            }

            foreach (Transform child in dConnectionContainer.transform)
            {
                child.gameObject.SetActive(true);
            }

        }

        public IEnumerator HideAllServices()
        {
            GameObject serviceSliceContainer = dataManager.GetComponent<GlobalContainerHolder>().ServiceSliceContainer;
            GameObject dConnectionContainer = dataManager.GetComponent<GlobalContainerHolder>().DownwardConnectionContainer;

            foreach (Transform child in dConnectionContainer.transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (Transform child in serviceSliceContainer.transform)
            {
                ServiceSlice currentSlice = child.GetComponent<ServiceSlice>();
                foreach (Transform sliceObj in currentSlice.transform)
                {
                    foreach (Transform sliceObjChild in sliceObj.transform)
                    {
                        sliceObjChild.gameObject.SetActive(false);
                    }
                    sliceObj.gameObject.SetActive(false);
                }
                yield return null;
            }
        }

        public void ShowAllDependencies()
        {
            if (dockConstructor == null)
            {
                Debug.Log("DockConstructor is null!");
                return;
            }
            else
            {
                foreach (GameObject dockGO in dockConstructor.getDocks())
                {
                    DependencyDock dockComponent = dockGO.GetComponent<DependencyDock>();
                    dockComponent.showAllDependencies();
                }
            }
        }

        public void HideAllDependencies()
        {
            if (dockConstructor == null)
            {
                Debug.Log("DockConstructor is null!");
                return;
            }
            else
            {
                foreach (GameObject dockGO in dockConstructor.getDocks())
                {
                    DependencyDock dockComponent = dockGO.GetComponent<DependencyDock>();
                    dockComponent.hideAllDependencies();
                }
            }
        }

        // Compute the center of the holographic world + the maximal radius needed to cover all islands from this center.
        // And stores them in the GlobalVariables, needed to later constrain the users movement and scaling.
        // With this information, it recenters the holographic projection.
        public void RecenterViewport()
        {
            if (!GlobalVar.recenterValid)
            {
                List<IslandGO> islands = islandConstructor.getIslandGOs();
                GameObject hologramCenter = GameObject.Find("HologramCenter");

                float maximalDistance = 0;
                Vector3 center = Vector3.zero;
                foreach (IslandGO island in islands)
                {
                    float distance = Vector3.Distance(island.gameObject.transform.position, hologramCenter.transform.position);
                    center += island.gameObject.transform.position;

                    if (distance > maximalDistance)
                        maximalDistance = distance;
                }

                GlobalVar.worldRadius = maximalDistance;
                float normFactor = 1.0f / islands.Count;
                center = center * normFactor;
                GlobalVar.worldCenter = center;
                Vector3 newPos = center;
                newPos.y = 0;

                viewportController.Reposition(newPos);
                newPos.y = GlobalVar.hologramTableHeight;
                viewportController.RotateAndScale(newPos, 0f, maximalDistance * 1.0f);
                GlobalVar.recenterValid = true;
            }
            else
            {
                GameObject transformCandidate = GameObject.Find("RealWorld");

                Vector3 newPos = GlobalVar.worldCenter;
                newPos.y = GlobalVar.hologramTableHeight;

                float scale = GlobalVar.worldRadius / transformCandidate.transform.localScale.x;
                viewportController.RotateAndScale(newPos, -transformCandidate.transform.localRotation.y, scale);
                GameObject.Find("RealWorld").transform.localRotation = Quaternion.identity;

                newPos.y = -GlobalVar.hologramTableHeight * GlobalVar.worldRadius;
                viewportController.Reposition(newPos);
            }
        }

    }
}
