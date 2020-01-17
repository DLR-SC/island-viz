using Assets;
using HexLayout.Basics;
using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ComposedTypes;

public class DeathAreaController_Script : MonoBehaviour
{

    private HexGrid grid;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 deathAreaColor = Constants.deathAreaColor;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(deathAreaColor.x / 255f, deathAreaColor.y / 255f, deathAreaColor.z / 255f, 1f);
        IslandVizInteraction.Instance.OnNewCommit += OnNewCommit;
    }

    public void SetGrid(HexGrid g)
    {
        grid = g;
    }

    public void OnNewCommit(Commit oldCommit, Commit newCommit)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RenewDeathAreaMesh(newCommit));
        }
    }

    public IEnumerator RenewDeathAreaMesh(Commit c)
    {
        //Lists as Input for new Mesh
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Vector3> Normals = new List<Vector3>();

        int cellsInMesh = 0;

        foreach (Cell cell in grid.GetCells())
        {
            if (cell.IsAssignedToRegion())
            {
                if (cell.GetCompUnit().RelationOfCommitToTimeline(c) == TimelineStatus.notPresentAnymore)
                {
                    //TODO: Welche Höhe haben Tote Zellen=
                    Vertices.AddRange(cell.GetVerticeList(0));
                    Triangles.AddRange(cell.GetTrianglesList(cellsInMesh));
                    Normals.AddRange(cell.GetNormals());
                    cellsInMesh++;
                }
            }
        }
        yield return null;

        if (cellsInMesh != 0)
        {
          //  Debug.Log("Cells in DeathArea Mesh " + cellsInMesh);
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
}
