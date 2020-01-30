using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ArchitectureElements;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using Assets;
using OsgiViz.Unity.Island;
using OsgiViz.SoftwareArtifact;

public class BuildingController_Script : MonoBehaviour
{
    private GameObject buildingGo;
    private Region regionScript;
    private int oldBucket;
    private CompUnitMaster compUnit;
    private BuildingProvider_Script bpScript;

    public void SetCompUnit(CompUnitMaster cum)
    {
        compUnit = cum;
        oldBucket = -1;
        bpScript = GameObject.Find("IslandObjectContainer").GetComponent<BuildingProvider_Script>();
        IslandVizInteraction.Instance.OnHistoryHighlightChanged += ChangeHighlight;

    }

    public void SetRegion(Region r)
    {
        regionScript = r;
    }

    public Building UpdateBuilding (Commit c)
    {
        TimelineStatus tls = compUnit.RelationOfCommitToTimeline(c);
        if (tls != TimelineStatus.present)
        {
            if (buildingGo != null)
            {
                oldBucket = -1;
                Destroy(buildingGo);
                CallIslandStructureChange();
            }
            return null;
        }
        else
        {
            ChangeStatus cs = ChangeStatus.unknown;
            CompilationUnit cuCurrent = compUnit.GetElement(c);
            long loc = cuCurrent.getLoc();
            List<object> prefabAndBucket = bpScript.GetBuildingPrefabForLoc(loc);

            int newBucket = (int)prefabAndBucket[1];
            if (newBucket != oldBucket)
            {
                if (oldBucket == -1)
                    cs = ChangeStatus.newElement;
                else
                    cs = ChangeStatus.changedElement;

                oldBucket = newBucket;
                if (buildingGo != null)
                {
                    Destroy(buildingGo);
                }
                GameObject prefab = (GameObject)prefabAndBucket[0];
                buildingGo = Instantiate(prefab, gameObject.transform, false);
                buildingGo.transform.localPosition = Vector3.zero;
                buildingGo.transform.localRotation = Quaternion.identity;
                buildingGo.transform.localScale = new Vector3(1, 1, 1);
                buildingGo.AddComponent<Building>();
                buildingGo.GetComponent<Building>().SetParentRegion(regionScript);


                buildingGo.layer = LayerMask.NameToLayer("Visualization");
                CapsuleCollider capsuleCol = buildingGo.AddComponent<CapsuleCollider>();          

            }



            if (cs.Equals(ChangeStatus.unknown))
            {
                RemoveChangeIndicator();
            }else if (cs.Equals(ChangeStatus.newElement))
            {
                AddChangeIndicatorNew();
            }else if (cs.Equals(ChangeStatus.changedElement))
            {
                AddChangeIndicatorChange();
            }

            if (tls.Equals(TimelineStatus.notYetPresent))
                buildingGo.name = "Future Building";
            else if (tls.Equals(TimelineStatus.present))
                buildingGo.name = cuCurrent.getName();
            else if (tls.Equals(TimelineStatus.notPresentAnymore))
                buildingGo.name = "Deleted Building";
            else
                buildingGo.name = "Unknown Building";

            Building buildingComponent = buildingGo.GetComponent<Building>();
            buildingComponent.setCU(cuCurrent);
            cuCurrent.setGameObject(buildingGo);

            //Adjust Height if HeightDisplay
            int heightDif = 0;
            if (HistoryNavigation.Instance.showTimeDependentHight)
            {
                    heightDif = c.GetCommitIndex() - compUnit.GetStart(SortTypes.byTime).GetCommitIndex();
            }
            float heigth = Constants.standardHeight + Constants.heightFactor * heightDif;
            Vector3 pos = gameObject.transform.localPosition;
            pos.y = heigth;
            gameObject.transform.localPosition = pos;

            if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Near))
            {
                buildingGo.SetActive(true);
                if (!HistoryNavigation.Instance.historyHighlightActive)
                {
                    if (buildingGo.transform.Find("ChangeIndicator") != null)
                    {
                        buildingGo.transform.Find("ChangeIndicator").gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                buildingGo.SetActive(false);
            }


            return buildingComponent;
            
        }
        
    }

    public void RemoveChangeIndicator()
    {
        if (buildingGo != null)
        {
            Transform ciT = buildingGo.transform.Find("ChangeIndicator");
            if (ciT != null)
            {
                GameObject.Destroy(ciT.gameObject);
            }
        }
    }
    public void AddChangeIndicatorChange()
    {
        GameObject changeIdicator = AddChangeIndicator();
        MeshFilter mf = changeIdicator.GetComponent<MeshFilter>();
        GameobjectHelperClass.setUVsToSingularCoord(Constants.colValChangeHighlight, mf);
    }
    public void AddChangeIndicatorNew()
    {
        GameObject changeIdicator = AddChangeIndicator();
        MeshFilter mf = changeIdicator.GetComponent<MeshFilter>();
        GameobjectHelperClass.setUVsToSingularCoord(Constants.colValNewHighlight, mf);
        CallIslandStructureChange();
    }

    public GameObject AddChangeIndicator()
    {
        if (buildingGo != null)
        {
            Transform ciT = buildingGo.transform.Find("ChangeIndicator");
            if (ciT != null)
            {
                return ciT.gameObject;
            }
            GameObject changeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            changeIndicator.transform.parent = buildingGo.transform;
            changeIndicator.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            changeIndicator.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            changeIndicator.name = "ChangeIndicator";
            changeIndicator.GetComponent<CapsuleCollider>().enabled = false;
            changeIndicator.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;
            return changeIndicator;
        }
        return null;

    }

    public void CallIslandStructureChange()
    {
        regionScript.getParentIsland().gameObject.GetComponent<IslandController_Script>().SubstructureChange();
    }


    public void ChangeHighlight(bool enabled)
    {
        if (!gameObject.activeSelf || buildingGo == null || buildingGo.transform.Find("ChangeIndicator") == null)
        {
            return;
        }
        if (!enabled)
        {
            buildingGo.transform.Find("ChangeIndicator").gameObject.SetActive(false);
        }
        else
        {
            if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Near))
            {
                buildingGo.transform.Find("ChangeIndicator").gameObject.SetActive(true);
            }
        }
    }

}
