using OsgiViz;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseMultiTouchInput : AdditionalIslandVizComponent {

    private GameObject mapNavigationArea;
    private MeshCollider meshCollider;

    private InverseMultiTouchController inverseMultiTouchController;

    // Use this for initialization
    public override void Init () {
        // Init GameObject
        mapNavigationArea = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mapNavigationArea.name = "MapNavigationArea";
        mapNavigationArea.transform.localScale = new Vector3(1.75f, 0.15f, 1.75f);
        mapNavigationArea.transform.position = new Vector3(0f, OsgiViz.Core.GlobalVar.hologramTableHeight, 0f);

        // Remove Renderer & default collider 
        Destroy(mapNavigationArea.GetComponent<MeshRenderer>());
        Destroy(mapNavigationArea.GetComponent<CapsuleCollider>());

        // Init Collider
        meshCollider = mapNavigationArea.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        // Physics Settings
        mapNavigationArea.tag = "MapNavigationArea";
        mapNavigationArea.layer = LayerMask.NameToLayer("MapNavigationArea"); // TODO ?

        // Add InverseMultiTouchController Component
        inverseMultiTouchController = mapNavigationArea.AddComponent<InverseMultiTouchController>();
        inverseMultiTouchController.drag = 0f; // 7.5f;
    }
	
}
