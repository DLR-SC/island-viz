using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using OsgiViz.Unity.Island;
using OsgiViz.SoftwareArtifact;

public class IslandController_Script : MonoBehaviour
{
    public GameObject coastlinePrefab;
    //public GameObject deathareaPrefab;
    public GameObject importDockPrefab;
    public GameObject exportDockPrefab;
    public GameObject regionPrefab;

    private GameObject coastLine;
    //private GameObject deathArea;
    private GameObject exportDock;
    private GameObject importDock;
    private List<GameObject> regions;

    private BundleMaster bundleMaster;

    private System.Random RNG;
    private IslandGO islandGOScript;
    //private IslandObjectContainer_Script mainController;
    //private Commit currentCommit;
    //private bool transformationRunning;


    public void Awake()
    {
        RNG = new System.Random(0);
    }

    public void Start()
    {
        IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;
    }

    public void SetBunldeMaster(BundleMaster bm)
    {
        bundleMaster = bm;
    }
    public BundleMaster GetBundleMaster()
    {
        return bundleMaster;
    }

    public IEnumerator Initialise()
    {
        gameObject.layer = LayerMask.NameToLayer("Visualization");
        islandGOScript = gameObject.GetComponent<IslandGO>();

        regions = new List<GameObject>();

        coastLine = Instantiate(coastlinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        coastLine.name = "Coastline";
        coastLine.transform.parent = gameObject.transform;
        coastLine.transform.localPosition = new Vector3(0, 0, 0);
        coastLine.GetComponent<CoastlineController_Script>().SetGrid(bundleMaster.GetGrid());
        islandGOScript.Coast = coastLine;

        importDock = Instantiate(importDockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        importDock.name = "ImportDock";
        importDock.transform.parent = gameObject.transform;
        importDock.transform.localPosition = new Vector3(0, Constants.dockYPos, 2);
        importDock.layer = LayerMask.NameToLayer("Visualization");
        islandGOScript.ImportDock = importDock;

        exportDock = Instantiate(exportDockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        exportDock.name = "ExportDock";
        exportDock.transform.parent = gameObject.transform;
        exportDock.transform.localPosition = new Vector3(2, Constants.dockYPos, 0);
        exportDock.layer = LayerMask.NameToLayer("Visualization");
        islandGOScript.ExportDock = exportDock;

        foreach (PackageMaster pm in bundleMaster.GetContainedMasterPackages())
        {
            GameObject region = Instantiate(regionPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            region.name = "Region"; //TODO sinnnvoller Name für Region nach Packet
            region.transform.parent = gameObject.transform;
            region.transform.localPosition = new Vector3(0, 0, 0);
            region.layer = LayerMask.NameToLayer("Visualization");
            region.GetComponent<RegionController_Script>().SetPackage(pm);
            region.GetComponent<Region>().setParentIsland(islandGOScript);
            region.GetComponent<RegionController_Script>().InitColor(new Vector2((float)RNG.NextDouble(), (float)RNG.NextDouble()*0.4f));
            region.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;
            //TODO timedephight global regeln
            yield return region.GetComponent<RegionController_Script>().CreateBuildingManagers();
            regions.Add(region);
        }
        yield return null;
    }


    public IEnumerator UpdateRoutine(Commit newCommit, IslandContainerController_Script controllerScript)
    {
        //Update visible Sub-GameObjects
        StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));

        List<Region> activeRegions = new List<Region>();
        float islandHeight = 10;
        foreach (GameObject region in regions)
        {
            TimelineStatus tls = TimelineStatus.defValue;
            Region regionScriptComp = null;
            Vector2 regionHeight = Vector2.zero;
            yield return StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegion(null, newCommit, tls, regionScriptComp, regionHeight));
            if (tls.Equals(TimelineStatus.present))
            {
                activeRegions.Add(regionScriptComp);
            }
            if(islandHeight < regionHeight.x)
            {
                islandHeight = regionHeight.x;
            }
        }

        //Update IslandGO-Script Attributes
        islandGOScript.SetRegions(activeRegions);
        Bundle bundle = bundleMaster.GetElement(newCommit);
        islandGOScript.Bundle = bundle;

        //Get Island Radius
        int maxRingTotal = bundleMaster.GetGrid().GetOuterAssignedTotal(newCommit);
        int maxRingSegment = bundleMaster.GetGrid().GetOuterAssignedFirstTwoSixths(newCommit);

        if (maxRingTotal < 1)
        {
            maxRingTotal = 1;
        }
        if (maxRingSegment < 1)
        {
            maxRingSegment = 1;
        }

        float radiusTotal = Constants.GetRadiusFromRing(maxRingTotal);
        float radiusSegment = Constants.GetRadiusFromRing(maxRingSegment);

        //Reposition IslandDocks
        importDock.transform.localPosition = new Vector3(0f, Constants.dockYPos, radiusSegment + 2);
        exportDock.transform.localPosition = new Vector3(2f, Constants.dockYPos, radiusSegment + 1);

        //Resize Island CapsuleCollider
        CapsuleCollider cc = gameObject.GetComponent<CapsuleCollider>();
        cc.height = islandHeight;
        cc.radius = radiusTotal;
        

        controllerScript.NotifyIslandTransformationFinished();
        yield return null;
    }

    public void OnNewCommit(Commit oldCommit, Commit newCommit)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(UpdateRoutine2(newCommit));
        }
    }

    public IEnumerator UpdateRoutine2(Commit newCommit)
    {
        //TODO Death Area Rausnehmen update regions als Coroutinen probieren
       /* foreach (GameObject region in regions)
        {
            StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegionMesh(newCommit));
            StartCoroutine(region.GetComponent<RegionController_Script>().UpdateBuildings(newCommit));
        }*/
        //StartCoroutine(deathArea.GetComponent<DeathAreaController_Script>().RenewDeathAreaMesh(newCommit));
        //StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));

        int maxRingTotal = bundleMaster.GetGrid().GetOuterAssignedTotal(newCommit);
        int maxRingSegment = bundleMaster.GetGrid().GetOuterAssignedFirstTwoSixths(newCommit);

        if (maxRingTotal < 1)
        {
            maxRingTotal = 1;
        }
        if (maxRingSegment < 1)
        {
            maxRingSegment = 1;
        }

        float radiusTotal = Constants.GetRadiusFromRing(maxRingTotal);
        float radiusSegment = Constants.GetRadiusFromRing(maxRingSegment);

        importDock.transform.localPosition = new Vector3(0f, Constants.dockYPos, radiusSegment + 2);
        exportDock.transform.localPosition = new Vector3(2f, Constants.dockYPos, radiusSegment + 1);


        //controllerScript.NotifyIslandTransformationFinished();
        yield return null;
    }
}



