using OsgiViz;
using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// This class is a interaction component creating a interaction area on top of the table whitch can change transform of the visualization. 
/// Most of this comes from the InverseMultiTouchController.cs in Assets/OsgiViz/VR_Interaction/.
/// </summary>
public class InverseMultiTouchInput : AdditionalIslandVizComponent {

    [Header("Settings")]
    public float PivotTransferCutoff = 1.25f;
    public float TranslationMult = 1f;
    public float ScaleMult = 2.0f;
    public float RotationMult = 1f;


    private GameObject mapNavigationArea; // The GameObject which holds the collider.

    private List<Hand> touchingHandList;
    private List<Hand> usingHandList;
    private Vector3 currentTranslationVelocity;
    private bool initiated = false;
    private bool tooltippsDisabled = false;

    // Settings
    private string mapNavigationAreaTag = "MapNavigationArea";



    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Initialize this input component. 
    /// This method is called by the IslandVizInteraction class.
    /// </summary>
    public override IEnumerator Init ()
    {
        IslandVizUI.Instance.UpdateLoadingScreenUI("InverseMultiTouchInput Construction", "");

        // Init GameObject
        mapNavigationArea = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mapNavigationArea.name = mapNavigationAreaTag;
        mapNavigationArea.transform.localScale = new Vector3(1.75f, 0.15f, 1.75f);
        mapNavigationArea.transform.position = new Vector3(0f, OsgiViz.Core.GlobalVar.hologramTableHeight, 0f);

        // Remove Renderer & default collider 
        Destroy(mapNavigationArea.GetComponent<MeshRenderer>());
        Destroy(mapNavigationArea.GetComponent<CapsuleCollider>());

        // Init Collider
        MeshCollider meshCollider = mapNavigationArea.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        // Physics Settings
        mapNavigationArea.tag = mapNavigationAreaTag;
        mapNavigationArea.layer = LayerMask.NameToLayer("Interaction");
        
        // Subscribe input methods
        IslandVizInteraction.Instance.OnControllerEnter += OnControllerEnterEvent;
        IslandVizInteraction.Instance.OnControllerExit += OnControllerExitEvent;
        IslandVizInteraction.Instance.OnControllerButtonEvent += OnControllerTriggerPressedEvent;
        IslandVizVisualization.Instance.OnTableHeightChanged += OnTableHeightChangedEvent;

        touchingHandList = new List<Hand>();
        usingHandList = new List<Hand>();
        currentTranslationVelocity = new Vector3(0f, 0f, 0f);
        initiated = true;

        yield return null;
    }

    #endregion


    // ################
    // Interaction - Event Handling
    // ################

    #region Interaction - Event Handling

    private void OnControllerEnterEvent(Collider collider, Hand hand)
    {
        if (collider.tag == mapNavigationAreaTag && !touchingHandList.Contains(hand))
        {
            touchingHandList.Add(hand);

            if (!tooltippsDisabled)
            {
                EnableTooltips(hand);
            }
        }
    }

    private void OnControllerExitEvent(Collider collider, Hand hand)
    {
        if (collider.tag == mapNavigationAreaTag && touchingHandList.Contains(hand))
        {
            touchingHandList.Remove(hand);

            if (!tooltippsDisabled)
            {
                DisableTooltips(hand);
            }
        }
    }

    private void OnControllerTriggerPressedEvent (IslandVizInteraction.Button button, IslandVizInteraction.PressType type, Hand hand)
    {
        if (button != IslandVizInteraction.Button.Trigger)
        {
            return;
        }

        if (type == IslandVizInteraction.PressType.PressDown)
        {
            if (!usingHandList.Contains(hand) && touchingHandList.Contains(hand))
            {
                usingHandList.Add(hand);

                if (!tooltippsDisabled)
                {
                    tooltippsDisabled = true;
                    DisableTooltips();
                }
            }
        }
        else if (type == IslandVizInteraction.PressType.PressUp)
        {
            if (usingHandList.Contains(hand))
            {
                usingHandList.Remove(hand);
            }
        }
    }

