using OsgiViz;
using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class InverseMultiTouchInput : AdditionalIslandVizComponent {

    private GameObject mapNavigationArea;
    private MeshCollider meshCollider;

    private InverseMultiTouchController inverseMultiTouchController;
    private List<Hand> touchingHandList;
    private List<Hand> usingHandList;
    private Vector3 currentTranslationVelocity = new Vector3(0f, 0f, 0f);
    private float pivotTransferCutoff = 1.25f;
    private float translationSpeedCutoff = 0.5f;
    private float effectivePivotTransferCutoff;
    private float effectiveTranslationSpeedCutoff;
    private float translationMult = 1f;
    private float scaleMult = 2.0f;
    private float rotationMult = 1f;
    public float drag;
    private float effectiveDrag;



    // Use this for initialization
    public override void Init () {
        // Init GameObject
        mapNavigationArea = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mapNavigationArea.name = "MapNavigationArea";
        mapNavigationArea.tag = "MapNavigationArea";
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
        //inverseMultiTouchController = mapNavigationArea.AddComponent<InverseMultiTouchController>();
        //inverseMultiTouchController.drag = 0f; // 7.5f;

        // Subscribe input function
        IslandVizInteraction.Instance.OnControllerEnter += OnControllerEnterEvent;
        IslandVizInteraction.Instance.OnControllerExit += OnControllerExitEvent;
        IslandVizInteraction.Instance.OnControllerTriggerDown += OnControllerTriggerPressed;
        IslandVizInteraction.Instance.OnControllerTriggerUp += OnControllerTriggerReleased;

        touchingHandList = new List<Hand>();
        usingHandList = new List<Hand>();
        effectivePivotTransferCutoff = pivotTransferCutoff;
        effectiveTranslationSpeedCutoff = translationSpeedCutoff;
        effectiveDrag = drag;

    }


    private void OnControllerEnterEvent(Collider collider, Hand hand)
    {
        if (collider.tag == "MapNavigationArea" && !touchingHandList.Contains(hand))
        {
            touchingHandList.Add(hand);
        }
    }

    private void OnControllerExitEvent(Collider collider, Hand hand)
    {
        if (collider.tag == "MapNavigationArea" && touchingHandList.Contains(hand))
        {
            touchingHandList.Remove(hand);
        }
    }

    private void OnControllerTriggerPressed (Hand hand)
    {
        if (touchingHandList.Contains(hand))
        {
            usingHandList.Add(hand);
        }
    }

    private void OnControllerTriggerReleased(Hand hand)
    {
        if (usingHandList.Contains(hand))
        {
            usingHandList.Remove(hand);
        }
    }



    IEnumerator Movement ()
    {
        yield return null;
    }


    // Update is called once per frame
    void Update()
    {
        //Identify which controllers are using the object from the touching list
        
        //Handle Movement
        if (usingHandList.Count == 1)
        {
            currentTranslationVelocity = -usingHandList[0].GetTrackedObjectVelocity();
            UpdateTranslation(false);
        }
        else if (usingHandList.Count == 2)
        {
            //current pivot
            Vector3 origin1 = usingHandList[0].gameObject.transform.GetChild(0).position;
            Vector3 origin2 = usingHandList[1].gameObject.transform.GetChild(0).position;
            Vector3 currentPivot = (origin1 + origin2) / 2f;

            //next pivot
            Vector3 controllerVelocity1 = usingHandList[0].GetTrackedObjectVelocity();
            Vector3 controllerVelocity2 = usingHandList[1].GetTrackedObjectVelocity();
            Vector3 nextOrigin1 = controllerVelocity1 * Time.deltaTime + origin1;
            Vector3 nextOrigin2 = controllerVelocity2 * Time.deltaTime + origin2;
            Vector3 nextPivot = (nextOrigin1 + nextOrigin2) / 2f;

            //For an ideal scale/rotate gesture the pivot would stay the same. For real world applications
            //the pivotTransferCutoff allows for some sloppiness in the gesture
            if (Vector3.Distance(currentPivot, nextPivot) < effectivePivotTransferCutoff)
            {
                Vector3 diffCurrent = origin1 - origin2;
                Vector3 diffNext = nextOrigin1 - nextOrigin2;
                float scalingFactor = diffCurrent.magnitude / diffNext.magnitude;
                scalingFactor = 1.0f / scalingFactor;
                Vector3 scaleRotPivot = new Vector3(currentPivot.x, GlobalVar.hologramTableHeight, currentPivot.z);

                float radCurrent = Mathf.Atan2(diffCurrent.x, diffCurrent.z);
                float radNext = Mathf.Atan2(diffNext.x, diffNext.z);
                float rotationAngle = -Mathf.Rad2Deg * (radNext - radCurrent) * rotationMult;

                RotateAndScale(scaleRotPivot, rotationAngle, scalingFactor);
            }
        }
        else
        {
            UpdateTranslation(true);
        }

        //Shader.SetGlobalVector(clippingCenterShaderID, IslandVizVisualization.Instance.Table.transform.position); // TODO: nötig?
                                                                                                                  //Shader.SetGlobalFloat(hologramScaleShaderID, GlobalVar.CurrentZoomLevel * 0.8f);
    }



    private void UpdateTranslation(bool useDrag)
    {
        #region translation constraint
        // TODO
        
        #endregion

        if (useDrag && currentTranslationVelocity != Vector3.zero)
        {
            currentTranslationVelocity -= currentTranslationVelocity * (2f - GlobalVar.CurrentZoomLevel * 5f) * Time.deltaTime;
        }

        currentTranslationVelocity = ClampTranslationVelocityVector(currentTranslationVelocity);
        IslandVizVisualization.Instance.VisualizationRoot.Translate(-currentTranslationVelocity * Time.deltaTime * translationMult, Space.World);
    }

    public void RotateAndScale(Vector3 origin, float amountRot, float amountScale)
    {
        // Scale Constraints
        if (GlobalVar.CurrentZoomLevel * amountScale > GlobalVar.MaxZoomLevel
            || GlobalVar.CurrentZoomLevel * amountScale < GlobalVar.MinZoomLevel)
        {
            amountScale = 1.0f;
        }

        Vector3 scaleVec = new Vector3(amountScale, amountScale, amountScale);
        Helperfunctions.scaleFromPivot(IslandVizVisualization.Instance.VisualizationRoot, origin, scaleVec);
        IslandVizVisualization.Instance.VisualizationRoot.RotateAround(origin, Vector3.up, -amountRot);

        #region Update due to scale change
        GlobalVar.CurrentZoomLevel = IslandVizVisualization.Instance.VisualizationRoot.localScale.x;
        //mainLight.range = originalLightRange * GlobalVar.CurrentZoomLevel;
        effectiveDrag = drag * 1.0f / GlobalVar.CurrentZoomLevel;
        effectiveTranslationSpeedCutoff = translationSpeedCutoff * GlobalVar.CurrentZoomLevel;
        effectivePivotTransferCutoff = pivotTransferCutoff * GlobalVar.CurrentZoomLevel;
        #endregion

    }


    // Tracking issues and the drag can cause the controller velocity to spike and cause problems, so we clamp the values.
    private Vector3 ClampTranslationVelocityVector(Vector3 vector)
    {
        return new Vector3(Mathf.Clamp(vector.x, -3f, 3f), 0f, Mathf.Clamp(vector.z, -3f, 3f));
    }

}
