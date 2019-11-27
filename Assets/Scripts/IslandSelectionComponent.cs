using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class IslandSelectionComponent : MonoBehaviour
{

    private List<Hand> touchingHandList; // List of hands, that are currently touching the handle.
    private Hand currentHand; // The hand that is currently using the handle.

    private List<Touch> currentTouches;


    private void Start()
    {
        currentTouches = new List<Touch>();

        // Subscribe input methods
        IslandVizInteraction.Instance.OnControllerEnter += OnControllerEnterEvent;
        IslandVizInteraction.Instance.OnControllerExit += OnControllerExitEvent;
        IslandVizInteraction.Instance.OnControllerTriggerDown += OnControllerTriggerPressed;
        IslandVizInteraction.Instance.OnControllerTriggerUp += OnControllerTriggerReleased;
    }



    // ################
    // Interaction - Event Handling
    // ################

    #region Interaction - Event Handling

    private void OnControllerEnterEvent(Collider collider, Hand hand)
    {
        Debug.Log("OnControllerEnterEvent " + collider.name);
        if (ColliderIsIsland(collider) && !TouchAlreadyExists(collider, hand))
        {
            currentTouches.Add(new Touch(collider, hand));
            Debug.Log("Added Touch ...");
        }        
    }

    private void OnControllerExitEvent(Collider collider, Hand hand)
    {
        if (ColliderIsIsland(collider) && TouchAlreadyExists(collider, hand))
        {
            currentTouches.Remove(FindTouch(collider, hand));
        }
    }

    private void OnControllerTriggerPressed(Hand hand)
    {
        if (FindFirstTouchWithHand(hand) != null)
        {
            SelectIsland(FindFirstTouchWithHand(hand).Collider);
            Debug.Log("Island Selected");
        }
    }

    private void OnControllerTriggerReleased(Hand hand)
    {
        // TODO
    }

    #endregion


    private void SelectIsland (Collider collider)
    {
        if (IslandVizVisualization.Instance.CurrentZoomLevel == ZoomLevel.Far && collider.GetComponent<IslandGO>())
        {
            collider.GetComponent<IslandGO>().Select();
        }
        else if (IslandVizVisualization.Instance.CurrentZoomLevel == ZoomLevel.Medium && collider.GetComponent<Region>())
        {
            collider.GetComponent<Region>().getParentIsland().Select();
        }
    }


    // ################
    // Helper Functions
    // ################

    private bool TouchAlreadyExists (Collider collider, Hand hand)
    {
        return FindTouch(collider, hand) != null;
    }

    private Touch FindTouch (Collider collider, Hand hand)
    {
        foreach (var touch in currentTouches)
        {
            if (touch.Collider == collider && touch.Hand == hand)
            {
                return touch;
            }
        }
        return null;
    }

    private Touch FindFirstTouchWithHand(Hand hand)
    {
        foreach (var touch in currentTouches)
        {
            if (touch.Hand == hand)
            {
                return touch;
            }
        }
        return null;
    }

    private bool ColliderIsIsland (Collider collider)
    {
        return collider.GetComponent<IslandGO>() || collider.GetComponent<Region>() || collider.GetComponent<Building>();
    }


    public class Touch
    {
        public Collider Collider;
        public Hand Hand;

        public Touch (Collider collider, Hand hand)
        {
            Collider = collider;
            Hand = hand;
        }
    }
}
