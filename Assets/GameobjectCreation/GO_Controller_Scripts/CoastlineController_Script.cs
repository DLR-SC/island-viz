using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using HexLayout.Basics;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;

public class CoastlineController_Script : MonoBehaviour
{
    private HexGrid grid;
    private bool timeDepHight;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 coastLineColor = Constants.coastlineColor;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(coastLineColor.x / 255f, coastLineColor.y / 255f, coastLineColor.z / 255f, 1f);
    }

    public void SetGrid(HexGrid g)
    {
        grid = g;
    }

    public void ShowTimeDependentHight(bool value)
    {
        //TODO dies ändern momentan nur false, da einfacher und anderer Fall noch nicht ausimplementiert
        timeDepHight = false;
        //TODO einkommentieren, obere Zeile löschen
        //timeDepHight = value;
    }

    public IEnumerator RenewCoastlineMesh(Commit c)
    {
        if (timeDepHight)
        {
            yield return CoastlineTimeDependentHight(c);
        }
        else
        {
            yield return CoastlineStandardHight(c);
        }
        yield return null;
    }

    private IEnumerator CoastlineTimeDependentHight(Commit c)
    {
        //TODO
        yield return null;
    }

    private IEnumerator CoastlineStandardHight(Commit c)
    {
        //Empty lists to be filled with mesh info
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        //Create CoastLine Mesh
        foreach (Cell cell in grid.GetCells())
        {
            if (cell.IsAssignedToRegion())
            {
                TimelineStatus tls = cell.GetCompUnit().RelationOfCommitToTimeline(c);
                if (tls == TimelineStatus.present || tls == TimelineStatus.notPresentAnymore)
                {
                    Vector3[] hexVertices = cell.GetVerticeList(0);
                    int i = 0;
                    foreach (Cell neighbourCell in cell.GetNeighboursForCoastLine())
                    {
                        //Part Of Coastline if neighbour is not assigned at all or not yet assigned (not anymore is deatharea and thus needs no coastline)
                        if(!neighbourCell.IsAssignedToRegion() || neighbourCell.GetCompUnit().RelationOfCommitToTimeline(c) == TimelineStatus.notYetPresent)
                        {
                            //CoastLine completely down to sea
                            vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y, hexVertices[(i + 1) % 6].z));
                            vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y, hexVertices[i].z));
                            vertices.Add(new Vector3(hexVertices[i].x, 0f, hexVertices[i].z));
                            vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, 0f, hexVertices[(i + 1) % 6].z));

                            int j = vertices.Count;
                            triangles.Add(j - 4);
                            triangles.Add(j - 1);
                            triangles.Add(j - 3);
                            triangles.Add(j - 3);
                            triangles.Add(j - 1);
                            triangles.Add(j - 2);

                            Vector3 normal = HexHelper.GetNormalForSide(i, (i + 1) % 6);
                            normals.Add(normal);
                            normals.Add(normal);
                            normals.Add(normal);
                            normals.Add(normal);
                        }
                        i++;
                    }
                }
            }
        }
        yield return null;

        //Update Unity Mesh
        var newVertices = vertices.ToArray();
        var newTriangles = triangles.ToArray();
        var newNormals = normals.ToArray();

        var mesh = gameObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
        yield return null;
    }

    public void CopiedFunktion(int time)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        foreach (Cell cell in grid.GetCells())
        {
            if (cell.IsAssignedToRegion())
            {
                Vector3[] hexVertices = cell.GetVerticeList(time);
                int i = 0;
                foreach (Cell neighbourCell in cell.GetNeighboursForCoastLine())
                {
                    if (neighbourCell.IsAssignedToRegion() && (cell.GetTimeTag() >= neighbourCell.GetTimeTag() || time == 0))
                    {
                        //neighbour cell on same height, no coastline necessary
                        i++;
                        continue;
                    }
                    if (!neighbourCell.IsAssignedToRegion())
                    {
                        //CoastLine completely down to sea
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y, hexVertices[(i + 1) % 6].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[i].x, 0f, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, 0f, hexVertices[(i + 1) % 6].z));
                    }
                    else
                    {
                        //Coastline down to neighbourCell
                        int heightdif = neighbourCell.GetTimeTag() - cell.GetTimeTag();
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y, hexVertices[(i + 1) % 6].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y - Constants.heightFactor * heightdif, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y - Constants.heightFactor * heightdif, hexVertices[(i + 1) % 6].z));
                    }
                    int j = vertices.Count;
                    triangles.Add(j - 4);
                    triangles.Add(j - 1);
                    triangles.Add(j - 3);
                    triangles.Add(j - 3);
                    triangles.Add(j - 1);
                    triangles.Add(j - 2);

                    Vector3 normal = HexHelper.GetNormalForSide(i, (i + 1) % 6);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    i++;
                }
            }
        }
        var newVertices = vertices.ToArray();
        var newTriangles = triangles.ToArray();
        var newNormals = normals.ToArray();

        var mf = gameObject.GetComponent<MeshFilter>();
        var mesh = gameObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

    }


}
