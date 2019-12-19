using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;
using Assets;


public class DockPlacer : MonoBehaviour {
    public float dockDist = 3f;

    private float radius = 5f;
    private GameObject importDock;
    private GameObject exportDock;
   
	// Use this for initialization
	void Start () {
        importDock = gameObject.transform.Find("ImportDock").gameObject;
        exportDock = gameObject.transform.Find("ExportDock").gameObject;

        importDock.GetComponent<Renderer>().material.color = Color.green;
        exportDock.GetComponent<Renderer>().material.color = Color.red;

        importDock.SetActive(false);
        exportDock.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetDockPositions(float newRadius)
    {
        if(newRadius != radius)
        {
            radius = newRadius;
            exportDock.transform.localPosition = new Vector3(radius, Constants.dockYPos, 0f);
            importDock.transform.localPosition = SecondDockPosition();
        }
        if (!exportDock.activeSelf)
        {
            exportDock.SetActive(true);
            importDock.SetActive(true);
        }
    }

    private Vector3 SecondDockPosition()
    {
        float phi = dockDist / radius;
        float otherX = radius * Mathf.Cos(phi);
        float otherZ = radius * Mathf.Sin(phi);

        return new Vector3(otherX, Constants.dockYPos, otherZ);
    }
}
