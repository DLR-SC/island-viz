using Assets;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;
using OSGI_Datatypes.ComposedTypes;
using OsgiViz.SoftwareArtifact;
using OsgiViz.Unity.Island;

public class RegionController_Script : MonoBehaviour
{
    [SerializeField]
    private GameObject buildingManager_Prefab;
    public float scaleFactor = 0.7f;

    HexLayout.Basics.Region region;
    PackageMaster packageMaster;
    private List<GameObject> buildingMangagerGOs;
    private  OsgiViz.Unity.Island.Region regionScript;
    private Vector2 colorValue;


    #region Initialisation

    public void Start()
    {
        regionScript = gameObject.GetComponent<OsgiViz.Unity.Island.Region>();
        regionScript.setRegionArea(gameObject.GetComponent<MeshFilter>());
    }

    public void InitColor(Vector2 value)
    {
        colorValue = value;
        //Vector3 color = Constants.colVals[i];
        //gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x / 255f, color.y / 255f, color.z / 255f, 1f);
    }
    
    public void SetPackage(PackageMaster pm, IslandGO parentIsland)
    {
        packageMaster = pm;
        region = pm.GetRegion();
        //TODO set Parent Island But not on this Place because regionScript is not yet known here
        //regionScript.setParentIsland(parentIsland);
    }

    public IEnumerator CreateBuildingManagers()
    {
        buildingMangagerGOs = new List<GameObject>();
        PackageMaster package = region.GetPackageMaster();

        int i = 0;
        foreach (CompUnitMaster cum in package.GetContainedMasterCompUnits())
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

    #endregion

    public void CreateMesh(Commit c, List<Vector3> Vertices, List<int> Triangles, List<Vector3> Normals, out int cellsInMesh)
    {
         cellsInMesh = 0;

        foreach (Cell assignedCell in region.GetAssignedCells())
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
            else if (tls.Equals(TimelineStatus.notPresentAnymore))
            {
                //TODO verticeList unter beeinflussung von timeDepHight & letztem vorhandenen Commit
                Vertices.AddRange(assignedCell.GetVerticeList(0));
                Triangles.AddRange(assignedCell.GetTrianglesList(cellsInMesh));
                Normals.AddRange(assignedCell.GetNormals());
                cellsInMesh++;
            }
        }
    }

    public void RenewRegionMesh(List<Vector3> Vertices, List<int> Triangles, List<Vector3> Normals)
    {
        var newVertices = Vertices.ToArray();
        var newTriangles = Triangles.ToArray();
        var newNormals = Normals.ToArray();

        var mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
    }

    public void RenewColliderMesh(List<Vector3> Vertices, List<int> Triangles, List<Vector3> Normals)
    {
        var newVertices = Vertices.ToArray();
        var newTriangles = Triangles.ToArray();
        var newNormals = Normals.ToArray();


        MeshCollider mc = gameObject.GetComponent<MeshCollider>();
        var mesh = mc.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
        }
        else
        {
            mesh.Clear();
        }
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
    }

    public IEnumerator UpdateBuildings(Commit c, List<Building> activeBuildingsInRegion)
    {
        int i = 0;
        foreach(GameObject buildingManager in buildingMangagerGOs)
        {
            Building result = buildingManager.GetComponent<BuildingController_Script>().UpdateBuilding(c);
            if (result != null)
            {
                activeBuildingsInRegion.Add(result);
            }
            i++;
            if (i % 10 == 0)
            {
                yield return null;
            }
        }
    }

  

    /// <summary>
    /// MainMethod called to Renew Region
    /// </summary>
    /// <param name="oldCommit"></param>
    /// <param name="newCommit"></param>
    /// <param name="tls"></param>
    /// <param name="regionScript"></param>
    /// <returns></returns>
    public IEnumerator RenewRegion(Commit oldCommit, Commit newCommit, TimelineStatus tls, OsgiViz.Unity.Island.Region rS)
    {
        tls = packageMaster.RelationOfCommitToTimeline(newCommit);
        rS = regionScript; 

        //Variables
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Vector3> Normals = new List<Vector3>();

        int cellsInMesh = 0;
        yield return null;

        CreateMesh(newCommit, Vertices, Triangles, Normals, out cellsInMesh);
        //yield return null;

        //Set Mesh To Filter
        RenewRegionMesh(Vertices, Triangles, Normals);
        yield return null;
        //Set Color?
        MeshFilter regionMeshFilter = gameObject.GetComponent<MeshFilter>();
        setUVsToSingularCoord(colorValue, regionMeshFilter);
        yield return null;

        //Set Mesh To MeshCollider
        RenewColliderMesh(Vertices, Triangles, Normals);

        //Renew Buildings
        List<Building> activeBuildings = new List<Building>();
        yield return UpdateBuildings(newCommit, activeBuildings);

        //Renew Attributes in RegionScript
        Package package = packageMaster.GetElement(newCommit);
        regionScript.setPackage(package);
        regionScript.setBuildings(activeBuildings);

    }

    private void setUVsToSingularCoord(Vector2 newUV, MeshFilter mesh)
    {
        Vector3[] uvs = mesh.mesh.vertices;
        Vector2[] newUVs = new Vector2[uvs.Length];
        for (int i = 0; i < uvs.Length; i++)
            newUVs[i] = newUV;

        mesh.sharedMesh.uv = newUVs;
    }
}

