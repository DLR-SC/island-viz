using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class RaycastSelection : AdditionalIslandVizComponent
{
    public Hand[] Hands; // You can attach both or only one hand.
    public RayMode Mode;
    //public float MaxDistance = 100f;    
    public Material laserMaterial;

    public bool ShowTooltipps = true;

    private float laserLength = 3f;
    private float laserThickness = 0.01f;

    private RaycastHit[] hit;

    private GameObject[] beamObjs;
    private LineRenderer[] lineRenderers;
    private Vector3[] forwards;

    private bool initiated = false;
    private bool tooltippsDisabled = false;

    private bool[] touchpadTouch = new bool[] { false, false };
    private bool[] currentlyHitting = new bool[] { false, false };

    private Collider[] currendHittingCollider;






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
        beamObjs = new GameObject[Hands.Length];
        lineRenderers = new LineRenderer[Hands.Length];
        forwards = new Vector3[Hands.Length];
        hit = new RaycastHit[Hands.Length];
        currendHittingCollider = new Collider[Hands.Length];

        IslandVizInteraction.Instance.OnControllerTouchpadDown += OnTouchpadPressed;
        IslandVizInteraction.Instance.OnControllerTouchpadTouchDown += OnTouchpadTouchDown;
        IslandVizInteraction.Instance.OnControllerTouchpadTouchUp += OnTouchpadTouchUp;

        for (int i = 0; i < Hands.Length; i++)
        {
            // Laser beam visuals
            beamObjs[i] = new GameObject();
            beamObjs[i].name = "LaserBeam";
            beamObjs[i].transform.SetParent(Hands[i].transform);
            lineRenderers[i] = beamObjs[i].AddComponent<LineRenderer>();
            lineRenderers[i].material = laserMaterial;
            lineRenderers[i].startWidth = laserThickness;
            lineRenderers[i].endWidth = laserThickness;
            beamObjs[i].SetActive(false);

            if (ShowTooltipps)
            {
                ControllerButtonHints.ShowTextHint(Hands[i], EVRButtonId.k_EButton_SteamVR_Touchpad, "Touch to enable Selector");
            }            
        }

        if (!ShowTooltipps)
            tooltippsDisabled = true;

        yield return null;

        initiated = true;
    }
    #endregion



    private IEnumerator RaySelection (int handID)
    {
        beamObjs[handID].SetActive(true);

        while (touchpadTouch[handID])
        {
            forwards[handID] = Mode == RayMode.Laserpointer ? Hands[handID].transform.forward : (Hands[handID].transform.forward - Hands[handID].transform.up) / 2f;

            //if (Physics.Raycast(Hand.transform.position + Hand.transform.forward * 0.1f, Hand.transform.forward * 1.5f, out hit, 5f))
            if (Physics.SphereCast(Hands[handID].transform.position + forwards[handID] * 0.1f, laserThickness / 2, forwards[handID], out hit[handID]))
            {
                if (hit[handID].collider != currendHittingCollider[handID])
                {
                    if (currendHittingCollider[handID] != null) // We jumped from one collider to the next, hence, we need to deselect the prior collider.
                    {
                        ToggleSelection(currendHittingCollider[handID], false);
                    }
                    ToggleSelection(hit[handID].collider, true);
                    currendHittingCollider[handID] = hit[handID].collider;
                }

                // Make laser visuals look like it stops at hit.
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, hit[handID].point);

                if (!currentlyHitting[handID])
                {
                    currentlyHitting[handID] = true;
                }
            }
            else if (currentlyHitting[handID] || currendHittingCollider[handID] != null) // We hit something last update, but we do not now.
            {
                ToggleSelection(currendHittingCollider[handID], false);

                currentlyHitting[handID] = false;
                currendHittingCollider[handID] = null;

                // Reset laser visuals
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + forwards[handID] * 5f);
            }

            if (currendHittingCollider[handID] == null)
            {
                lineRenderers[handID].SetPosition(0, Hands[handID].transform.position);
                lineRenderers[handID].SetPosition(1, Hands[handID].transform.position + forwards[handID] * 5f);
            }

            yield return new WaitForFixedUpdate();
        }

        beamObjs[handID].SetActive(false);
    }




    public void ToggleSelection(Collider collider, bool select)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), select);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), select);
        }
        else if (collider.GetComponent<Building>()) 
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), select);
        }
    }


    private void OnTouchpadPressed (Hand hand)
    {
        int handID = GetHandID(hand);

        if (handID >= 0 && touchpadTouch[handID] && currentlyHitting[handID])
        {
            IslandVizVisualization.Instance.SelectAndFlyTo(hit[handID].collider.transform);
        }
    }

    private void OnTouchpadTouchDown(Hand hand)
    {
        int handID = GetHandID(hand);
        
        if (initiated && handID >= 0 && !touchpadTouch[handID])
        {
            touchpadTouch[handID] = true;
            StartCoroutine(RaySelection(handID));

            if (!tooltippsDisabled)
            {
                DIsableTooltipps();
            }
        }
    }
    private void OnTouchpadTouchUp(Hand hand)
    {
        int handID = GetHandID(hand);

        if (handID >= 0 && touchpadTouch[handID])
        {
            touchpadTouch[handID] = false;
        }
    }




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

    private void DIsableTooltipps ()
    {
        foreach (var hand in Hands)
        {
            ControllerButtonHints.HideTextHint(hand, EVRButtonId.k_EButton_SteamVR_Touchpad);
        }
    }
}
