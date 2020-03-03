﻿using OsgiViz;
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
public class InverseMultiRaycastInput : AdditionalIslandVizComponent
{

    [Header("Settings")]
    public float PivotTransferCutoff = 1.25f;
    public float TranslationMult = 1f;
    public float ScaleMult = 2.0f;
    public float RotationMult = 1f;
    
    private GameObject mapNavigationArea; // The GameObject which holds the collider.

    private Vector3 currentTranslationVelocity;
    private bool initiated = false;
    private bool tooltipsDisabled = false;
    private bool selected = false;
    private List<Hand> currentHands;
    private Vector3 lastHitPosition;

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
    public override IEnumerator Init()
    {
        IslandVizUI.Instance.UpdateLoadingScreenUI("InverseMultiTouchInput Construction", "");

        // Init GameObject
        mapNavigationArea = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mapNavigationArea.name = mapNavigationAreaTag;
        mapNavigationArea.transform.localScale = new Vector3(1.75f, 0.1f, 1.75f);
        mapNavigationArea.transform.position = new Vector3(0f, OsgiViz.Core.GlobalVar.hologramTableHeight - 0.1f, 0f);

        // Remove Renderer & default collider 
        Destroy(mapNavigationArea.GetComponent<MeshRenderer>());
        Destroy(mapNavigationArea.GetComponent<CapsuleCollider>());

        // Init Collider
        MeshCollider meshCollider = mapNavigationArea.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        //meshCollider.isTrigger = true;

        // Physics Settings
        mapNavigationArea.tag = mapNavigationAreaTag;
        mapNavigationArea.layer = LayerMask.NameToLayer("Visualization");

        // Subscribe input events
        IslandVizInteraction.Instance.OnOtherSelected += OnRaycastSelectionEvent;
        IslandVizVisualization.Instance.OnTableHeightChanged += OnTableHeightChangedEvent;

        currentHands = new List<Hand>();
        currentTranslationVelocity = Vector3.zero;
        lastHitPosition = Vector3.zero;
        initiated = true;

        yield return null;
    }

    #endregion


    // ################
    // Interaction - Event Handling
    // ################

    #region Interaction - Event Handling

    public void OnRaycastSelectionEvent (GameObject go, Hand hand, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (go != mapNavigationArea)
        {
            return;
        }
        // Current hand und selection handling.
        if (selectionType == IslandVizInteraction.SelectionType.Select)
        {
            this.selected = selected;
            if (selected && hand != null && !currentHands.Contains(hand))
            {
                currentHands.Add(hand);
            }
            else if (!selected && hand != null && currentHands.Contains(hand))
            {
                currentHands.Remove(hand);
            }
        }
        // Tooltips
        if (selectionType == IslandVizInteraction.SelectionType.Highlight)
        {
            if (selected && !tooltipsDisabled)
            {
                //EnableTooltips(hand);  
            }
            else if (!selected && !tooltipsDisabled)
            {
                //DisableTooltips(hand);
                tooltipsDisabled = true;
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
        if (selected && currentHands.Count == 1)
        {
            Debug.Log("InverseMultiRaycastInput!!");

            int handID = RaycastSelection.Instance.GetHandID(currentHands[0]);
            Vector3 hitPosition = RaycastSelection.Instance.GetCurrentHit(handID).point;

            if (lastHitPosition == Vector3.zero) // we did not hit anything last frame.
            {
                lastHitPosition = hitPosition;
                UpdateTranslation(true);
            }
            else
            {
                currentTranslationVelocity = (lastHitPosition - hitPosition) * Time.deltaTime;
                UpdateTranslation(false);
                lastHitPosition = hitPosition;
            }

            Debug.Log("Point: " + RaycastSelection.Instance.GetCurrentHit(handID).point);
            Debug.Log("Velocity: " + currentTranslationVelocity);
            
        }
        //else if (usingHandList.Count == 2)
        //{
        //    //current pivot
        //    Vector3 origin1 = usingHandList[0].gameObject.transform.GetChild(0).position;
        //    Vector3 origin2 = usingHandList[1].gameObject.transform.GetChild(0).position;
        //    Vector3 currentPivot = (origin1 + origin2) / 2f;

        //    //next pivot
        //    Vector3 controllerVelocity1 = usingHandList[0].GetTrackedObjectVelocity();
        //    Vector3 controllerVelocity2 = usingHandList[1].GetTrackedObjectVelocity();
        //    Vector3 nextOrigin1 = controllerVelocity1 * Time.deltaTime + origin1;
        //    Vector3 nextOrigin2 = controllerVelocity2 * Time.deltaTime + origin2;
        //    Vector3 nextPivot = (nextOrigin1 + nextOrigin2) / 2f;

        //    //For an ideal scale/rotate gesture the pivot would stay the same. For real world applications
        //    //the pivotTransferCutoff allows for some sloppiness in the gesture
        //    if (Vector3.Distance(currentPivot, nextPivot) < PivotTransferCutoff)
        //    {
        //        Vector3 diffCurrent = origin1 - origin2;
        //        Vector3 diffNext = nextOrigin1 - nextOrigin2;
        //        float scalingFactor = diffCurrent.magnitude / diffNext.magnitude;
        //        scalingFactor = 1.0f / scalingFactor;
        //        Vector3 scaleRotPivot = new Vector3(currentPivot.x, GlobalVar.hologramTableHeight, currentPivot.z);

        //        float radCurrent = Mathf.Atan2(diffCurrent.x, diffCurrent.z);
        //        float radNext = Mathf.Atan2(diffNext.x, diffNext.z);
        //        float rotationAngle = -Mathf.Rad2Deg * (radNext - radCurrent) * RotationMult;

        //        RotateAndScale(scaleRotPivot, rotationAngle, scalingFactor);
        //    }
        //}
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
        IslandVizVisualization.Instance.Visualization.Translate(-currentTranslationVelocity * Time.deltaTime * TranslationMult, Space.World);
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
        Helperfunctions.scaleFromPivot(IslandVizVisualization.Instance.Visualization, origin, scaleVec);
        IslandVizVisualization.Instance.Visualization.RotateAround(origin, Vector3.up, -amountRot);

        #region Update due to scale change
        GlobalVar.CurrentZoom = IslandVizVisualization.Instance.Visualization.localScale.x;
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