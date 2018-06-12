using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTestScript : MonoBehaviour {

    public Transform parentTrans;
    public Vector3 originalLocalPosition;
    public Vector3 originalLocalScale;
    public Quaternion originalLocalRotation;

    public bool simulateParent = false;

    // Use this for initialization
    void Start () {

        parentTrans = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale    = transform.localScale;

	}
	
	// Update is called once per frame
	void Update () {
        if (simulateParent)
        {
            transform.parent = parentTrans;
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            transform.localScale = originalLocalScale;
        }
        
	}
}
