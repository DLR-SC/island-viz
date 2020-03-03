using OsgiViz;
using OsgiViz.Unity.Island;
using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// This component enables the user to select islands via a SphereCast. Additionally a laser beam is visualizing the SphereCast.
/// </summary>
public class RaycastSelection : AdditionalIslandVizComponent
{
    public static RaycastSelection Instance { get; private set; }

    // ################
    // Public - Unity
    // ################

    public Hand[] Hands; // Attach both or only one hand in the editor.
    public RayMode Mode; // This changes the angle of the ray. Choose whatever you prever.
    public Material LaserMaterial; // The material of the laser pointer visual.

    [Tooltip("Should tooltips be shown on start. Tooltipps will stay until the touchpad was touched.")]
    public bool ShowTooltips = true;

    // ################
    // Private
    // ################

    private int laserLayerMask = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask. -> Only use 8th layer.
    private float laserLength = 3f; // The length of the SphereCast and the laser beam.
    private float laserThickness = 0.01f; // The thickness of the SphereCast and the laser beam.

    // Every field in these arrays belongs to one hand, i.e. the first hand always stores the values in the 0th field and so on.
    private RaycastHit[] currentHits; // Current RaycastHits of the hands.
    private Vector3[] currentForwards; // Current forward vetors of the hands.
    private Collider[] hittingColliders; // Current colliders the hands are hitting with the SphereCast.

    private GameObject[] laserBeamObjs; // The GameObjects of the laser beams.
    private LineRenderer[] lineRenderers; // The LineRenderer components of the laser beam GameObjects.
    
    private bool[] raycastSelectionIsRunning; // True if the touchpad is currently touched.
    private bool[] currentlyHitting; // True if the SphereCast is currently hitting something.

    private bool initiated = false;
    private bool tooltipsDisabled = false;




    // ################
    // Initiation
    // ################

    #region Initiation

    void Awake ()
    {
        Instance = this;
    }

    private void Start() { } // When this has no Start method, you will not be able to disable this in the editor.

    /// <summary>
    /// Initialize this input component. 
    /// This method is called by the IslandVizInteraction class.
    /// </summary>
    public override IEnumerator Init()
    {
        // Initiate arrays.
        laserBeamObjs = new GameObject[Hands.Length];
        lineRenderers = new LineRenderer[Hands.Length];
        currentForwards = new Vector3[Hands.Length];
        currentHits = new RaycastHit[Hands.Length];
        hittingColliders = new Collider[Hands.Length];
        raycastSelectionIsRunning = new bool[] { false, false };
        currentlyHitting = new bool[] { false, false };

        // Subscribe to events.
        IslandVizInteraction.Instance.OnControllerButtonEvent += OnTouchpadEvent;

        // Create laser beams.
        for (int i = 0; i < Hands.Length; i++)
        {
            // Laser beam visuals
            laserBeamObjs[i] = new GameObject();
            laserBeamObjs[i].name = "LaserBeam";
            laserBeamObjs[i].transform.SetParent(Hands[i].transform);
            lineRenderers[i] = laserBeamObjs[i].AddComponent<LineRenderer>();
            lineRenderers[i].material = LaserMaterial;
            lineRenderers[i].startWidth = laserThickness;
            lineRenderers[i].endWidth = laserThickness;
            laserBeamObjs[i].SetActive(false);           
        }

        // Enable Tooltips.
        if (ShowTooltips)
        {
            EnableTooltips();
        }
        else
        {
            tooltipsDisabled = true;
        }

        yield return null;

        initiated = true;
    }

    #endregion


    // ################
    // Input
    // ################

    #region Input

    /// <summary>
    /// This is called when the controller touchpad is pressed, released or touched.
    /// </summary>
    /// <param name="button">The button that was pressed, released or touched.</param>
    /// <param name="type">Weather the button was pressed, released or touched.</param>
    /// <param name="hand">The hand that pressed the button.</param>
    private void OnTouchpadEvent(IslandVizInteraction.Button button, IslandVizInteraction.PressType type, Hand hand)
    {
        if (button != IslandVizInteraction.Button.Touchpad || !initiated)
        {
            return;
        }

        int handID = GetHandID(hand);
        if (handID < 0) // handID is -1 when no hand was found.
        {
            return;
        }

        if (type == IslandVizInteraction.PressType.TouchDown && !raycastSelectionIsRunning[handID])
        {
            raycastSelectionIsRunning[handID] = true;
            StartCoroutine(RaySelection(handID)); // Start the RaySelection Coroutine.

            if (!tooltipsDisabled) // Tooltips are only visible until first use.
            {
                DisableTooltips();
            }
        }
        else if (type == IslandVizInteraction.PressType.TouchUp && raycastSelectionIsRunning[handID])
        {
            raycastSelectionIsRunning[handID] = false; // This stops the RaySelection Coroutine.
        }
        else if (type == IslandVizInteraction.PressType.PressDown && raycastSelectionIsRunning[handID] && currentlyHitting[handID])
        {
            Collider collider = currentHits[handID].collider; // This local variable is very important for the undo to work!
            ToggleSelection(collider, Hands[handID], true); // Select the current selection.
        }
    }

    #endregion





    // ################
    // Selection
    // ################

    #region Selection

