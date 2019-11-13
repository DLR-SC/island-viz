using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class TableHeightAdjuster : AdditionalIslandVizComponent
{
    public Material Material;


    private GameObject handle;

    private List<Hand> touchingHandList;
    private Hand currentHand;

    private bool initiated = false;


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
        IslandVizUI.Instance.UpdateLoadingScreenUI("TableHeightAdjuster Construction", "");

        // Init GameObject
        handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "TableHeightAdjusterGrip";
        handle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        handle.transform.position = new Vector3(-0.4f, GlobalVar.hologramTableHeight, -1.1f);
        handle.GetComponent<MeshRenderer>().material = Material;

        // Modify Collider
        handle.GetComponent<BoxCollider>().isTrigger = true;

        // Physics Settings
        handle.tag = "TableHeightAdjusterGrip";
        handle.layer = LayerMask.NameToLayer("MapNavigationArea"); // TODO ?

        // Subscribe input methods
        IslandVizInteraction.Instance.OnControllerEnter += OnControllerEnterEvent;
        IslandVizInteraction.Instance.OnControllerExit += OnControllerExitEvent;
        IslandVizInteraction.Instance.OnControllerTriggerDown += OnControllerTriggerPressed;
        IslandVizInteraction.Instance.OnControllerTriggerUp += OnControllerTriggerReleased;

        touchingHandList = new List<Hand>();
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
        if (collider.tag == "TableHeightAdjusterGrip" && !touchingHandList.Contains(hand))
        {
            touchingHandList.Add(hand);
        }
    }

    private void OnControllerExitEvent(Collider collider, Hand hand)
    {
        if (collider.tag == "TableHeightAdjusterGrip" && touchingHandList.Contains(hand))
        {
            touchingHandList.Remove(hand);
        }
    }

    private void OnControllerTriggerPressed(Hand hand)
    {
        if (currentHand == null && touchingHandList.Contains(hand))
        {
            StartCoroutine(HeightAdjustment(hand));
        }
    }

    private void OnControllerTriggerReleased(Hand hand)
    {
        if (currentHand == hand)
        {
            currentHand = null; // This stops the coroutine.
        }
    }

    #endregion


    IEnumerator HeightAdjustment (Hand hand)
    {
        currentHand = hand;
        float newHeight;

        while (currentHand != null)
        {
            newHeight = GlobalVar.hologramTableHeight + (hand.GetTrackedObjectVelocity().y * Time.fixedDeltaTime);

            IslandVizVisualization.Instance.UpdateTableHight(newHeight);

            handle.transform.position = new Vector3(handle.transform.position.x, newHeight, handle.transform.position.z);

            yield return new WaitForFixedUpdate();
        }
    }
}
