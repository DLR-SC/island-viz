using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using QuickGraph;
using System.Linq;
using OsgiViz.Relations;
using OsgiViz.Core;
using OsgiViz.Unity.Island;

namespace OsgiViz
{
    public enum DockType
    {
        ImportDock,
        ExportDock
    };

    [RequireComponent(typeof(Valve.VR.InteractionSystem.Interactable))]
    public class DependencyDock : MonoBehaviour
    {
        private GameObject dependencyContainer;
        private ConnectionPool connectionPool;
        private GameObject rotPivot;
        private GameObject importArrowPrefab;
        private GameObject exportArrowPrefab;
        private GameObject arrowHeadPrefab;
        private List<GameObject> connectionArrows;
        private List<DependencyDock> connectedDocks;
        private List<float> dockWeights;
        //If not set, defaults to ImportDock
        private DockType dockType;
        public bool expanded;

        void Awake()
        {
            connectionArrows = new List<GameObject>();
            connectedDocks = new List<DependencyDock>();
            dockWeights = new List<float>();
            dependencyContainer = IslandVizVisualization.Instance.TransformContainer.DependencyContainer.gameObject;

            #region clickable
            InteractableViaClickTouch ict = gameObject.GetComponent<InteractableViaClickTouch>();
            if (ict == null)
                ict = gameObject.AddComponent<InteractableViaClickTouch>();

            ict.handleActivationDeactivation.Add(handleActivationDeactivation);
            #endregion

            expanded = false;
            dockType = DockType.ImportDock;
            importArrowPrefab = (GameObject)Resources.Load("Prefabs/ImportArrow");
            exportArrowPrefab = (GameObject)Resources.Load("Prefabs/ExportArrow");
            arrowHeadPrefab = (GameObject)Resources.Load("Prefabs/ArrowHead");
            rotPivot = new GameObject("Rotation Pivot");
            rotPivot.transform.position = transform.position;
            rotPivot.transform.SetParent(transform);

            ConnectionPool[] pools = FindObjectsOfType(typeof(ConnectionPool)) as ConnectionPool[];
            if (pools.Length == 1)
                connectionPool = pools[0];
            else
                throw new Exception("No connection pool component found, or too many connection pools! There can only be one.");
        }

        public void setDockType(DockType type)
        {
            dockType = type;
        }

        private void handleActivationDeactivation(Hand hand)
        {
            if (expanded)
            {
                HideAllDependencies();
            }
            else
            {
                ShowAllDependencies();
            }
        }

        public void addDockConnection(DependencyDock dock, float w)
        {
            connectedDocks.Add(dock);
            dockWeights.Add(w);
        }



        // ################
        // Connection Arrow Construction
        // ################

        public void constructConnectionArrows()
        {
            
            //Construct new Arrows
            int cc = 0;
            foreach (DependencyDock dock in connectedDocks)
            {

                //Check if Arrow already exists
                IDPair pair = new IDPair(this.GetInstanceID(), dock.GetInstanceID());
                GameObject conArrow = connectionPool.getConnection(pair);
                if (conArrow == null)
                {
                    GameObject arrowBody;
                    if (dockType == DockType.ImportDock)
                        arrowBody = Instantiate(importArrowPrefab, transform.position, Quaternion.identity);
                    else
                        arrowBody = Instantiate(exportArrowPrefab, transform.position, Quaternion.identity);

                    GameObject arrowHead = Instantiate(arrowHeadPrefab, transform.position, Quaternion.identity);
                    conArrow = new GameObject();
                    conArrow.name = "Connection To " + dock.gameObject.name;
                    #region adjust transform
                    Vector3 dirVec = dock.transform.position - transform.position;
                    dirVec.y = 0;
                    float distance = dirVec.magnitude;
                    float sDWidth = gameObject.GetComponent<Collider>().bounds.extents.x;
                    float tDWidth = dock.gameObject.GetComponent<Collider>().bounds.extents.x;;
                    float aWidth = GlobalVar.depArrowWidth * dockWeights[cc];
                    float connectionLength = distance - (sDWidth + tDWidth); 
                    Vector3 newScale = new Vector3(connectionLength, connectionLength, GlobalVar.depArrowWidth * dockWeights[cc]);
                    arrowBody.transform.localScale = newScale;
                    #region Arrowhead
                    
                    newScale.x = GlobalVar.depArrowWidth * dockWeights[cc];
                    newScale.y = 1f;
                    arrowHead.transform.localScale = newScale;
                    if(dockType == DockType.ImportDock)
                    {
                        arrowHead.transform.position += new Vector3(-connectionLength * 0.5f, 0f, 0f);
                        arrowHead.transform.localEulerAngles = new Vector3(0f, 180f, -39f);
                    }
                    else
                    {
                        arrowHead.transform.position += new Vector3(connectionLength * 0.5f, 0f, 0f);
                        arrowHead.transform.localEulerAngles = new Vector3(0f, 0f, -39f);
                    }
                    
                    arrowHead.transform.parent = conArrow.transform;
                    
                    #endregion

                    arrowBody.transform.parent = conArrow.transform;
                    float maxHeight = Mathf.Max(gameObject.GetComponent<Collider>().bounds.extents.y, dock.gameObject.GetComponent<Collider>().bounds.extents.y);
                    conArrow.transform.position += new Vector3((connectionLength / 2f), maxHeight, 0);

                    conArrow.transform.parent = rotPivot.transform;
                    float angle = Vector3.Angle(Vector3.right, dirVec / distance);
                    Vector3 cross = Vector3.Cross(Vector3.right, dirVec / distance);
                    if (cross.y < 0) angle = -angle;
                    rotPivot.transform.Rotate(Vector3.up, angle);
                    conArrow.transform.parent = null;
                    conArrow.transform.parent = dependencyContainer.transform;
                    rotPivot.transform.Rotate(Vector3.up, -angle);
                    #endregion
                    conArrow.SetActive(false);
                    connectionPool.AddConnection(pair, conArrow);
                }
                connectionArrows.Add(conArrow);
                cc++;
            }
        }



        public void HideAllDependencies()
        {
            expanded = false;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(false);
        }

        public void ShowAllDependencies()
        {
            expanded = true;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(true);

            List<Transform> connectedDockTransforms = new List<Transform>();
            foreach (var item in connectedDocks)
            {
                connectedDockTransforms.Add(item.transform.parent);
            }
            connectedDockTransforms.Add(this.transform.parent);
            IslandVizVisualization.Instance.SelectAndFlyTo(connectedDockTransforms.ToArray());
        }

    }
}