    /// <summary>
    /// Shoot a raycast every fixed update from one hand and handle the hits.
    /// This Coroutine is started when the player touches the touchpad of the controller (OnTouchpadTouchDown).
    /// </summary>
    /// <param name="handID">The hand from which the raycast comes from.</param>
    /// <returns></returns>
    private IEnumerator RaySelection (int handID)
    {
        laserBeamObjs[handID].SetActive(true);

        while (raycastSelectionIsRunning[handID]) // While touchpad is pressed.
        {
            currentForwards[handID] = Mode == RayMode.Laserpointer ? Hands[handID].transform.forward : (Hands[handID].transform.forward - Hands[handID].transform.up) / 2f;

            if (Physics.SphereCast(Hands[handID].transform.position + currentForwards[handID] * 0.1f, laserThickness / 2, currentForwards[handID], out currentHits[handID], laserLength, laserLayerMask))
            {
                if (currentHits[handID].collider != hittingColliders[handID])
                {
                    if (hittingColliders[handID] != null) // We jumped from one collider to the next, hence, we need to deselect the collider that we hit last fixed update.
                    {
                        ToggleHighlight(hittingColliders[handID], Hands[handID], false);
                    }
                    ToggleHighlight(currentHits[handID].collider, Hands[handID], true);
                    hittingColliders[handID] = currentHits[handID].collider;

                    if (Hands[handID] != null)
                        Hands[handID].controller.TriggerHapticPulse(250); // Vibrate
                }

                currentlyHitting[handID] = true;

                // Make laser visuals look like it stops at hit.
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, currentHits[handID].point);
            }
            else if (currentlyHitting[handID] || hittingColliders[handID] != null) // We hit something last update, but we do not now.
            {
                ToggleHighlight(hittingColliders[handID], Hands[handID], false);

                currentlyHitting[handID] = false;
                hittingColliders[handID] = null;

                // Reset laser visuals
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + currentForwards[handID] * 5f);
            }

            if (hittingColliders[handID] == null)
            {
                // Make laser visuals look like it hits nothing.
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + currentForwards[handID] * 5f);
            }

            yield return new WaitForFixedUpdate();
        }
        // On ray selection ended ...

        if (currentlyHitting[handID])
        {
            ToggleHighlight(hittingColliders[handID], Hands[handID], false);
            hittingColliders[handID] = null;
            currentlyHitting[handID] = false;
        }
        laserBeamObjs[handID].SetActive(false);
    }
    
    /// <summary>
    /// Throw a highlight event for this collider.
    /// </summary>
    /// <param name="collider">The collider whose highlight status is to be changed.</param>
    /// <param name="select">Wether it should be highlighted (true) or unhighlighted (false).</param>
    public void ToggleHighlight(Collider collider, Hand hand, bool select)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Highlight, select);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), IslandVizInteraction.SelectionType.Highlight, select);
        }
        else if (collider.GetComponent<Building>())
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), IslandVizInteraction.SelectionType.Highlight, select);
        }
        else if (collider.GetComponent<DependencyDock>())
        {
            IslandVizInteraction.Instance.OnDockSelect(collider.GetComponent<DependencyDock>(), IslandVizInteraction.SelectionType.Highlight, select);
        }
        else if (collider.GetComponent<UI_Button>())
        {
            IslandVizInteraction.Instance.OnUIButtonSelected(collider.GetComponent<UI_Button>(), IslandVizInteraction.SelectionType.Highlight, select);
        }
        else
        {
            IslandVizInteraction.Instance.OnOtherSelected?.Invoke(collider.gameObject, hand, IslandVizInteraction.SelectionType.Highlight, select);
        }
    }

    /// <summary>
    /// Throw a selection event for this collider.
    /// </summary>
    /// <param name="collider">The collider whose selection status is to be changed.</param>
    /// <param name="select">Wether it should be selected (true) or unselected (false).</param>
    public void ToggleSelection(Collider collider, Hand hand, bool select, bool addToUndo = true)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Select, select);
            IslandVizVisualization.Instance.FlyToSingleTarget(collider.transform, 5f);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), IslandVizInteraction.SelectionType.Select, select);
            //IslandVizVisualization.Instance.FlyToSingleTarget(collider.transform, 0.25f);
        }
        else if (collider.GetComponent<Building>())
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), IslandVizInteraction.SelectionType.Select, select);
            //IslandVizVisualization.Instance.FlyToSingleTarget(collider.transform, 0.5f);
        }
        else if (collider.GetComponent<DependencyDock>())
        {
            IslandVizInteraction.Instance.OnDockSelect(collider.GetComponent<DependencyDock>(), IslandVizInteraction.SelectionType.Select, select);           
        }
        else if (collider.GetComponent<UI_Button>())
        {
            IslandVizInteraction.Instance.OnUIButtonSelected(collider.GetComponent<UI_Button>(), IslandVizInteraction.SelectionType.Select, select);
            addToUndo = false;
        }
        else
        {
            IslandVizInteraction.Instance.OnOtherSelected?.Invoke(collider.gameObject, hand, IslandVizInteraction.SelectionType.Select, select);
        }

        if (select && addToUndo)
        {
            IslandVizBehaviour.Instance.AddUndoAction(delegate () {
                ToggleSelection(collider, hand, true, false);
            });
        }
    }

    #endregion


    // ################
    // Tooltips
    // ################

    #region Tooltips

    private void EnableTooltips()
    {
        foreach (var hand in Hands)
        {
            ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad, "Touch to enable Selector");
        }
    }

    private void DisableTooltips()
    {
        foreach (var hand in Hands)
        {
            ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad);
        }
        tooltipsDisabled = true;
    }

    #endregion



    // ################
    // Enums
    // ################

    #region Enums

    public enum RayMode
    {
        Laserpointer,
        Pistol
    }

    private enum HandType
    {
        Left,
        Right
    }

    #endregion


    // ################
    // Helper Functions
    // ################

    #region Helper Functions

    public int GetHandID (Hand hand)
    {
        for (int i = 0; i < Hands.Length; i++)
        {
            if (Hands[i] == hand)
            {
                return i;
            }
        }
        return -1;
    }

    public RaycastHit GetCurrentHit (int handID)
    {
        return currentHits[handID];
    }

    #endregion
}
