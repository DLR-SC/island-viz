﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
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
       public bool Selected = false;

        public DockType DockType { get; set; }



        private GameObject dependencyContainer;
        private ConnectionPool connectionPool;
        private GameObject rotPivot;
        private List<GameObject> connectionArrows;
        private List<DependencyDock> connectedDocks;
        private List<float> dockWeights;
        //If not set, defaults to ImportDock
        public bool expanded;

        void Awake()
        { /*
            connectionArrows = new List<GameObject>();
            connectedDocks = new List<DependencyDock>();
            dockWeights = new List<float>();
            dependencyContainer = IslandVizVisualization.Instance.TransformContainer.DependencyContainer.gameObject;
            
            expanded = false;
            DockType = DockType.ImportDock;
            rotPivot = new GameObject("Rotation Pivot");
            rotPivot.transform.position = transform.position;
            rotPivot.transform.SetParent(transform);

            ConnectionPool[] pools = FindObjectsOfType(typeof(ConnectionPool)) as ConnectionPool[];
            if (pools.Length == 1)
                connectionPool = pools[0];
            else
                throw new Exception("No connection pool component found, or too many connection pools! There can only be one.");

            // Subscribe to events
            IslandVizInteraction.Instance.OnDockSelect += OnDockSelected;*/
        }


        public void AddDockConnection(DependencyDock dock, float weight)
        {/*
            connectedDocks.Add(dock);
            dockWeights.Add(weight);
            */
        }



        // ################
        // Connection Arrow Construction
        // ################

        public void ConstructConnectionArrows()
        {           /* 
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
                    if (DockType == DockType.ImportDock)
                        arrowBody = Instantiate(IslandVizVisualization.Instance.ImportArrowPrefab, transform.position, Quaternion.identity);
                    else
                        arrowBody = Instantiate(IslandVizVisualization.Instance.ExportArrowPrefab, transform.position, Quaternion.identity);

                    GameObject arrowHead = Instantiate(IslandVizVisualization.Instance.ArrowHeadPrefab, transform.position, Quaternion.identity);
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
                    if(DockType == DockType.ImportDock)
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
            Destroy(rotPivot);*/
        }



        public void HideAllDependencies()
        {/*
            expanded = false;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(false);

            foreach (var item in connectedDocks)
            {
                IslandVizInteraction.Instance.OnIslandSelect(item.transform.parent.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Highlight, false);
            }
            */
        }

        public void ShowAllDependencies()
        {/*
            expanded = true;
            foreach (GameObject arrow in connectionArrows)
                arrow.SetActive(true);

            List<Transform> connectedDockTransforms = new List<Transform>();
            foreach (var item in connectedDocks)
            {
                connectedDockTransforms.Add(item.transform.parent);
                IslandVizInteraction.Instance.OnIslandSelect(item.transform.parent.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Highlight, true);
            }
            connectedDockTransforms.Add(this.transform.parent);
            IslandVizInteraction.Instance.OnIslandSelect(this.transform.parent.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Select, true);

            IslandVizVisualization.Instance.FlyTo(connectedDockTransforms.ToArray());
            */
        }
        

        private void OnDockSelected (DependencyDock dock, IslandVizInteraction.SelectionType selectionType, bool selected)
        {
            /*
            if (Selected && dock != this && selectionType == IslandVizInteraction.SelectionType.Select && selected)
            {
                HideAllDependencies();
                Selected = false;
                IslandVizInteraction.Instance.OnDockSelect(this, IslandVizInteraction.SelectionType.Select, false);
            }
            else if (dock == this && selectionType == IslandVizInteraction.SelectionType.Select && selected)
            {
                if (selected)
                {
                    ShowAllDependencies();
                    Selected = true;
                }
                else
                {
                    HideAllDependencies();
                    Selected = false;
                    IslandVizInteraction.Instance.OnDockSelect(this, IslandVizInteraction.SelectionType.Select, false);
                }
            }
            else if (dock == this && selectionType == IslandVizInteraction.SelectionType.Highlight)
            {
                //if (!selected && Selected)
                //    return;

                IslandVizInteraction.Instance.OnIslandSelect(this.transform.parent.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Highlight, selected);
            }*/
        }
    }
}

