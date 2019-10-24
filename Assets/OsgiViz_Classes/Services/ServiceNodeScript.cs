using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using OsgiViz.Relations;
using OsgiViz.Core;

namespace OsgiViz
{

    [RequireComponent(typeof(Valve.VR.InteractionSystem.Interactable))]
    public class ServiceNodeScript : MonoBehaviour
    {
        //private Transform topmostTransform;
        private GameObject rotPivot;
        private ServiceLayerGO serviceNodeEmitter;
        private List<GameObject> connections;
        //Also have ServiceNodeScript component
        private List<ServiceNodeScript> connectedServiceComponents;


        private GameObject connectionPrefab;
        private ConnectionPool connectionPool;


        void Start()
        {
            connections = new List<GameObject>();
            connectionPrefab = (GameObject)Resources.Load("Prefabs/ServiceConnection");
            connectedServiceComponents = new List<ServiceNodeScript>();

            #region clickable
            InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            if (ict == null)
                ict = gameObject.AddComponent<InteractableViaClickTouch>();

            ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            #endregion

            rotPivot = new GameObject("Rotation Pivot");
            rotPivot.transform.position = transform.position;
            rotPivot.transform.SetParent(transform);

            connectionPool = IslandVizBehaviour.Instance.GetComponent<ConnectionPool>();
        }

        public ServiceLayerGO getEmitterParent()
        {
            return serviceNodeEmitter;
        }

        public void setEmitterParent(ServiceLayerGO parent)
        {
            serviceNodeEmitter = parent;
        }

        public int getActiveConnectionCount()
        {
            int activeConnections = connections.FindAll(x => (x.activeSelf == true)).Count;
            return activeConnections;
        }

        public void expandAll()
        {
            foreach (ServiceNodeScript node in connectedServiceComponents)
            {
                node.enableServiceNode();
                node.getEmitterParent().expandNodes();
            }
            showAllServiceConnections();
        }

        private void handleActivationDeactivation(Hand hand)
        {
            int activeConnections = getActiveConnectionCount();
            if (activeConnections > 0)
            {
                hideAllServiceConnections();
                foreach (ServiceNodeScript node in connectedServiceComponents)
                {
                    List<ServiceNodeScript> allNodesFromEmitter = node.getEmitterParent().getServiceNodes();
                    bool contractEmitter = true;
                    foreach (ServiceNodeScript emitterNode in allNodesFromEmitter)
                    {
                        if (emitterNode.getActiveConnectionCount() > 0)
                            contractEmitter = false;
                    }
                    if (contractEmitter)
                        node.getEmitterParent().contractNodes();
                }
            }
            else
            {
                expandAll();
            }
        }

        public void addConnectedServiceNode(ServiceNodeScript node)
        {
            if (connectedServiceComponents == null)
            {
                connectedServiceComponents = new List<ServiceNodeScript>();
            }
            connectedServiceComponents.Add(node);
        }

        
         
        //Hides all connections going towards connected ServiceNodes as well as
        //the connections going back to this ServiceNode
        public void hideAllServiceConnections()
        {
            if (connections == null)
                return;

            foreach (GameObject connection in connections)
            {
                connection.SetActive(false);
            }
        }


        public void disableServiceNode()
        {
            hideAllServiceConnections();
            gameObject.SetActive(false);
        }

        public void enableServiceNode()
        {
            gameObject.SetActive(true);
        }

        //Shows all connections going towards connected ServiceNodes as well as
        //the connections going back to this ServiceNode
        public void showAllServiceConnections()
        {
            foreach (GameObject connection in connections)
            {
                connection.SetActive(true);
            }
        }

        //Should be called after connections list is finished
        public void constructServiceConnections()
        {
            foreach (ServiceNodeScript node in connectedServiceComponents)
            {
                //Check if Arrow already exists
                IDPair pair = new IDPair(this.GetInstanceID(), node.GetInstanceID());                
                GameObject connectionGO = connectionPool.getConnection(pair);
                if (connectionGO == null)
                {
                    connectionGO = Instantiate(connectionPrefab, transform.position, Quaternion.identity);

                    connectionGO.name = "Connection To " + node.gameObject.name;
                    #region adjust transform
                    Vector3 dirVec = node.transform.position - transform.position;
                    dirVec.y = 0;
                    float distance = dirVec.magnitude;
                    Vector3 newScale = new Vector3(distance, GlobalVar.serviceNodeSize * 0.25f, GlobalVar.serviceNodeSize * 0.25f);
                    connectionGO.transform.localScale = newScale;
                    connectionGO.transform.position += new Vector3(distance / 2f, 0, 0);

                    connectionGO.transform.parent = rotPivot.transform;
                    float angle = Vector3.Angle(Vector3.right, dirVec / distance);
                    Vector3 cross = Vector3.Cross(Vector3.right, dirVec / distance);
                    if (cross.y < 0) angle = -angle;
                    rotPivot.transform.Rotate(Vector3.up, angle);
                    connectionGO.transform.parent = null;
                    connectionGO.transform.parent = transform;
                    rotPivot.transform.Rotate(Vector3.up, -angle);
                    #endregion
                    connectionGO.SetActive(false);
                    connectionPool.AddConnection(pair, connectionGO);
                }
                connections.Add(connectionGO);
                GameObject.Destroy(rotPivot);
            }
        }

    }

}