using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using OSGI_Datatypes.OrganisationElements;

public class IslandController_Script : MonoBehaviour
{
    public GameObject coastlinePrefab;
    public GameObject deathareaPrefab;
    public GameObject importDockPrefab;
    public GameObject exportDockPrefab;
    public GameObject regionPrefab;

    private GameObject coastLine;
    private GameObject deathArea;
    private GameObject exportDock;
    private GameObject importDock;
    private List<GameObject> regions;
    private Rigidbody rb;

    private BundleMaster bundleMaster;
    //private IslandObjectContainer_Script mainController;
    //private Commit currentCommit;
    //private bool transformationRunning;


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
        rb = gameObject.GetComponent<Rigidbody>();
        //rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;

        regions = new List<GameObject>();

        coastLine = Instantiate(coastlinePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        coastLine.name = "Coastline";
        coastLine.transform.parent = gameObject.transform;
        coastLine.transform.localPosition = new Vector3(0, 0, 0);
        coastLine.GetComponent<CoastlineController_Script>().SetGrid(bundleMaster.GetGrid());
        coastLine.GetComponent<CoastlineController_Script>().ShowTimeDependentHight(false);

        deathArea = Instantiate(deathareaPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        deathArea.name = "DeathArea";
        deathArea.transform.parent = gameObject.transform;
        deathArea.transform.localPosition = new Vector3(0, 0, 0);
        deathArea.GetComponent<DeathAreaController_Script>().SetGrid(bundleMaster.GetGrid());
        deathArea.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/Combined HoloMaterial");

        importDock = Instantiate(importDockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        importDock.name = "ImportDock";
        importDock.transform.parent = gameObject.transform;
        importDock.transform.localPosition = new Vector3(0, Constants.dockYPos, 2);

        exportDock = Instantiate(exportDockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        exportDock.name = "ExportDock";
        exportDock.transform.parent = gameObject.transform;
        exportDock.transform.localPosition = new Vector3(2, Constants.dockYPos, 0);

        int regionCount = 0;
        foreach (PackageMaster pm in bundleMaster.GetContainedMasterPackages())
        {
            GameObject region = Instantiate(regionPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            region.name = "Region"; //TODO sinnnvoller Name für Region nach Packet
            region.transform.parent = gameObject.transform;
            region.transform.localPosition = new Vector3(0, 0, 0);
            region.GetComponent<RegionController_Script>().SetPackage(pm);
            region.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/Combined HoloMaterial");
            region.GetComponent<RegionController_Script>().InitColor(regionCount);
            //TODO timedephight global regeln
            yield return region.GetComponent<RegionController_Script>().CreateBuildingManagers();
            regionCount = (regionCount + 1) % Constants.colVals.Length;
            regions.Add(region);
        }
        yield return null;
    }


    public IEnumerator UpdateRoutine(Commit newCommit, IslandContainerController_Script controllerScript)
    {

        //TODO Death Area Rausnehmen update regions als Coroutinen probieren
        foreach (GameObject region in regions)
        {
            StartCoroutine(region.GetComponent<RegionController_Script>().RenewRegionMesh(newCommit));
            StartCoroutine(region.GetComponent<RegionController_Script>().UpdateBuildings(newCommit));
        }
        StartCoroutine(deathArea.GetComponent<DeathAreaController_Script>().RenewDeathAreaMesh(newCommit));
        StartCoroutine(coastLine.GetComponent<CoastlineController_Script>().RenewCoastlineMesh(newCommit));

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

        float colliderDim = 3f;
        
        if(radiusSegment + 3 <= radiusTotal)
        {
            colliderDim = radiusTotal;
        }
        else
        {
            colliderDim = radiusSegment + 3;
        }
        colliderDim += 8;

        float boxDim = Mathf.Floor(colliderDim / Mathf.Sqrt(2) * 10) / 10f;


        gameObject.GetComponent<BoxCollider>().size = new Vector3(boxDim * 2, 6, boxDim *2);
        gameObject.GetComponent<SphereCollider>().radius = colliderDim;

        controllerScript.NotifyIslandTransformationFinished();
        yield return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.tag.Equals("Island"))
        {
            Debug.Log("CollisionExit " + gameObject.name + ", " + collision.gameObject.name);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 dirToOther = collision.gameObject.transform.position;
        Vector3 dirRight = Vector3.Cross(Vector3.up, dirToOther);
        rb.AddForce(5 * (dirRight - dirToOther).normalized);
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Island"))
        {
            Debug.Log("CollisionExit " + gameObject.name + ", " + collision.gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (rb == null)
        {
            return;
        }
        if (other.gameObject.tag.Equals("Island")&&gameObject!=other.gameObject)
        {
            Vector3 dir = other.gameObject.transform.position - gameObject.transform.position;

            Vector3 velo = rb.velocity;
            Vector3 repForceDir;
            if (velo.magnitude != 0)
            {
                repForceDir = Vector3.Cross(Vector3.up, velo);
            }
            else
            {
                repForceDir = Vector3.Cross(Vector3.up, dir);
            }
            float factor = 20f;
            if (dir.magnitude != 0)
            {
                factor *= Mathf.Log(1f / dir.magnitude);
            }
            gameObject.GetComponent<ConstantForce>().force = factor * repForceDir.normalized;
            Debug.Log("Trigger Enter " + gameObject.name + ", " + other.gameObject.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Island"))
        {
            gameObject.GetComponent<ConstantForce>().force = Vector3.zero;
            Debug.Log("TriggerExit " + gameObject.name + ", " + other.gameObject.name);
        }
    }


}



