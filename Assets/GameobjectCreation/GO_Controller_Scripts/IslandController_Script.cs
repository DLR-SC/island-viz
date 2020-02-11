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
            region.name = "Region";
            region.transform.parent = gameObject.transform;
            region.transform.localPosition = new Vector3(0, 0, 0);
            region.layer = LayerMask.NameToLayer("Visualization");
            region.GetComponent<RegionController_Script>().SetPackage(pm);
            region.GetComponent<RegionController_Script>().SetParentIsland(gameObject);
            region.GetComponent<Region>().setParentIsland(islandGOScript);
            region.GetComponent<RegionController_Script>().InitColor(Constants.colVals[i]);
            region.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;
            yield return region.GetComponent<RegionController_Script>().CreateBuildingManagers();
            regions.Add(region);
            i = (i+1)%8;

        }
        yield return null;
    }

    public GameObject AddChangeIndicator()
    {
        if (changeIndikator != null)
        {
            return changeIndikator;
        }
            changeIndikator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            changeIndikator.transform.parent = gameObject.transform;
            changeIndikator.transform.localPosition = new Vector3(0f, Constants.standardHeight/6f, 0f);
            changeIndikator.transform.localScale = new Vector3(5f, Constants.standardHeight/3f, 5f);
            changeIndikator.name = "ChangeIndicator";
            changeIndikator.GetComponent<CapsuleCollider>().enabled = false;
            changeIndikator.GetComponent<MeshRenderer>().sharedMaterial = IslandVizVisualization.Instance.CombinedHoloMaterial;

        if (coastLine != null)
        {
            Renderer coastRender = coastLine.GetComponent<MeshRenderer>();
            Vector3 center = coastRender.bounds.center;
            Vector3 extends = coastRender.bounds.extents;
            float radius = Mathf.Max(extends.x, extends.z);
            Vector3 centerlocal = transform.InverseTransformPoint(center);
            centerlocal.y = Constants.standardHeight / 6f;
            changeIndikator.transform.localPosition = centerlocal;
            changeIndikator.transform.localScale = new Vector3(2 * radius / transform.lossyScale.x, Constants.standardHeight / 3f, 2 * radius / transform.lossyScale.z);
        }
           

        return changeIndikator;
    }


    public IEnumerator UpdateRoutine(Commit newCommit, IslandContainerController_Script controllerScript, bool justActivated, System.Action<IslandController_Script> callback)
    {
        changeStatus = ChangeStatus.unknown;
        if (justActivated)
        {
            changeStatus = ChangeStatus.newElement;
        }
        else
        {
            changeIndikator.SetActive(false);
            changeIndikator.GetComponent<MeshRenderer>().enabled = false;
            //GameObject.Destroy(changeIndikator);
            //changeIndikator = null;
        }

        //Reposition & Update IslandDocks
        float radius2 = bundleMaster.GetGrid().GetOuterAssignedFirstTwoSixths(newCommit);
        if (radius2 < 1)
            radius2 = 1;
        radius2 = Constants.GetRadiusFromRing((int)radius2);
        importDock.transform.localPosition = new Vector3(0f, Constants.dockYPos, 1.1f * radius2 + 2);
        exportDock.transform.localPosition = new Vector3(2f, Constants.dockYPos, 1.1f * radius2 + 1);
        yield return UpdateExportDock(newCommit);
        yield return UpdateImportDock(newCommit);
        //after this docks are ready for Dependency Arrow creation
        callback(this);

        //Update visible Sub-GameObjects
        StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));
        List<Region> activeRegions = new List<Region>();
        foreach (GameObject region in regions)
        {
            StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegion(null, newCommit, (returnScript)=> { if (returnScript != null) { activeRegions.Add(returnScript); } } ));
        }
        yield return null;

        //Update IslandGO-Script Attributes
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
        float radius = Mathf.Max(extends.x, extends.z);

        //Resize & Update Island Collider
        SphereCollider cc = gameObject.GetComponent<SphereCollider>();
        cc.radius = 2*radius / transform.lossyScale.x;
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
            cc.enabled = true;
        else
            cc.enabled = false;

        //Resize ChangeIndicator Based on MeshSizeOfIsland
        if (changeIndikator != null)
        {
            Vector3 centerlocal = transform.InverseTransformPoint(center);
            centerlocal.y = Constants.standardHeight / 6f;
            changeIndikator.transform.localPosition = centerlocal;
            changeIndikator.transform.localScale = new Vector3(2 * radius / transform.lossyScale.x, Constants.standardHeight / 3f, 2 * radius / transform.lossyScale.z);
            SetChangeIndicator(changeStatus);
        }



        islandGOScript.SetRegions(activeRegions);

        yield return null;
    }

    public IEnumerator UpdateImportDock(Commit newCommit)
    {
        importDock.SetActive(true);
        DependencyDock iDock = importDock.GetComponent<DependencyDock>();

        Bundle bundle = bundleMaster.GetElement(newCommit);
        foreach (KeyValuePair<Bundle, float> importBundle in bundle.GetImportedBundles())
        {
            iDock.AddDockConnection(importBundle.Key.GetMaster().islandController.exportDock.GetComponent<DependencyDock>(), importBundle.Value);
        }

        yield return null;
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
        {
            importDock.SetActive(false);
        }
    }

    public IEnumerator UpdateExportDock(Commit newCommit)
    {
        exportDock.SetActive(true);
        DependencyDock eDock = exportDock.GetComponent<DependencyDock>();

        Bundle bundle = bundleMaster.GetElement(newCommit);
        foreach(KeyValuePair<Bundle, float> exportPartner in bundle.GetExportReceiverBundles())
        {
            eDock.AddDockConnection(exportPartner.Key.GetMaster().islandController.importDock.GetComponent<DependencyDock>(), exportPartner.Value);
        }

        yield return null;
        if (IslandVizVisualization.Instance.CurrentZoomLevel.Equals(ZoomLevel.Far))
        {
            exportDock.SetActive(false);
        }
    }


    public void SetChangeIndicator(ChangeStatus cs)
    {
        if (cs.Equals(ChangeStatus.unknown) || cs.Equals(ChangeStatus.deletedElement))
        {
            changeIndikator.GetComponent<MeshRenderer>().enabled = false;
            return;
        }
        changeIndikator.GetComponent<MeshRenderer>().enabled = true;
        MeshFilter mf = changeIndikator.GetComponent<MeshFilter>();
        if (cs.Equals(ChangeStatus.newElement))
        {
            GameobjectHelperClass.setUVsToSingularCoord(Constants.colValNewHighlight, mf);
        }
        else if (cs.Equals(ChangeStatus.changedElement))
        {
            GameobjectHelperClass.setUVsToSingularCoord(Constants.colValChangeHighlight, mf);
        }else if (cs.Equals(ChangeStatus.changedInnerElement))
        {
            GameobjectHelperClass.setUVsToSingularCoord(Constants.colValChangeBuildingHighlight, mf);
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
       
    }

    public void SubstructureChange()
    {
        if (changeStatus.Equals(ChangeStatus.unknown)||changeStatus.Equals(ChangeStatus.changedInnerElement))
        {
            changeStatus = ChangeStatus.changedElement;
            SetChangeIndicator(changeStatus);
        }

    }

    public void BuildingHightChange()
    {
        if (changeStatus.Equals(ChangeStatus.unknown))
        {
            changeStatus = ChangeStatus.changedInnerElement;
            SetChangeIndicator(changeStatus);
        }
    }

    public void ChangeHighlight(bool enabled)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (changeIndikator == null)
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



