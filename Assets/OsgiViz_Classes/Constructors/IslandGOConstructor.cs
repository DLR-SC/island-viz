using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuickGraph;
using System.Linq;
using OsgiViz.Core;
using OsgiViz.Relations;
using OsgiViz.Unity.Island;
using OsgiViz.Island;
using OsgiViz.SoftwareArtifact;

using VFace = TriangleNet.Topology.DCEL.Face;
using TnetMesh = TriangleNet.Mesh;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;

namespace OsgiViz.Unity.MainThreadConstructors
{

    public class IslandGOConstructor : MonoBehaviour
    {

        private Status status;
        private List<IslandGO> islandGOs;
        private Material combinedHoloMaterial;
        private GameObject VisualizationContainer;
        private System.Random RNG;

        private GameObject importDockPrefab;
        private GameObject exportDockPrefab;
        private List<GameObject> CUPrefabs;
        private List<GameObject> SIPrefabs;
        private List<GameObject> SDPrefabs;

        // Use this for initialization
        void Start()
        {
            status = Status.Idle;
            islandGOs = new List<IslandGO>();
            combinedHoloMaterial = (Material)Resources.Load("Materials/Combined HoloMaterial");

            #region load prefabs
            importDockPrefab = (GameObject)Resources.Load("Prefabs/Docks/iDock_1");
            exportDockPrefab = (GameObject)Resources.Load("Prefabs/Docks/eDock_1");

            CUPrefabs = Resources.LoadAll<GameObject>("Prefabs/CU/LOD0").ToList<GameObject>();
            SIPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD0").ToList<GameObject>();
            SDPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDeclare/LOD0").ToList<GameObject>();
            if (CUPrefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough CU prefabs!");
            if (SIPrefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SI prefabs!");
            if (SDPrefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SD prefabs!");
            #endregion

            VisualizationContainer = GetComponent<GlobalContainerHolder>().VisualizationContainer;

            RNG = new System.Random(0);
        }


        public IEnumerator Construct(List<CartographicIsland> structures)
        {
            status = Status.Working;
            Debug.Log("Started with Island-GameObject construction!");
            
            for (int i = 0; i < structures.Count; i++)
            {
                GraphVertex vert = structures[i].getDependencyVertex();
                if (vert != null)
                {
                    Vector3 placementPosition = vert.getPosition();
                    placementPosition.y = VisualizationContainer.transform.position.y - GlobalVar.islandHeightProfile[GlobalVar.islandHeightProfile.Length-1];
                    islandGOs.Add(ConstructIslandGO(structures[i], placementPosition));
                    yield return null;
                }
            }

            Debug.Log("Finished with Island-GameObject construction!");
            status = Status.Finished;
        }

        private void setUVsToSingularCoord(Vector2 newUV, MeshFilter mesh)
        {
            Vector2[] uvs = mesh.sharedMesh.uv;
            Vector2[] newUVs = new Vector2[uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
                newUVs[i] = newUV;

            mesh.sharedMesh.uv = newUVs;
        }

        private IslandGO ConstructIslandGO(CartographicIsland island, Vector3 pos)
        {
            int rngSeed = island.getName().GetHashCode() + 200;
            RNG = new System.Random(rngSeed);
            GameObject islandGO = new GameObject(island.getName());
            IslandGO islandGOComponent = islandGO.AddComponent<IslandGO>();
            islandGOComponent.setIslandStructure(island);
            island.setIslandGO(islandGO);

            #region create countries
            List<List<TnetMesh>> tmeshList = island.getPackageMeshes();
            List<List<VFace>> islandCells = island.getPackageCells();
            List<Package> packageList = island.getPackages();

            float maximumBuildingBoundSize = 0;
            int counter = 0;
            foreach (List<TnetMesh> tmesh in tmeshList)
            {
                Package pckg = packageList[counter];

                GameObject region = new GameObject(pckg.getName());

                Region regionComponent = region.AddComponent<Region>();
                regionComponent.setParentIsland(islandGOComponent);
                region.transform.SetParent(islandGO.transform);
                islandGOComponent.addRegion(regionComponent);

                #region RegionArea
                GameObject regionArea = new GameObject("Region area");
                regionArea.transform.SetParent(region.transform);
                MeshFilter mFilter = regionArea.AddComponent<MeshFilter>();
                MeshRenderer mRender = regionArea.AddComponent<MeshRenderer>();
                mRender.sharedMaterial = combinedHoloMaterial;
                
                regionComponent.setRegionArea(regionArea);
                regionComponent.setPackage(pckg);
                #endregion

                List<VFace> packageCells = islandCells[counter];
                CombineInstance[] combineCellMeshes = new CombineInstance[tmesh.Count];
                int cc = 0;
                #region Combine package cell meshes
                foreach (TnetMesh tm in tmesh)
                {
                    Mesh m = Helperfunctions.convertTriangleNETMesh(tm);
                    combineCellMeshes[cc].mesh = m;
                    cc++;
                }
                mFilter.mesh = new Mesh();
                mFilter.mesh.CombineMeshes(combineCellMeshes, true, false);

                float rndU = (float)RNG.NextDouble();
                float rndV = (float)RNG.NextDouble()*0.4f;

                Vector2 rndUV = new Vector2(rndU, rndV);
                setUVsToSingularCoord(rndUV, mFilter);
                #endregion

                cc = 0;
                #region Create CUs
                foreach (CompilationUnit cu in pckg.getCompilationUnits())
                {
                    float x = (float)packageCells[cc].generator.X;
                    float y = (float)packageCells[cc].generator.Z;
                    float z = (float)packageCells[cc].generator.Y;
                    int heightLevel = Helperfunctions.mapLOCtoLevel(cu.getLoc());

                    GameObject building;
                    if (cu.implementsServiceComponent())
                    {
                        building = GameObject.Instantiate(SIPrefabs[heightLevel], region.transform);
                        building.AddComponent<ServiceLayerGO>();
                    }
                    else if (cu.declaresService())
                    {
                        building = GameObject.Instantiate(SDPrefabs[heightLevel], region.transform);
                        building.AddComponent<ServiceLayerGO>();
                    }
                    else
                        building = GameObject.Instantiate(CUPrefabs[heightLevel], region.transform);

                    building.name = cu.getName();
                    Vector3 randomRotation = new Vector3(0f, UnityEngine.Random.Range(-180, 180), 0f);
                    building.transform.localEulerAngles = randomRotation;
                    Building buildingComponent = building.AddComponent<Building>();
                    buildingComponent.setCU(cu);
                    cu.setGameObject(building);
                    building.transform.position = new Vector3(x, y, z);
                    building.transform.localScale = new Vector3(GlobalVar.cuScale, GlobalVar.cuScale, GlobalVar.cuScale);
                    regionComponent.addBuilding(buildingComponent);
                    //////////////////////////
                    #region BuildingCollider
                    building.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                    CapsuleCollider capsuleCol = building.AddComponent<CapsuleCollider>();
                    capsuleCol.isTrigger = true;
                    #endregion
                    float currentBuildingExtent = capsuleCol.bounds.size.magnitude;
                    if (currentBuildingExtent > maximumBuildingBoundSize)
                        maximumBuildingBoundSize = currentBuildingExtent;
                    //////////////////////////
                    cc++;
                }
                #endregion

                counter++;
            }


            #endregion

            #region create coastline
            GameObject coastline = new GameObject("Coastline");
            islandGOComponent.setCoast(coastline);
            coastline.transform.SetParent(islandGO.transform);
            MeshFilter coastMFilter = coastline.AddComponent<MeshFilter>();
            MeshRenderer coastMRender = coastline.AddComponent<MeshRenderer>();
            coastMRender.sharedMaterial = combinedHoloMaterial;
            List<TnetMesh> tmeshCoastList = island.getCoastlineMeshes();
            CombineInstance[] combineCoastInstance = new CombineInstance[tmeshCoastList.Count];
            counter = 0;
            foreach (TnetMesh tmesh in tmeshCoastList)
            {
                Mesh m = Helperfunctions.convertTriangleNETMesh(tmesh);
                combineCoastInstance[counter].mesh = m;
                counter++;
            }
            coastMFilter.mesh = new Mesh();
            coastMFilter.mesh.CombineMeshes(combineCoastInstance, true, false);

            setUVsToSingularCoord(new Vector2(0f, 0.7f), coastMFilter);

            #endregion

            #region init docks
            
            //get graph vertex associated with the island
            GraphVertex vert = island.getDependencyVertex();
            if (vert != null)
            {
                //Relative dock position
                Vector3 dockDirection = new Vector3(UnityEngine.Random.value, 0, UnityEngine.Random.value);
                dockDirection.Normalize();
                dockDirection *= island.getRadius();

                //Import Dock
                Vector3 dockPosition = island.getWeightedCenter() + dockDirection;
                dockPosition.y -= Mathf.Abs(GlobalVar.islandHeightProfile[GlobalVar.islandHeightProfile.Length - 1]) * GlobalVar.islandAboveOcean;
                GameObject importD = Instantiate(importDockPrefab, dockPosition, Quaternion.identity);
                importD.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                importD.name = island.getName() + " import dock";
                importD.transform.localScale = new Vector3(1, 1, 1);
                importD.transform.SetParent(islandGO.transform);
                islandGOComponent.setImportDock(importD);
                //setUVsToSingularCoord(new Vector2(0.7f, 0.1f), importD.GetComponent<MeshFilter>());

                //Export Dock
                GameObject exportD = Instantiate(exportDockPrefab, dockPosition, Quaternion.identity);
                exportD.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                exportD.name = island.getName() + " export dock";
                exportD.transform.localScale = new Vector3(1, 1, 1);
                exportD.transform.SetParent(islandGO.transform);
                islandGOComponent.setExportDock(exportD);
                //setUVsToSingularCoord(new Vector2(0.1f, 0.1f), exportD.GetComponent<MeshFilter>());
            }
            #endregion

            islandGO.transform.position = pos;
            islandGO.transform.SetParent(VisualizationContainer.transform);

            #region rise Islands above ocean
            float newIslandHeight = Mathf.Abs(GlobalVar.islandHeightProfile[GlobalVar.islandHeightProfile.Length - 1]) * GlobalVar.islandAboveOcean;
            Vector3 newIslandPos = islandGO.transform.localPosition;
            newIslandPos.y = newIslandHeight;
            islandGO.transform.localPosition = newIslandPos;
            #endregion

            #region Create colliders

            #region CountryCollider
            List<Region> regions = islandGOComponent.getRegions();
            foreach(Region region in regions)
            {
                GameObject countryGO = region.gameObject;
                countryGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
                MeshCollider cColliderCountry = countryGO.AddComponent<MeshCollider>();
                MeshFilter mFilter = region.getRegionArea().GetComponent<MeshFilter>();

                cColliderCountry.sharedMesh = mFilter.sharedMesh;
                cColliderCountry.convex = true;
                cColliderCountry.isTrigger = true;
            }
            #endregion

            #region IslandCollider
            islandGO.layer = LayerMask.NameToLayer("InteractionSystemLayer");
            CapsuleCollider cColliderIsland = islandGO.AddComponent<CapsuleCollider>();
            float b = island.getRadius();
            cColliderIsland.radius = b;
            float newColliderHeight = islandGOComponent.getCoast().GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            cColliderIsland.height = newColliderHeight;
            Vector3 newCenter = island.getWeightedCenter();
            newCenter.y = -islandGOComponent.getCoast().GetComponent<MeshFilter>().sharedMesh.bounds.size.y + (newColliderHeight * 0.5f);
            cColliderIsland.center = newCenter;
            cColliderIsland.isTrigger = true;
            #endregion

            #endregion

            return islandGOComponent;
        }

        public List<IslandGO> getIslandGOs()
        {
            return islandGOs;
        }

        public Status getStatus()
        {
            return status;
        }

        public void setStatus(Status newStatus)
        {
            status = newStatus;
        }

    }

}