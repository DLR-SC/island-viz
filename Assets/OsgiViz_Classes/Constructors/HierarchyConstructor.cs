using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OsgiViz.Core;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Unity.Island;


namespace OsgiViz.Unity.MainThreadConstructors
{
    public class HierarchyConstructor : MonoBehaviour {

        private Status status;

        private Material holomaterial;
        private List<GameObject> CULod1Prefabs;
        private List<GameObject> SILod1Prefabs;
        private List<GameObject> SDLod1Prefabs;

        private List<GameObject> CULod2Prefabs;
        private List<GameObject> SILod2Prefabs;
        private List<GameObject> SDLod2Prefabs;

        private GameObject eDockLod1Prefab;
        private GameObject iDockLod1Prefab;


        private void Awake()
        {
            holomaterial = Resources.Load<Material>("Materials/Combined HoloMaterial");

            #region load prefabs
            CULod1Prefabs = Resources.LoadAll<GameObject>("Prefabs/CU/LOD1").ToList<GameObject>();
            SILod1Prefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD1").ToList<GameObject>();
            SDLod1Prefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDeclare/LOD1").ToList<GameObject>();
            if (CULod1Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough CU_LOD1 prefabs!");
            if (SILod1Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SI_LOD1 prefabs!");
            if (SDLod1Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SD_LOD1 prefabs!");

            CULod2Prefabs = Resources.LoadAll<GameObject>("Prefabs/CU/LOD2").ToList<GameObject>();
            SILod2Prefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD2").ToList<GameObject>();
            SDLod2Prefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDeclare/LOD2").ToList<GameObject>();
            if (CULod2Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough CU_LOD2 prefabs!");
            if (SILod2Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SI_LOD2 prefabs!");
            if (SDLod2Prefabs.Count < GlobalVar.numLocLevels)
                throw new Exception("For the selected number of discreet LOC levels, there are not enough SD_LOD2 prefabs!");

            eDockLod1Prefab = Resources.Load<GameObject>("Prefabs/Docks/LOD1/eDock_1_LOD1");
            iDockLod1Prefab = Resources.Load<GameObject>("Prefabs/Docks/LOD1/iDock_1_LOD1");
            #endregion
        }

        // Use this for initialization
        void Start () {
            status = Status.Idle;

        }


        public IEnumerator Construct(List<IslandGO> islands)
        {
            status = Status.Working;
            Debug.Log("Started with Island-Hierarchy injection!");
            yield return StartCoroutine(constructAll(islands));
        }

        private void Add_HC_TLC(GameObject go, float sdq)
        {
            HierarchicalComponent goHC = go.AddComponent<HierarchicalComponent>();
            goHC.subdivisionDistanceSquared = sdq;
            TextLabelComponent goTLC = go.AddComponent<TextLabelComponent>();
        }

        IEnumerator constructAll(List<IslandGO> islandGOs)
        {
            for (int i = 0; i < islandGOs.Count; i++)
            {
                Add_HC_TLC(islandGOs[i].gameObject, GlobalVar.subdivisionDistanceIslandSquared);
                HierarchicalComponent islandHC = islandGOs[i].gameObject.GetComponent<HierarchicalComponent>();
                TextLabelComponent islandTLC = islandGOs[i].gameObject.GetComponent<TextLabelComponent>();
                islandTLC.relativeHeight = 0.3f;

                List<Region> regions = islandGOs[i].getRegions();
                for (int c = 0; c < regions.Count; c++)
                {
                    Add_HC_TLC(regions[c].gameObject, GlobalVar.subdivisionDistanceCountrySquared);
                    HierarchicalComponent regionHC = regions[c].gameObject.GetComponent<HierarchicalComponent>();
                    TextLabelComponent regionTLC = regions[c].gameObject.GetComponent<TextLabelComponent>();
                    regionTLC.relativeHeight = 0.2f;
                    regionTLC.relativeScale = 0.75f;

                    regionHC.parentComponent = islandHC;
                    islandHC.childrenComponents.Add(regionHC);

                    List<Building> cus = regions[c].getBuildings();
                    for (int b = 0; b < cus.Count; b++)
                    {
                        Add_HC_TLC(cus[b].gameObject, GlobalVar.subdivisionDistanceCUSquared);
                        HierarchicalComponent buildingHC = cus[b].gameObject.GetComponent<HierarchicalComponent>();
                        TextLabelComponent buildingTLC = cus[b].gameObject.GetComponent<TextLabelComponent>();
                        buildingTLC.relativeHeight = 0.1f;
                        buildingTLC.relativeScale = 0.5f;

                        buildingHC.parentComponent = regionHC;
                        regionHC.childrenComponents.Add(buildingHC);
                    }

                    #region RegionArea // TODO
                    //HierarchicalComponent regionAreaHC = regions[c].getRegionMesh().AddComponent<HierarchicalComponent>();
                    //regionAreaHC.parentComponent = regionHC;
                    //regionHC.childrenComponents.Add(regionAreaHC);
                    #endregion


                    bakeRegionMesh(regions[c]);
                }

                #region ExportDock
                Add_HC_TLC(islandGOs[i].getExportDock(), GlobalVar.subdivisionDistanceCUSquared);
                HierarchicalComponent exportDockHC = islandGOs[i].getExportDock().GetComponent<HierarchicalComponent>();
                exportDockHC.parentComponent = islandHC;
                islandHC.childrenComponents.Add(exportDockHC);
                #endregion
                #region ImportDock
                Add_HC_TLC(islandGOs[i].getImportDock(), GlobalVar.subdivisionDistanceCUSquared);
                HierarchicalComponent importDockHC = islandGOs[i].getImportDock().GetComponent<HierarchicalComponent>();
                importDockHC.parentComponent = islandHC;
                islandHC.childrenComponents.Add(importDockHC);
                #endregion
                #region Coast
                HierarchicalComponent coastHC = islandGOs[i].getCoast().AddComponent<HierarchicalComponent>();
                coastHC.parentComponent = islandHC;
                coastHC.subdivisionDistanceSquared = GlobalVar.subdivisionDistanceCUSquared;
                islandHC.childrenComponents.Add(coastHC);
                #endregion


                bakeIslandMesh(islandGOs[i]);
                Deactivate(islandGOs[i]);

                yield return null;
            }

            Debug.Log("Finished with Island-Hierarchy injection!");
            status = Status.Finished;
        }

        private void Deactivate(IslandGO islandGO)
        {
            List<Region> regions = islandGO.getRegions();
            for (int c = 0; c < regions.Count; c++)
            {
                List<Building> building = regions[c].getBuildings();
                for (int b = 0; b < building.Count; b++)
                {
                    building[b].gameObject.SetActive(false);   
                }
                regions[c].gameObject.SetActive(false);
            }
            islandGO.getExportDock().SetActive(false);
            islandGO.getImportDock().SetActive(false);
            islandGO.getCoast().SetActive(false);
        }

        private void bakeRegionMesh(Region reg)
        {
            GameObject regionGO = reg.gameObject;
            List<Building> buildings = reg.getBuildings();

            #region Add CUs to CombineList

            List<CombineInstance> currentCiList = new List<CombineInstance>();
            foreach (Building building in buildings)
            {
                CombineInstance ci = new CombineInstance();

                CompilationUnit cu = building.GetComponent<Building>().getCU();
                int modelIdx = Helperfunctions.mapLOCtoLevel(cu.getLoc());

                if (cu.implementsServiceComponent())
                    ci.mesh = SILod1Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;
                else if(cu.declaresService())
                    ci.mesh = SDLod1Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;
                else
                    ci.mesh = CULod1Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;

                ci.subMeshIndex = 0;                    
                ci.transform = regionGO.transform.worldToLocalMatrix * building.transform.localToWorldMatrix;
                currentCiList.Add(ci);
            }
            #endregion
            #region Add country area to FinalCombineList
            CombineInstance ciCountry = new CombineInstance();
            ciCountry.mesh = reg.getRegionMesh().sharedMesh;
            ciCountry.subMeshIndex = 0;
            ciCountry.transform = Matrix4x4.identity;
            currentCiList.Add(ciCountry);
            #endregion

            #region Replace Country Mesh and Materials with baked one
            MeshRenderer bakedMR = regionGO.AddComponent<MeshRenderer>();
            bakedMR.sharedMaterial = holomaterial;
            MeshFilter bakedMF = regionGO.AddComponent<MeshFilter>();
            bakedMF.mesh = new Mesh();
            bakedMF.sharedMesh.CombineMeshes(currentCiList.ToArray(), true, true);
            #endregion

        }

        private void bakeIslandMesh(IslandGO islandGOComponent)
        {
            GameObject islandGO = islandGOComponent.gameObject;
            List<CombineInstance> currentCiList = new List<CombineInstance>();

            foreach (Region region in islandGOComponent.getRegions())
            {
                foreach (Building b in region.getBuildings())
                {
                    CombineInstance ci = new CombineInstance();
                    CompilationUnit cu = b.getCU();
                    int modelIdx = Helperfunctions.mapLOCtoLevel(cu.getLoc());

                    if (cu.implementsServiceComponent())
                        ci.mesh = SILod2Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;
                    else if (cu.declaresService())
                        ci.mesh = SDLod2Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;
                    else
                        ci.mesh = CULod2Prefabs[modelIdx].GetComponent<MeshFilter>().sharedMesh;

                    ci.subMeshIndex = 0;
                    ci.transform = islandGO.transform.worldToLocalMatrix * b.gameObject.transform.localToWorldMatrix;
                    currentCiList.Add(ci);
                }

                CombineInstance ciCountry = new CombineInstance();
                ciCountry.mesh = region.getRegionMesh().sharedMesh;
                ciCountry.subMeshIndex = 0;
                ciCountry.transform = Matrix4x4.identity;
                currentCiList.Add(ciCountry);
            }

            #region Add coast to FinalCombineList
            CombineInstance ciCoast = new CombineInstance();
            ciCoast.mesh = islandGOComponent.getCoast().GetComponent<MeshFilter>().sharedMesh;
            ciCoast.subMeshIndex = 0;
            ciCoast.transform = Matrix4x4.identity;
            currentCiList.Add(ciCoast);
            #endregion

            #region Add docks to FinalCombineList
            GameObject expDock = islandGOComponent.getExportDock();
            GameObject impDock = islandGOComponent.getImportDock();

            CombineInstance eDockCI = new CombineInstance();
            eDockCI.mesh = eDockLod1Prefab.GetComponent<MeshFilter>().sharedMesh;
            eDockCI.subMeshIndex = 0;
            eDockCI.transform = islandGOComponent.transform.worldToLocalMatrix * expDock.transform.localToWorldMatrix;

            CombineInstance iDockCI = new CombineInstance();
            iDockCI.mesh = iDockLod1Prefab.GetComponent<MeshFilter>().sharedMesh;
            iDockCI.subMeshIndex = 0;
            iDockCI.transform = islandGOComponent.transform.worldToLocalMatrix * impDock.transform.localToWorldMatrix;

            currentCiList.Add(eDockCI);
            currentCiList.Add(iDockCI);            
            #endregion

            #region Replace Island Mesh and Materials with baked one
            MeshRenderer bakedMR = islandGO.AddComponent<MeshRenderer>();
            bakedMR.sharedMaterial = holomaterial;
            MeshFilter bakedMF = islandGO.AddComponent<MeshFilter>();
            bakedMF.mesh = new Mesh();
            bakedMF.sharedMesh.CombineMeshes(currentCiList.ToArray(), true, true);

            #endregion

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
