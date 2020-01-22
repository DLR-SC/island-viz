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
    private int oldBucket;
    private CompUnitMaster compUnit;
    private BuildingProvider_Script bpScript;

    public void SetCompUnit(CompUnitMaster cum)
    {
        compUnit = cum;
        oldBucket = -1;
        bpScript = GameObject.Find("IslandObjectContainer").GetComponent<BuildingProvider_Script>();
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
                buildingGo.layer = LayerMask.NameToLayer("Visualization");
                CapsuleCollider capsuleCol = buildingGo.AddComponent<CapsuleCollider>();

                //Adjust Position of this gameObject (BuildingManager so building is placed on of region)
                float posY;
                if (Constants.timeDepHight)
                {
                    posY = Constants.standardHeight;
                    //TODO fill this (Zeitabhängige Höhe gleich wie bei HexHelber.GetVertices
                    //posY = Constants.standardHeight + 0.5f + Constants.heightFactor * (timedif);
                }
                else
                {
                    posY = Constants.standardHeight;
                }
                Vector3 pos = gameObject.transform.localPosition;
                pos.y = posY;
                gameObject.transform.localPosition = pos;
                

            }
            buildingGo.name = cuCurrent.getName();
            Building buildingComponent = buildingGo.GetComponent<Building>();
            buildingComponent.setCU(cuCurrent);
            cuCurrent.setGameObject(buildingGo);
            return buildingComponent;
            
        }
        
    }  
}
