using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// This component enables the user to select islands via a SphereCast. Additionally a laser beam is visualizing the SphereCast.
/// </summary>
public class RaycastSelection : AdditionalIslandVizComponent
{
    // ################
    // Public
    // ################

    public Hand[] Hands; // Attach both or only one hand in the editor.
    public RayMode Mode; // This changes the angle of the ray. Choose whatever you prever.
    public Material LaserMaterial; // The material of the laser pointer visual.

    [Tooltip("Wether tooltips should be shown on start. Tooltipps will stay until the touchpad was touched.")]
    public bool ShowTooltips = true;

    // ################
    // Private
    // ################

    private float laserLength = 3f; // The length of the SphereCast and the laser beam.
    private float laserThickness = 0.01f; // The thickness of the SphereCast and the laser beam.

    // Every field in the arrays belongs to one hand, i.e. the first hand always stores the values in the 0th field and so on.
    private RaycastHit[] hit; // Current RaycastHits of the hands.
    private Vector3[] forward; // Current forward vetors of the hands.
    private Collider[] hittingCollider; // Current colliders the hands are hitting with the SphereCast.

    private GameObject[] laserBeamObjs; // The GameObjects of the laser beams.
    private LineRenderer[] lineRenderers; // The LineRenderer components of the laser beam GameObjects.
    
    private bool[] touchpadTouch; // True if the touchpad is currently touched.
    private bool[] currentlyHitting; // True if the SphereCast is currently hitting something.

    private bool initiated = false;
    private bool tooltippsDisabled = false;




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
        // Initiate arrays.
        laserBeamObjs = new GameObject[Hands.Length];
        lineRenderers = new LineRenderer[Hands.Length];
        forward = new Vector3[Hands.Length];
        hit = new RaycastHit[Hands.Length];
        hittingCollider = new Collider[Hands.Length];
        touchpadTouch = new bool[] { false, false };
        currentlyHitting = new bool[] { false, false };

        // Subscribe to events.
        IslandVizInteraction.Instance.OnControllerTouchpadDown += OnTouchpadPressed;
        IslandVizInteraction.Instance.OnControllerTouchpadTouchDown += OnTouchpadTouchDown;
        IslandVizInteraction.Instance.OnControllerTouchpadTouchUp += OnTouchpadTouchUp;

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
            tooltippsDisabled = true;
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
    /// If the Hands array contains the hand, the a RaySelection Coroutine is started.
    /// </summary>
    /// <param name="hand">The hand where the touchpad was touched.</param>
    private void OnTouchpadTouchDown(Hand hand)
    {
        int handID = GetHandID(hand);

        if (initiated && handID >= 0 && !touchpadTouch[handID])
        {
            touchpadTouch[handID] = true;
            StartCoroutine(RaySelection(handID));

            if (!tooltippsDisabled)
            {
                DisableTooltips();
            }
        }
    }

    /// <summary>
    /// If the Hands array contains the hand, the touchpadTouch bool is set to false which stops the RaySelection Coroutine.
    /// </summary>
    /// <param name="hand">The hand where the touchpad is not touched anymore.</param>
    private void OnTouchpadTouchUp(Hand hand)
    {
        int handID = GetHandID(hand);

        if (handID >= 0 && touchpadTouch[handID])
        {
            touchpadTouch[handID] = false;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="hand">The hand where the touchpad was pressed.</param>
    private void OnTouchpadPressed(Hand hand)
    {
        int handID = GetHandID(hand);

        if (handID >= 0 && touchpadTouch[handID] && currentlyHitting[handID])
        {
            Collider collider = hit[handID].collider; // This local variable is very important for the undo to work!

            ToggleSelection(collider, true);
            IslandVizVisualization.Instance.FlyTo(collider.transform);

            IslandVizBehaviour.Instance.UndoList.Add(delegate () {
                ToggleSelection(collider, true);
                IslandVizVisualization.Instance.FlyTo(collider.transform);
            });
        }
    }

    #endregion





    // ################
    // Selection
    // ################

    #region Selection

    private IEnumerator RaySelection (int handID)
    {
        laserBeamObjs[handID].SetActive(true);

        while (touchpadTouch[handID])
        {
            forward[handID] = Mode == RayMode.Laserpointer ? Hands[handID].transform.forward : (Hands[handID].transform.forward - Hands[handID].transform.up) / 2f;

            //if (Physics.Raycast(Hand.transform.position + Hand.transform.forward * 0.1f, Hand.transform.forward * 1.5f, out hit, 5f))
            if (Physics.SphereCast(Hands[handID].transform.position + forward[handID] * 0.1f, laserThickness / 2, forward[handID], out hit[handID], laserLength))
            {
                if (hit[handID].collider != hittingCollider[handID])
                {
                    if (hittingCollider[handID] != null) // We jumped from one collider to the next, hence, we need to deselect the prior collider.
                    {
                        ToggleHighlight(hittingCollider[handID], false);
                    }
                    ToggleHighlight(hit[handID].collider, true);
                    hittingCollider[handID] = hit[handID].collider;
                }

                // Make laser visuals look like it stops at hit.
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, hit[handID].point);

                if (!currentlyHitting[handID])
                {
                    currentlyHitting[handID] = true;
                }
            }
            else if (currentlyHitting[handID] || hittingCollider[handID] != null) // We hit something last update, but we do not now.
            {
                ToggleHighlight(hittingCollider[handID], false);

                currentlyHitting[handID] = false;
                hittingCollider[handID] = null;

                // Reset laser visuals
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + forward[handID] * 5f);
            }

            if (hittingCollider[handID] == null)
            {
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + forward[handID] * 5f);
            }

            yield return new WaitForFixedUpdate();
        }

        if (currentlyHitting[handID])
        {
            ToggleHighlight(hittingCollider[handID], false);
            hittingCollider[handID] = null;
            currentlyHitting[handID] = false;
        }
        laserBeamObjs[handID].SetActive(false);
    }




    public void ToggleHighlight(Collider collider, bool select)
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
    }

    public void ToggleSelection(Collider collider, bool select)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), IslandVizInteraction.SelectionType.Select, select);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), IslandVizInteraction.SelectionType.Select, select);
        }
        else if (collider.GetComponent<Building>())
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), IslandVizInteraction.SelectionType.Select, select);
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
        tooltippsDisabled = true;
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

    private int GetHandID (Hand hand)
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

    #endregion
}
