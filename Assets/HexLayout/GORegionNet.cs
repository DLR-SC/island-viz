using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;

public class GORegionNet : MonoBehaviour {

    public void InitialiseStandards(Vector3 color)
    {
        var rend = gameObject.AddComponent<MeshRenderer>();
        rend.material = Resources.Load<Material>("TestPlaneMaterial");
        rend.material.color = new Color(color.x, color.y, color.z);

        var mf = gameObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;
    }

    public void RenewMesh(List<Cell> cellList, int currentTime)
    {
        List<Vector3> Vertices = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Vector3> Normals = new List<Vector3>();

        for (int i = 0; i<cellList.Count; i++)
        {
            Vertices.AddRange(cellList[i].GetVerticeList(currentTime));
            Triangles.AddRange(cellList[i].GetTrianglesList(i));
            Normals.AddRange(cellList[i].GetNormals());

        }

        var newVertices = Vertices.ToArray();
        var newTriangles = Triangles.ToArray();
        var newNormals = Normals.ToArray();

        var mf = gameObject.GetComponent<MeshFilter>();
        var mesh = gameObject.GetComponent<MeshFilter>().mesh;

        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
    }
}
