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
    public GameObject changeIndikator { get; set; }

    private BundleMaster bundleMaster;

    private System.Random RNG;
    private IslandGO islandGOScript;
    public ChangeStatus changeStatus { get; set; }
    //private IslandObjectContainer_Script mainController;
    //private Commit currentCommit;
    //private bool transformationRunning;


    public void Awake()
    {
        RNG = new System.Random(0);
    }

    public void Start()
    {
        IslandVizInteraction.Instance.OnHistoryHighlightChanged += ChangeHighlight;
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
        }
        else
        {
            changeIndikator.SetActive(false);
        }
        ZoomLevel currentZoomLevel = islandGOScript.CurrentZoomLevel;
        islandGOScript.ResetRegions();

        //Update visible Sub-GameObjects
        StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));
        yield return null;

        List<Region> activeRegions = new List<Region>();
        float islandHeight = 10;
        foreach (GameObject region in regions)
        {
            StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegion(null, newCommit, currentZoomLevel, (returnScript)=> { if (returnScript != null) { activeRegions.Add(returnScript); } } ));
        }
        yield return null;

        //Update IslandGO-Script Attributes
        //islandGOScript.SetRegions(activeRegions);
        Bundle bundle = bundleMaster.GetElement(newCommit);
        islandGOScript.Bundle = bundle;
        gameObject.name = bundle.getName();
        importDock.name = bundle.getName() + "_ImportDock";
        exportDock.name = bundle.getName() + "_ExportDock";

        yield return null;

        //Get Island Radius
        Renderer coastRender = coastLine.GetComponent<MeshRenderer>();
        Vector3 center = coastRender.bounds.center;
        Vector3 extends = coastRender.bounds.extents;
        float radius = Mathf.Sqrt(Mathf.Pow(extends.x, 2) + Mathf.Pow(extends.z, 2));

        //Reposition IslandDocks
        float radius2 = bundleMaster.GetGrid().GetOuterAssignedFirstTwoSixths(newCommit);
        if (radius2 < 1)
            radius2 = 1;
        radius2 = Constants.GetRadiusFromRing((int)radius2);
        Vector3 centerlocal = transform.InverseTransformPoint(center);
        centerlocal.y = 0;
        importDock.transform.localPosition = new Vector3(0f, Constants.dockYPos, 1.1f*radius2 + 2);
        exportDock.transform.localPosition = new Vector3(2f, Constants.dockYPos, 1.1f*radius2 + 1);

        StartCoroutine(UpdateExportDock(newCommit, currentZoomLevel));
        StartCoroutine(UpdateImportDock(newCommit, currentZoomLevel));

        //Resize & Update Island Collider
        SphereCollider cc = gameObject.GetComponent<SphereCollider>();
        cc.radius = 2*radius;
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
            cc.enabled = true;
        else
            cc.enabled = false;

        //Resize ChangeIndicator Based on MeshSizeOfIsland
        centerlocal.y = Constants.standardHeight / 6f;
        changeIndikator.transform.localPosition = centerlocal;
        changeIndikator.transform.localScale = new Vector3(2*radius/transform.lossyScale.x, Constants.standardHeight / 3f, 2*radius/transform.lossyScale.z);
        StartCoroutine(SetChangeIndicator(changeStatus));


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
        //iDock.ConstructConnectionArrows();
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
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
        //eDock.ConstructConnectionArrows();
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
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
        if (!IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
        {
            changeIndikator.SetActive(false);
        }
        else if(HistoryNavigation.Instance.historyHighlightActive)
        {
            changeIndikator.SetActive(true);
        }
        else
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
            StartCoroutine(SetChangeIndicator(changeStatus));
        }

    }

    public void ChangeHighlight(bool enabled)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (!enabled)
        {
            changeIndikator.SetActive(false);
        }
        else
        {
            if ((changeStatus.Equals(ChangeStatus.newElement) || changeStatus.Equals(ChangeStatus.changedElement))&&IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
            {
                changeIndikator.SetActive(true);
            }
        }
    }

}



