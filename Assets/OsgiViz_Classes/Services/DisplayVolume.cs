using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayVolume : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void toggleGameObject()
    {
        this.gameObject.active = !this.gameObject.active;
    }
}
