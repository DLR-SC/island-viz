using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// This class is a interaction component creating a handle which can change the height of the table.
/// </summary>
public class TableHeightAdjuster : AdditionalIslandVizComponent
{
    public Material Material; // The Material of the generated handle. 

    private GameObject handle; // The GameObject of the generated handle.

    private List<Hand> touchingHandList; // List of hands, that are currently touching the handle.
    private Hand currentHand; // The hand that is currently using the handle.

    private bool initiated = false; // Set to TRUE at the end of the Init method. 


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
        handle.name = "TableHeightAdjusterGrip"; // This is just for a nice looking scene :)
        handle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        handle.transform.position = new Vector3(-0.4f, GlobalVar.hologramTableHeight, -1.1f);
        handle.GetComponent<MeshRenderer>().material = Material;

        // Modify Collider
        handle.GetComponent<BoxCollider>().isTrigger = true;

        // Physics Settings
        handle.tag = "TableHeightAdjusterGrip"; // Set a tag, so we can later recognize the handle in the ControllerEnterEvents.
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
            currentHand = null; // This stops the HeightAdjustment coroutine.
        }
    }

    #endregion

    /// <summary>
    /// While the trigger button is pressed, the y-movement of the hand is translated to the height of the table and the height of the handle.
    /// </summary>
    /// <param name="hand">The hand that touched the handles and pressed the trigger button.</param>
    /// <returns></returns>
    IEnumerator HeightAdjustment (Hand hand)
    {
        currentHand = hand; // When the trigger button is released, this gets set to null.
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
