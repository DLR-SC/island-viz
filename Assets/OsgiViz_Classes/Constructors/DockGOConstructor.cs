using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
using System.Linq;
using OsgiViz.Core;
using OsgiViz.Relations;
using OsgiViz.Unity.Island;
using OsgiViz.Island;

namespace OsgiViz.Unity.MainThreadConstructors
{
    public class DockGOConstructor : MonoBehaviour
    {

        private Status status;
        private GameObject VisualizationContainer;
        private List<GameObject> dockList;

        private void Awake()
        {

        }

        void Start()
        {
            status = Status.Idle;
            dockList = new List<GameObject>();
            VisualizationContainer = GetComponent<GlobalContainerHolder>().VisualizationContainer;
        }

        public IEnumerator Construct(List<IslandGO> islands)
        {
            status = Status.Working;
            Debug.Log("Started with Dock-GameObject construction!");
            yield return StartCoroutine(constructAll(islands));
        }

        IEnumerator constructAll(List<IslandGO> islands)
        {
            for (int i = 0; i < islands.Count; i++)
            {
                constructDockGO(islands[i]);
                yield return null;
            }

            for (int i = 0; i < islands.Count; i++)
            {
                IslandGO islandGOComponent = islands[i].GetComponent<IslandGO>();
                GameObject eDock = islandGOComponent.getExportDock();
                GameObject iDock = islandGOComponent.getImportDock();
                if (eDock != null)
                {
                    dockList.Add(eDock);
                    eDock.GetComponent<DependencyDock>().constructConnectionArrows();
                }
                if (iDock != null)
                {
                    dockList.Add(iDock);
                    iDock.GetComponent<DependencyDock>().constructConnectionArrows();
                }
                yield return null;
            }

            Debug.Log("Finished with Dock-GameObject construction!");
            status = Status.Finished;
        }

        
        private bool findSuitablePosition2D(GameObject obj, List<GameObject> doNotCollideWith, GameObject placeNearThis, int iterations)
        {
            bool result = false;
            List<GameObject> objsWithMeshCollider = new List<GameObject>();
            int calculationLayermask = 1 << LayerMask.NameToLayer("CalculationOnly");

            #region clone doNotCollideWith objects and give them a mesh collider
            foreach (GameObject go in doNotCollideWith)
                objsWithMeshCollider.Add(GameObject.Instantiate(go, go.transform.parent));
            foreach (GameObject go in objsWithMeshCollider)
            {
                GameObject.Destroy(go.GetComponent<Collider>());
                MeshCollider mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
                mc.convex = true;
                go.layer = LayerMask.NameToLayer("CalculationOnly");
            }
            #endregion


            Vector3 originalPosition = obj.transform.position;
            Collider objCollider = obj.GetComponent<Collider>();
            Collider nearThisCollider = placeNearThis.GetComponent<Collider>();
            float placeDistance = objCollider.bounds.extents.magnitude + nearThisCollider.bounds.extents.magnitude;
            for (int i = 0; i < iterations; i++)
            {
                Vector3 dockDirection = new Vector3(UnityEngine.Random.value, 0, UnityEngine.Random.value);
                dockDirection.Normalize();
                dockDirection *= placeDistance;
                Vector3 newPossiblePosition = placeNearThis.transform.position + dockDirection;

                bool intersects = Physics.CheckSphere(newPossiblePosition, placeDistance, calculationLayermask);
                if (!intersects)
                {
                    obj.transform.position = newPossiblePosition;
                    result = true;
                    break;
                }

            }

            #region cleanup
            foreach (GameObject go in objsWithMeshCollider)
                GameObject.Destroy(go);
            #endregion

            return result;
        }
        

        private void constructDockGO(IslandGO island)
        {
            CartographicIsland islandStructure = island.getIslandStructure();

            //Get graph vertex associated with the island
            BidirectionalGraph<GraphVertex, GraphEdge> depGraph = islandStructure.getBundle().getParentProject().getDependencyGraph();

            GraphVertex vert = islandStructure.getDependencyVertex();
            if (vert != null)
            {

                float importSize = GlobalVar.minDockSize;
                float exportSize = GlobalVar.minDockSize;

                //Outgoing edges -Bundle depends on...
                IEnumerable<GraphEdge> outEdges;
                depGraph.TryGetOutEdges(vert, out outEdges);
                List<GraphEdge> edgeList = outEdges.ToList();
                importSize = Helperfunctions.mapDependencycountToSize(edgeList.Count);
                //Import Dock
                GameObject importD = island.getImportDock();
                importD.transform.localScale = new Vector3(importSize, importSize, importSize);
                //Link dependencies
                DependencyDock dockComponent = importD.GetComponent<DependencyDock>();
                dockComponent.setDockType(DockType.ImportDock);
                foreach(GraphEdge e in edgeList)
                {
                    GameObject ed = e.Target.getIsland().getIslandGO().GetComponent<IslandGO>().getExportDock();
                    dockComponent.addDockConnection(ed.GetComponent<DependencyDock>(), e.getWeight());
                }
                
                #region determine optimal Position for ImportDock
                List<GameObject> doNotCollideList = new List<GameObject>();
                doNotCollideList.Add(island.getCoast());
                bool foundLocation = findSuitablePosition2D(importD, doNotCollideList, island.gameObject, 500);
                if(!foundLocation)
                    Debug.LogWarning("Could not find suitable location for " + importD.name);
                #endregion
                


                //Ingoing edges -Other Bundles depends on this one...
                depGraph.TryGetInEdges(vert, out outEdges);
                edgeList = outEdges.ToList();
                exportSize = Helperfunctions.mapDependencycountToSize(edgeList.Count);
                //Export Dock
                GameObject exportD = island.getExportDock();
                float eDockWidth = exportD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * exportSize;
                float iDockWidth = importD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * importSize;
                //exportD.transform.position = importD.transform.position + Vector3.left * (iDockWidth + eDockWidth) * 0.5f;                
                exportD.transform.localScale = new Vector3(exportSize, exportSize, exportSize);
                //Link dependencies
                dockComponent = exportD.GetComponent<DependencyDock>();
                dockComponent.setDockType(DockType.ExportDock);
                foreach (GraphEdge e in edgeList)
                {
                    GameObject id = e.Source.getIsland().getIslandGO().GetComponent<IslandGO>().getImportDock();
                    dockComponent.addDockConnection(id.GetComponent<DependencyDock>(), e.getWeight());
                }
                
                #region determine optimal Position for ExportDock
                doNotCollideList.Clear();
                doNotCollideList.Add(island.getCoast());
                foundLocation = findSuitablePosition2D(exportD, doNotCollideList, importD, 500);
                if (!foundLocation)
                    Debug.Log("Could not find suitable location for " + exportD.name);
                #endregion
                
                 
                #region extend Island collider based on new Docksizes
                island.GetComponent<CapsuleCollider>().radius += Mathf.Max(importSize, exportSize) * Mathf.Sqrt(2f);
                #endregion

            }
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }

        public List<GameObject> getDocks()
        {
            return dockList;
        }
    }
}