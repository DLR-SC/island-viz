using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Geometry;

using VFace = TriangleNet.Topology.DCEL.Face;

public class VoronoiTester : MonoBehaviour {


	// Use this for initialization
	void Start () {

        TriangleNet.Configuration conf = new TriangleNet.Configuration();


        List<Vertex> vertices = OsgiViz.Helperfunctions.createPointsOnPlane(1f, 1f, 50, 50, 1f, new System.Random());
        SweepLine sl = new SweepLine();
        //TriangleNet.Mesh tMesh = (TriangleNet.Mesh)TriangleNet.Meshing.GenericMesher.StructuredMesh(1f, 1f, 10, 10);
        TriangleNet.Mesh tMesh = (TriangleNet.Mesh)sl.Triangulate(vertices, conf);
        TriangleNet.Voronoi.BoundedVoronoi voronoi = new TriangleNet.Voronoi.BoundedVoronoi(tMesh);

        foreach (TriangleNet.Topology.DCEL.Face vf in voronoi.Faces)
            voronoiFaceToGO(vf);

    }

    public GameObject voronoiFaceToGO(TriangleNet.Topology.DCEL.Face face)
    {
        GameObject cell;

        if (face.Bounded)
        {
            cell = new GameObject("FaceID: " + face.ID);
            MeshFilter mFilter = cell.AddComponent<MeshFilter>();
            MeshRenderer mRender = cell.AddComponent<MeshRenderer>();
            mRender.material = new Material(Shader.Find("Diffuse"));
            mRender.material.color = new Color(1f, 1f, 1f);

            List<Vertex> vertices = new List<Vertex>();
            foreach (TriangleNet.Topology.DCEL.HalfEdge he in face.EnumerateEdges())
            {
                Vertex v = new Vertex(he.Origin.X, he.Origin.Y);
                vertices.Add(v);
            }
            SweepLine sl = new SweepLine();
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            TriangleNet.Mesh tMeshCell = (TriangleNet.Mesh)sl.Triangulate(vertices, conf);
            mFilter.mesh = OsgiViz.Helperfunctions.convertTriangleNETMesh(tMeshCell);
        }
        else
        {
            cell = new GameObject("unbounded cell");
        }

        return cell;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
