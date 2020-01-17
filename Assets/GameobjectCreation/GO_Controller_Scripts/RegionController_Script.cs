using Assets;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;
using OSGI_Datatypes.ComposedTypes;

public class RegionController_Script : MonoBehaviour
{
    [SerializeField]
    private GameObject buildingManager_Prefab;
    public float scaleFactor = 0.7f;

    Region region;
    private List<GameObject> buildingMangagerGOs;

    public void Start()
    {
        IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;
    }

    public void InitColor(int i)
    {
        Vector3 color = Constants.colVals[i];
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x / 255f, color.y / 255f, color.z / 255f, 1f);
    }


    public void SetPackage(PackageMaster pm)
    {
        region = pm.GetRegion();
    }

    public IEnumerator RenewRegionMesh(Commit c)
    {
        //Debug.Log("Entering Renew Region Mesh");
        //Lists as Input for new Mesh
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Vector3> Normals = new List<Vector3>();

        int cellsInMesh = 0;

        foreach(Cell assignedCell in region.GetAssignedCells())
        {
            TimelineStatus tls = assignedCell.GetCompUnit().RelationOfCommitToTimeline(c);
            if (tls.Equals(TimelineStatus.present))
            {
                //TODO verticeList unter beeinflussung von timeDepHight
                Vertices.AddRange(assignedCell.GetVerticeList(0));
                Triangles.AddRange(assignedCell.GetTrianglesList(cellsInMesh));
                Normals.AddRange(assignedCell.GetNormals());
                cellsInMesh++;
            }
        }
        yield return null;

        if (cellsInMesh != 0)
        {
          //  Debug.Log("Cells in Mesh " + cellsInMesh);
        }

        //Set Data from Mesh-Lists to mesh
        var newVertices = Vertices.ToArray();
        var newTriangles = Triangles.ToArray();
        var newNormals = Normals.ToArray();

        var mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

        yield return null;
    }

    public IEnumerator UpdateBuildings(Commit c)
    {
        int i = 0;
        foreach(GameObject buildingManager in buildingMangagerGOs)
        {
            buildingManager.GetComponent<BuildingController_Script>().UpdateBuilding(c);
            i++;
            if (i % 10 == 0)
            {
                yield return null;
            }
        }
    }

    public IEnumerator CreateBuildingManagers()
    {
        buildingMangagerGOs = new List<GameObject>();
        PackageMaster package = region.GetPackageMaster();

        int i = 0;
        foreach(CompUnitMaster cum in package.GetContainedMasterCompUnits())
        {
            GameObject buildingM = Instantiate(buildingManager_Prefab, Vector3.zero, Quaternion.identity);
            buildingM.transform.parent = gameObject.transform;

            float[] center = cum.GetCell().GetAbsoluteCoordinates();
            buildingM.transform.localPosition = new Vector3(center[0], Constants.dockYPos, center[1]);
            buildingM.transform.localRotation = new Quaternion(0, Random.value * 2 * Mathf.PI, 0, 1);
            buildingM.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            buildingMangagerGOs.Add(buildingM);

            buildingM.GetComponent<BuildingController_Script>().SetCompUnit(cum);

            i++;
            if (i % 10 == 0)
            {
                yield return null;
            }
        }
    }

    public void OnNewCommit(Commit oldCommit, Commit newCommit)
    {
        StartCoroutine(RenewRegionMesh(newCommit));
    }
}
