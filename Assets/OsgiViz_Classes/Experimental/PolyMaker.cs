using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PolyMaker : MonoBehaviour {

    private int polyCount = 3;

    public GameObject cursor3d;
    public GameObject steamControllerGameObject;

    private GameObject snapCursor3d;
    private Mesh createdMesh;
    private GameObject createdObject;
    private List<Vector3> currentPoly_Vertices;
    private List<int> currentPoly_Triangles;
    private List<Vector3> currentPoly_Normals;
    private SteamVR_TrackedController controller;

    private bool controller_triggerPreviousFrame;

    void Start () {

        Assert.IsNotNull(cursor3d,"No cursor specified for the PolyMaker!");

        createdMesh = new Mesh();
        createdObject = new GameObject("PolyMaker_Object");
        MeshFilter meshFilt = createdObject.AddComponent<MeshFilter>();
        meshFilt.sharedMesh = createdMesh;
        Renderer rend = createdObject.AddComponent<MeshRenderer>();
        rend.material = new Material(Shader.Find("Diffuse"));
       
        currentPoly_Vertices  = new List<Vector3>();
        currentPoly_Normals   = new List<Vector3>();
        currentPoly_Triangles = new List<int>() { 0, 1, 2 };

        controller = steamControllerGameObject.GetComponent<SteamVR_TrackedController>();
        controller_triggerPreviousFrame = false;


    }
	
	// Update is called once per frame
	void Update () {

        handleInput();

        if (currentPoly_Vertices.Count == polyCount)
            finalizePoly();

        controller_triggerPreviousFrame = controller.triggerPressed;
	}
    


    void finalizePoly()
    {

        //Compute normal
        Vector3 diffA  = Vector3.Normalize(currentPoly_Vertices[0] - currentPoly_Vertices[1]);
        Vector3 diffB  = Vector3.Normalize(currentPoly_Vertices[2] - currentPoly_Vertices[1]);
        Vector3 normal = Vector3.Normalize( Vector3.Cross(diffB, diffA) );

        //Offset poly index template
        for (int i = 0; i < currentPoly_Triangles.Count; i++)
            currentPoly_Triangles[i] = i + createdMesh.vertexCount;

        Vector3[] totalVertices = createdMesh.vertices;
        Vector3[] totalNormals  = createdMesh.normals;
        int[] totalTriangles    = createdMesh.triangles;

        //Get existing vertices, normals and indices
        //and add the newly created poly.
        List<Vector3> vertexList = new List<Vector3>(totalVertices);
        vertexList.AddRange(currentPoly_Vertices);
        List<int> triangleList = new List<int>(totalTriangles);
        triangleList.AddRange(currentPoly_Triangles);
        List<Vector3> normalList = new List<Vector3>(totalNormals);

        normalList.Add(normal);
        normalList.Add(normal);
        normalList.Add(normal);

        //Assign new Lists to Mesh
        createdMesh.Clear();
        createdMesh.SetVertices(vertexList);
        createdMesh.SetNormals(normalList);
        createdMesh.SetTriangles(triangleList, 0);
        
                
        //Clear temporary lists
        currentPoly_Vertices.Clear();
        currentPoly_Normals.Clear();
    }

    private Vector3 snapToNearest(Vector3 position, float snapRadius)
    {
        float minDistance = Mathf.Infinity;
        Vector3 result = position;
        for(int i = 0; i < createdMesh.vertexCount; i++)
        {
            float distance = Vector3.Distance(createdMesh.vertices[i], position);
            if (distance < snapRadius && distance < minDistance)
            {
                result = createdMesh.vertices[i];
                minDistance = distance;
            }       
        }

        return result;
    }

    void handleInput()
    {
        Vector3 nextVertex = cursor3d.transform.position;
        //If snapping = on
        if (controller.gripped)
        {
            nextVertex = snapToNearest(nextVertex, 0.1f);


            if (nextVertex != cursor3d.transform.position)
            {
                if (snapCursor3d == null)
                {
                    snapCursor3d = Instantiate(cursor3d, nextVertex, Quaternion.identity);
                    (snapCursor3d.GetComponent<MeshRenderer>()).material.color = new Color(1, 0, 0);
                }

            }
            else
            {
                if (snapCursor3d != null)
                    Destroy(snapCursor3d);
            }
        }
        else
        {
            if (snapCursor3d != null)
                Destroy(snapCursor3d);
        }



        if (controller.triggerPressed && !controller_triggerPreviousFrame)
        {
            currentPoly_Vertices.Add(nextVertex);
        }
            


    }
    


}