    #endregion



    // ################
    // Interaction - Event Handling
    // ################

    #region Interaction - Event Handling

    /// <summary>
    /// Called by IslandVizInteraction when table height was changed.
    /// </summary>
    /// <param name="newHeight">New height of the table.</param>
    public void OnTableHeightChangedEvent(float newHeight)
    {
        mapNavigationArea.transform.position = new Vector3(0f, newHeight, 0f);
    }

    #endregion



    // ################
    // Interaction - Movement
    // ################

    #region Interaction - Movement

    //From InverseMultiTouchController.cs

    void Update()
    {
        if (!initiated)
        {
            return;
        }

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
            if (Vector3.Distance(currentPivot, nextPivot) < PivotTransferCutoff)
            {
                Vector3 diffCurrent = origin1 - origin2;
                Vector3 diffNext = nextOrigin1 - nextOrigin2;
                float scalingFactor = diffCurrent.magnitude / diffNext.magnitude;
                scalingFactor = 1.0f / scalingFactor;
                Vector3 scaleRotPivot = new Vector3(currentPivot.x, GlobalVar.hologramTableHeight, currentPivot.z);

                float radCurrent = Mathf.Atan2(diffCurrent.x, diffCurrent.z);
                float radNext = Mathf.Atan2(diffNext.x, diffNext.z);
                float rotationAngle = -Mathf.Rad2Deg * (radNext - radCurrent) * RotationMult;

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
            currentTranslationVelocity -= currentTranslationVelocity * (2f - GlobalVar.CurrentZoom * 5f) * Time.deltaTime;
        }

        currentTranslationVelocity = ClampTranslationVelocityVector(currentTranslationVelocity);
        IslandVizVisualization.Instance.VisualizationRoot.Translate(-currentTranslationVelocity * Time.deltaTime * TranslationMult, Space.World);
    }

    public void RotateAndScale(Vector3 origin, float amountRot, float amountScale)
    {
        // Scale Constraints
        if (GlobalVar.CurrentZoom * amountScale > GlobalVar.MaxZoom
            || GlobalVar.CurrentZoom * amountScale < GlobalVar.MinZoom)
        {
            amountScale = 1.0f;
        }

        Vector3 scaleVec = new Vector3(amountScale, amountScale, amountScale);
        Helperfunctions.scaleFromPivot(IslandVizVisualization.Instance.VisualizationRoot, origin, scaleVec);
        IslandVizVisualization.Instance.VisualizationRoot.RotateAround(origin, Vector3.up, -amountRot);

        #region Update due to scale change
        GlobalVar.CurrentZoom = IslandVizVisualization.Instance.VisualizationRoot.localScale.x;
        IslandVizVisualization.Instance.OnVisualizationScaleChanged();
        #endregion
    }

    #endregion



    // ################
    // Tooltips
    // ################

    #region Tooltips

    private void EnableTooltips(Hand hand)
    {
        ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger, "HOLD and MOVE to navigate");
    }

    private void DisableTooltips(Hand hand)
    {
        ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    private void DisableTooltips()
    {
        ControllerButtonHints.HideTextHint(Player.instance.leftHand, EVRButtonId.k_EButton_SteamVR_Trigger);
        ControllerButtonHints.HideTextHint(Player.instance.rightHand, EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    #endregion


    // ################
    // Helper Functions
    // ################

    #region Helper Functions

    // Tracking issues and the velocity can cause the controller velocity to spike and cause problems, so we clamp the values.
    private Vector3 ClampTranslationVelocityVector(Vector3 vector)
    {
        return new Vector3(Mathf.Clamp(vector.x, -3f, 3f), 0f, Mathf.Clamp(vector.z, -3f, 3f));
    }

    #endregion
}
