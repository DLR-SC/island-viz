using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using HexLayout.Basics;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ComposedTypes;
using OSGI_Datatypes.ArchitectureElements;

public class CoastlineController_Script : MonoBehaviour
{
    private HexGrid grid;

    public void SetGrid(HexGrid g)
    {
        grid = g;
    }

    public IEnumerator RenewCoastlineMesh(Commit c)
    {
        if (HistoryNavigation.Instance.showTimeDependentHight)
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
        //Empty lists to be filled with mesh info
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        //Create CoastLine Mesh
        foreach (Cell cell in grid.GetCells())
        {
            if (cell.IsAssignedToRegion())
            {
                int refCellHeight = HeightDifOfCell(c, cell);
                if (refCellHeight == -1)
                {
                    continue;
                }
                TimelineStatus tls = cell.GetCompUnit().RelationOfCommitToTimeline(c);

                Vector3[] hexVertices = cell.GetVerticeList(refCellHeight);
                int i = 0;
                foreach (Cell neighbourCell in cell.GetNeighboursForCoastLine())
                {
                    int neigCellHeight = HeightDifOfCell(c, neighbourCell);
                    if (refCellHeight == neigCellHeight)
                    {
                        //nothing to do neighbour cells same height
                        i++;
                        continue;
                    }
                    else if (neigCellHeight > refCellHeight)
                    {
                        //nothing to do, Coastline from neighbour
                        i++;
                        continue;
                    }
                    else
                    {
                        if (neigCellHeight == -1)
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
                            int heightdif = refCellHeight - neigCellHeight;
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

    public int HeightDifOfCell(Commit c, Cell cell)
    {
        int heightDif = 0;
        if (HistoryNavigation.Instance.showTimeDependentHight)
        {
            CompUnitMaster master = cell.GetCompUnit();
            if (master == null)
            {
                return -1;
            }
            TimelineStatus tls = master.RelationOfCommitToTimeline(c);
            if (tls.Equals(TimelineStatus.present))
            {
                heightDif = c.GetCommitIndex() - master.GetStart(SortTypes.byTime).GetCommitIndex();
            }
            else if (tls.Equals(TimelineStatus.notPresentAnymore))
            {
                heightDif = master.GetEnd(SortTypes.byTime).GetCommitIndex() - master.GetStart(SortTypes.byTime).GetCommitIndex();
            }
            else if (tls.Equals(TimelineStatus.notYetPresent))
            {
                heightDif = -1;
            }
        }
        return heightDif;
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
                        if (!neighbourCell.IsAssignedToRegion() || neighbourCell.GetCompUnit().RelationOfCommitToTimeline(c) == TimelineStatus.notYetPresent)
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


}
