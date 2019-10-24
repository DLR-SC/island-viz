﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Core;

namespace OsgiViz
{
    [RequireComponent(typeof(Interactable))]
    public class ServiceLayerGO : MonoBehaviour {

        public bool simulateClickWithLongTouch = false;

        public List<ServiceNodeScript> serviceNodes;
        public List<GameObject> downwardConnections;
        private bool expanded = false;

        //private GameObject DownwardConnectionContainer;
        //private GameObject connectionPrefab;

        void Awake()
        {
            serviceNodes = new List<ServiceNodeScript>();
            downwardConnections = new List<GameObject>();
            //connectionPrefab = (GameObject)Resources.Load("Prefabs/ServiceConnection");
                        

            #region clickable
            InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            if (ict == null)
                ict = gameObject.AddComponent<InteractableViaClickTouch>();

            ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            #endregion
        }

        void Start()
        {
            //Create Spieß

        }

        //Should be called after all serviceNodes are added
        public void CreateDownwardConnections(GameObject DownwardConnectionContainer, GameObject connectionPrefab)
        {
            if (serviceNodes.Count > 0)
            {
                //Order ascending... no need as it is implicitly given through construction apparantely
                //serviceNodes.OrderBy(x => x.height);
                for (int i = 0; i < serviceNodes.Count; i++)
                {
                    float length;
                    if (i == 0)
                    {
                        length = serviceNodes[i].transform.parent.gameObject.GetComponent<ServiceSlice>().height - GlobalVar.hologramTableHeight;
                    }
                    else
                    {
                        float sliceHeightPrevious = serviceNodes[i - 1].transform.parent.gameObject.GetComponent<ServiceSlice>().height - GlobalVar.hologramTableHeight;
                        float sliceHeightCurrent = serviceNodes[i].transform.parent.gameObject.GetComponent<ServiceSlice>().height - GlobalVar.hologramTableHeight;
                        length = sliceHeightCurrent - sliceHeightPrevious;
                    }


                    GameObject connectionGO = Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
                    connectionGO.name = "Downward Connection";
                    length = Mathf.Abs(length) / GlobalVar.CurrentZoomLevel;
                    connectionGO.transform.SetParent(DownwardConnectionContainer.transform, false);
                    Vector3 newScale = new Vector3(GlobalVar.serviceNodeSize * 0.03f, length, GlobalVar.serviceNodeSize * 0.03f);
                    Vector3 newPosition = serviceNodes[i].transform.position;
                    newPosition.y = GlobalVar.hologramTableHeight + (length * 0.5f * GlobalVar.CurrentZoomLevel);
                    connectionGO.transform.localScale = newScale;
                    connectionGO.transform.position = newPosition;
                    connectionGO.SetActive(false);
                    downwardConnections.Add(connectionGO);
                }
            }
        }
        
        public void addServiceNode(ServiceNodeScript node)
        {
            serviceNodes.Add(node);
            node.setEmitterParent(this);
        }

        public List<ServiceNodeScript> getServiceNodes()
        {
            return serviceNodes;
        }

        public void expandNodes()
        {
            expanded = true;
            foreach (ServiceNodeScript node in serviceNodes)
                node.enableServiceNode();
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(true);
        }
        /*
        public void expandNodesWithActivation()
        {
            expanded = true;
            foreach (ServiceNodeScript node in serviceNodes)
            {
                node.enableServiceNode();
                node.expandAll();
            }
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(true);
            
        }
        */
        public void contractNodes()
        {
            expanded = false;
            foreach (ServiceNodeScript node in serviceNodes)
            {
                node.disableServiceNode();
            }
            foreach (GameObject downConnection in downwardConnections)
                downConnection.SetActive(false);
        }

        private void handleActivationDeactivation(Hand hand)
        {
            if (!expanded)
                expandNodes();
            else
                contractNodes();
        }
    }
}
