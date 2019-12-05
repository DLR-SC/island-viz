using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class IslandSelectionComponent : MonoBehaviour
{
    public static IslandSelectionComponent Instance;

    private List<Hand> touchingHandList; // List of hands, that are currently touching the handle.
    private Hand currentHand; // The hand that is currently using the handle.

    private List<Touch> currentTouches;

    private List<IslandGO> currentSelectedIslands;
    private List<Region> currentSelectedRegion;
    private List<Building> currentSelectedBuildings;


    private void Start()
    {
        Instance = this;

        currentTouches = new List<Touch>();

        currentSelectedIslands = new List<IslandGO>();
        currentSelectedRegion = new List<Region>();
        currentSelectedBuildings = new List<Building>();

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
            Select(FindFirstTouchWithHand(hand).Collider);
            Debug.Log("Island Selected");
        }
    }

    private void OnControllerTriggerReleased(Hand hand)
    {
        // TODO
    }

    #endregion


    // ################
    // Island Selection
    // ################

    #region Island Selection

    public void Select (Collider collider)
    {
        if (collider.GetComponent<IslandGO>()) // IslandVizVisualization.Instance.CurrentZoomLevel == ZoomLevel.Far && 
        {
            //SelectIsland(collider.GetComponent<IslandGO>());
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Select, true);
        }
        else if (collider.GetComponent<Region>()) // IslandVizVisualization.Instance.CurrentZoomLevel == ZoomLevel.Medium &&
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), IslandVizInteraction.SelectionType.Select, true);
        }
        else if (collider.GetComponent<Building>()) // IslandVizVisualization.Instance.CurrentZoomLevel == ZoomLevel.Near && 
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), IslandVizInteraction.SelectionType.Select, true);
        }
    }

    public void Deselect(Collider collider)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Select, false);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), IslandVizInteraction.SelectionType.Select, false);
        }
        else if (collider.GetComponent<Building>())
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), IslandVizInteraction.SelectionType.Select, false);
        }
    }

    public void SelectIsland (IslandGO island)
    {
        DeselectAllCurrentIslands(island);

        if (!currentSelectedIslands.Contains(island))
        {
            IslandVizInteraction.Instance.OnIslandSelect(island, IslandVizInteraction.SelectionType.Select, true);
            currentSelectedIslands.Add(island);
        }
    }

    public void SelectIslands(List<IslandGO> islands)
    {
        DeselectAllCurrentIslands(islands);

        foreach (var island in islands)
        {
            if (!currentSelectedIslands.Contains(island))
            {
                IslandVizInteraction.Instance.OnIslandSelect(island, IslandVizInteraction.SelectionType.Select, true);
                currentSelectedIslands.Add(island);
            }
        }
    }

    #endregion




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


    private void DeselectAllCurrentIslands()
    {
        if (currentSelectedIslands.Count > 0)
        {
            for (int i = currentSelectedIslands.Count - 1; i >= 0; i--)
            {
                IslandVizInteraction.Instance.OnIslandSelect(currentSelectedIslands[i], IslandVizInteraction.SelectionType.Select, false);
                currentSelectedIslands.Remove(currentSelectedIslands[i]);
            }
        }
    }

    private void DeselectAllCurrentIslands (IslandGO exception)
    {
        if (currentSelectedIslands.Count > 0)
        {
            for (int i = currentSelectedIslands.Count - 1; i >= 0; i--)
            {             
                if (currentSelectedIslands[i] != exception)
                {
                    IslandVizInteraction.Instance.OnIslandSelect(currentSelectedIslands[i], IslandVizInteraction.SelectionType.Select, false);
                    currentSelectedIslands.Remove(currentSelectedIslands[i]);
                }                
            }
        }
    }

    private void DeselectAllCurrentIslands(List<IslandGO> exceptions)
    {
        if (currentSelectedIslands.Count > 0)
        {
            for (int i = currentSelectedIslands.Count - 1; i >= 0; i--)
            {
                if (!exceptions.Contains(currentSelectedIslands[i]))
                {
                    IslandVizInteraction.Instance.OnIslandSelect(currentSelectedIslands[i], IslandVizInteraction.SelectionType.Select, false);
                    currentSelectedIslands.Remove(currentSelectedIslands[i]);
                }
            }
        }
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
