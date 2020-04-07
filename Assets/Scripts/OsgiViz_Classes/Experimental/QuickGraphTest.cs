using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;

public class customVertex
{
    private Vector3 position;
    private string name;

    public customVertex(string n)
    {
        name = n;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public void setPosition(Vector3 newPos)
    {
        position = newPos;
    }

}

public class customEdge : Edge<customVertex>
{
    private string value;

    public customEdge(customVertex source, customVertex target, string v) : base(source, target)
    {
        value = v;
    }
}

public class QuickGraphTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

        BidirectionalGraph<customVertex, customEdge> graph = new BidirectionalGraph<customVertex, customEdge>();
        customVertex v1 = new customVertex("first Vertex");
        customVertex v2 = new customVertex("second Vertex");
        customEdge e1 = new customEdge(v1, v2, "first Edge");
        customEdge e2 = new customEdge(v1, v2, "second Edge");

        graph.AddVerticesAndEdge(e1);
        graph.AddVerticesAndEdge(e2);

        //QuickGraph.Algorithms.

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
