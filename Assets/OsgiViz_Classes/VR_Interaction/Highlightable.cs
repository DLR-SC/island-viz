using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

//Currently using the Valve.VR.InteractionSystem.Hand to trigger the highlighting. Feel free to use another trigger.

//TODO: Merge all Submeshes into a single Mesh since we only need to apply one material => one Mesh is enough
public class Highlightable : MonoBehaviour {

    public Material highlightMaterial;
    private GameObject highlightedGOVersion;

	// Use this for initialization
	void Start () {

        if (highlightMaterial == null)
            highlightMaterial = (Material)Resources.Load("Materials/WireFrame");
            //highlightMaterial = (Material)Resources.Load("Materials/Highlight_large");

        highlightedGOVersion = new GameObject(gameObject.name + " - Highlight");
        highlightedGOVersion.tag = "Highlight";
        highlightedGOVersion.transform.SetParent(transform.parent);
        highlightedGOVersion.transform.localPosition = transform.localPosition;
        highlightedGOVersion.transform.localRotation = transform.localRotation;
        highlightedGOVersion.transform.localScale = transform.localScale; //+ new Vector3(0.1f,0f,0.1f);

        MeshFilter mfHighlight = highlightedGOVersion.AddComponent<MeshFilter>();
        MeshFilter mfOriginal = GetComponent<MeshFilter>();
        mfHighlight.mesh = mfOriginal.mesh;

        MeshRenderer mrHighlight = highlightedGOVersion.AddComponent<MeshRenderer>();
        int numSubmeshes = mfHighlight.mesh.subMeshCount;
        Material[] highlightMaterials = new Material[numSubmeshes];
        for (int i = 0; i < numSubmeshes; i++)
            highlightMaterials[i] = highlightMaterial;

        mrHighlight.sharedMaterials = highlightMaterials;

        highlightedGOVersion.SetActive(false);
	}

    public void highlight()
    {
        highlightedGOVersion.SetActive(enabled);
    }

    public void deHighlight()
    {
        if (highlightedGOVersion != null)
        {
            highlightedGOVersion.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Highlightable.deHighlight(): highlightedGOVersion is null!");
        }
    }

    private void OnHandHoverBegin(Hand hand)
    {
        highlight();
    }

    private void OnHandHoverEnd(Hand hand)
    {
        deHighlight();
    }

    void OnDisable()
    {
        deHighlight();
    }



}
