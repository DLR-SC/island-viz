using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;
using Assets;

public class GOCoastLine : MonoBehaviour {

    HexGrid grid;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(HexGrid grid)
    {
        this.grid = grid;
    }

    public void UpdateCoastLine(int time)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        foreach(Cell cell in grid.GetCells())
        {
            if (cell.IsAssignedToRegion())
            {
                Vector3[] hexVertices = cell.GetVerticeList(time);
                int i = 0;
                foreach(Cell neighbourCell in cell.GetNeighboursForCoastLine())
                {
                    if(neighbourCell.IsAssignedToRegion() && (cell.GetTimeTag() >= neighbourCell.GetTimeTag() || time==0))
                    {
                        //neighbour cell on same height, no coastline necessary
                        i++;
                        continue;
                    }
                    if (!neighbourCell.IsAssignedToRegion())
                    {
                        //CoastLine completely down to sea
                        vertices.Add(new Vector3(hexVertices[(i+1)%6].x, hexVertices[(i + 1)% 6].y, hexVertices[(i + 1)% 6].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[i].x, 0f, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, 0f, hexVertices[(i + 1) % 6].z));
                    }
                    else
                    {
                        //Coastline down to neighbourCell
                        int heightdif = neighbourCell.GetTimeTag()- cell.GetTimeTag();
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y, hexVertices[(i + 1) % 6].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[i].x, hexVertices[i].y - Constants.heightFactor* heightdif, hexVertices[i].z));
                        vertices.Add(new Vector3(hexVertices[(i + 1) % 6].x, hexVertices[(i + 1) % 6].y - Constants.heightFactor*heightdif, hexVertices[(i + 1) % 6].z));
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
