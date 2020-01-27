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
    }

    public void SetRegion(Region r)
    {
        regionScript = r;
    }

    public Building UpdateBuilding (Commit c, ZoomLevel currentZoomlevel)
    {
        TimelineStatus tls = compUnit.RelationOfCommitToTimeline(c);
        if (tls != TimelineStatus.present)
        {
            if (buildingGo != null)
            {
                oldBucket = -1;
                Destroy(buildingGo);
            }
            return null;
        }
        else
        {
            CompilationUnit cuCurrent = compUnit.GetElement(c);
            long loc = cuCurrent.getLoc();
            List<object> prefabAndBucket = bpScript.GetBuildingPrefabForLoc(loc);

            int newBucket = (int)prefabAndBucket[1];
            if (newBucket != oldBucket)
            {
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

            if (currentZoomlevel.Equals(ZoomLevel.Near))
            {
                buildingGo.SetActive(true);
            }
            else
            {
                buildingGo.SetActive(false);
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


            return buildingComponent;
            
        }
        
    }  
}
