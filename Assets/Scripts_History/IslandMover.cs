using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexLayout.Basics;

public class IslandMover : MonoBehaviour {
    private int pos;
    private bool moving;
    private Vector3[] positions = {new Vector3(4f,0f,4f), new Vector3(4f,0f,-4f), new Vector3(-4f, 0f, -4f), new Vector3(-4f, 0f, 4f) };

	// Use this for initialization
	void Start () {
        pos = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("right")&& !moving)
        {
            moving = true;
            pos = (pos + 1) % 4;
            Vector3 speedDir = positions[pos] - transform.position;
            speedDir = speedDir.normalized;
            GetComponent<Rigidbody>().velocity = 1.5f * speedDir;
        }
        if(moving && (positions[pos] - transform.position).magnitude < 0.1)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            moving = false;
        }

    }
}
