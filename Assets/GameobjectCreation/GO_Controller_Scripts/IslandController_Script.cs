using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using OsgiViz.Unity.Island;
using OsgiViz.SoftwareArtifact;
using OsgiViz;

public class IslandController_Script : MonoBehaviour
{
    public GameObject coastlinePrefab;
    public GameObject importDockPrefab;
    public GameObject exportDockPrefab;
    public GameObject regionPrefab;

    private GameObject coastLine;
    public GameObject exportDock { get; set; }
    public GameObject importDock { get; set; }
    private List<GameObject> regions;
    private GameObject changeIndikator;

    private BundleMaster bundleMaster;

    private System.Random RNG;
    private IslandGO islandGOScript;
    private ChangeStatus changeStatus;
    //private IslandObjectContainer_Script mainController;
    //private Commit currentCommit;
    //private bool transformationRunning;


    public void Awake()
    {
        RNG = new System.Random(0);
    }

    public void Start()
    {
        //IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;
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
        changeIndikator = AddChangeIndicator();

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
        importDock.GetComponent<DependencyDock>().DockType = DockType.ImportDock;
        islandGOScript.ImportDock = importDock;

        exportDock = Instantiate(exportDockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        exportDock.name = "ExportDock";
        exportDock.transform.parent = gameObject.transform;
        exportDock.transform.localPosition = new Vector3(2, Constants.dockYPos, 0);
        exportDock.layer = LayerMask.NameToLayer("Visualization");
        exportDock.GetComponent<DependencyDock>().DockType = DockType.ExportDock;
        islandGOScript.ExportDock = exportDock;

        int i = 0;

        foreach (PackageMaster pm in bundleMaster.GetContainedMasterPackages())
        {
            GameObject region = Instantiate(regionPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            region.name = "Region"; //TODO sinnnvoller Name für Region nach Packet
            region.transform.parent = gameObject.transform;
            region.transform.localPosition = new Vector3(0, 0, 0);
            region.layer = LayerMask.NameToLayer("Visualization");
            region.GetComponent<RegionController_Script>().SetPackage(pm);
            region.GetComponent<RegionController_Script>().SetParentIsland(gameObject);
            region.GetComponent<Region>().setParentIsland(islandGOScript);
            region.GetComponent<RegionController_Script>().InitColor(Constants.colVals[i]);
            region.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;
            //TODO timedephight global regeln
            yield return region.GetComponent<RegionController_Script>().CreateBuildingManagers();
            regions.Add(region);
            i = (i+1)%8;

        }
        yield return null;
    }

    public GameObject AddChangeIndicator()
    {
            GameObject changeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            changeIndicator.transform.parent = gameObject.transform;
            changeIndicator.transform.localPosition = new Vector3(0f, Constants.standardHeight/6f, 0f);
            changeIndicator.transform.localScale = new Vector3(5f, Constants.standardHeight/3f, 5f);
            changeIndicator.name = "ChangeIndicator";
            changeIndicator.GetComponent<CapsuleCollider>().enabled = false;
            changeIndicator.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;
            return changeIndicator;
    }


    public IEnumerator UpdateRoutine(Commit newCommit, IslandContainerController_Script controllerScript, bool justActivated)
    {
        changeStatus = ChangeStatus.unknown;
        if (justActivated)
        {
            changeStatus = ChangeStatus.newElement;
            SetChangeIndicator(changeStatus);
        }
        else
        {
            changeIndikator.SetActive(false);
        }
        ZoomLevel currentZoomLevel = islandGOScript.CurrentZoomLevel;
        islandGOScript.ResetRegions();

        StartCoroutine(UpdateExportDock(newCommit, currentZoomLevel));
        StartCoroutine(UpdateImportDock(newCommit, currentZoomLevel));

        //Update visible Sub-GameObjects
        StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));

        List<Region> activeRegions = new List<Region>();
        float islandHeight = 10;
        foreach (GameObject region in regions)
        {
            StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegion(null, newCommit, currentZoomLevel, (returnScript)=> { if (returnScript != null) { activeRegions.Add(returnScript); } } ));
        }

        //Update IslandGO-Script Attributes
        islandGOScript.SetRegions(activeRegions);
        Bundle bundle = bundleMaster.GetElement(newCommit);
        islandGOScript.Bundle = bundle;
        gameObject.name = bundle.getName();

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

        //Resize Island Collider
        SphereCollider cc = gameObject.GetComponent<SphereCollider>();
        cc.radius = radiusTotal;
        if (currentZoomLevel.Equals(ZoomLevel.Far))
        {
            cc.enabled = true;
        }
        else
        {
            cc.enabled = false;
        }
        //resize ChangeIndicator
        changeIndikator.transform.localScale = new Vector3(radiusTotal, Constants.standardHeight / 3f, radiusTotal);

        islandGOScript.SetRegions(activeRegions);

        controllerScript.NotifyIslandTransformationFinished();
        yield return null;
    }

    public IEnumerator UpdateImportDock(Commit newCommit, ZoomLevel currentZoomlevel)
    {
        importDock.SetActive(true);
        DependencyDock iDock = importDock.GetComponent<DependencyDock>();
        iDock.ResetDependencies(null, null);

        Bundle bundle = bundleMaster.GetElement(newCommit);
        foreach (KeyValuePair<Bundle, float> importBundle in bundle.GetImportedBundles())
        {
            iDock.AddDockConnection(importBundle.Key.GetMaster().islandController.exportDock.GetComponent<DependencyDock>(), importBundle.Value);
        }

        yield return null;
        iDock.ConstructConnectionArrows();
        if (currentZoomlevel.Equals(ZoomLevel.Far))
        {
            importDock.SetActive(false);
        }
    }

    public IEnumerator UpdateExportDock(Commit newCommit, ZoomLevel currentZoomlevel)
    {
        exportDock.SetActive(true);
        DependencyDock eDock = exportDock.GetComponent<DependencyDock>();
        eDock.ResetDependencies(null, null);

        Bundle bundle = bundleMaster.GetElement(newCommit);
        foreach(KeyValuePair<Bundle, float> exportPartner in bundle.GetExportReceiverBundles())
        {
            eDock.AddDockConnection(exportPartner.Key.GetMaster().islandController.importDock.GetComponent<DependencyDock>(), exportPartner.Value);
        }

        yield return null;
        eDock.ConstructConnectionArrows();
        if (currentZoomlevel.Equals(ZoomLevel.Far))
        {
            exportDock.SetActive(false);
        }
    }


    public IEnumerator SetChangeIndicator(ChangeStatus cs)
    {
        MeshFilter mf = changeIndikator.GetComponent<MeshFilter>();
        if (cs.Equals(ChangeStatus.newElement))
        {
            GameobjectHelperClass.setUVsToSingularCoord(Constants.colValNewHighlight, mf);
        }
        else if (cs.Equals(ChangeStatus.changedElement))
        {
            GameobjectHelperClass.setUVsToSingularCoord(Constants.colValChangeHighlight, mf);
        }
        if (!IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Near))
        {
            changeIndikator.SetActive(false);
        }
        yield return null;
    }

    public void SubstructureChange()
    {
        if (changeStatus.Equals(ChangeStatus.unknown))
        {
            changeStatus = ChangeStatus.changedElement;
            SetChangeIndicator(changeStatus);
        }

    }

    

}



