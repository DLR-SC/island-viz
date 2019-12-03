using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RaycastSelection : AdditionalIslandVizComponent
{
    public Hand Hand;
    public RayMode Mode;
    //public float MaxDistance = 100f;    
    public Material laserMaterial;

    private float laserLength = 3f;
    private float laserThickness = 0.01f;

    private RaycastHit hit;

    private GameObject beamObj;
    private LineRenderer lineRenderer;

    private bool initiated = false;
    private bool currentlyHitting = false;
    private Collider currendHittingCollider;

    private Vector3 forward;





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
        IslandVizInteraction.Instance.OnControllerTriggerDown += OnTriggerDown;

        // Laser beam visuals
        beamObj = new GameObject();
        beamObj.name = "LaserBeam";
        beamObj.transform.SetParent(Hand.transform);
        lineRenderer = beamObj.AddComponent<LineRenderer>();
        lineRenderer.material = laserMaterial;
        lineRenderer.startWidth = laserThickness;
        lineRenderer.endWidth = laserThickness;

        yield return null;

        initiated = true;
    }
    #endregion



    private void FixedUpdate()
    {
        if (!initiated)
            return;

        forward = Mode == RayMode.Laserpointer ? Hand.transform.forward : (Hand.transform.forward - Hand.transform.up) / 2f;

        //if (Physics.Raycast(Hand.transform.position + Hand.transform.forward * 0.1f, Hand.transform.forward * 1.5f, out hit, 5f))
        if (Physics.SphereCast(Hand.transform.position + forward * 0.1f, laserThickness/2, forward, out hit))
        {
            if (hit.collider != currendHittingCollider)
            {
                if (currendHittingCollider != null) // We jumped from one collider to the next, hence, we need to deselect the prior collider.
                {
                    ToggleSelection(currendHittingCollider, false);
                }
                ToggleSelection(hit.collider, true);
                currendHittingCollider = hit.collider;
            }

            // Make laser visuals look like it stops at hit.
            lineRenderer.SetPosition(0, Hand.transform.position);
            lineRenderer.SetPosition(1, hit.point);

            if (!currentlyHitting)
            {
                currentlyHitting = true;
            }
        }
        else if (currentlyHitting || currendHittingCollider != null) // We hit something last update, but we do not now.
        {
            ToggleSelection(currendHittingCollider, false);

            currentlyHitting = false;
            currendHittingCollider = null;

            // Reset laser visuals
            lineRenderer.SetPosition(0, Hand.transform.position);
            lineRenderer.SetPosition(1, Hand.transform.position + forward * 5f);
        }

        if (currendHittingCollider == null)
        {
            lineRenderer.SetPosition(0, Hand.transform.position);
            lineRenderer.SetPosition(1, Hand.transform.position + forward * 5f);
        }
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








    private void OnTriggerDown (Hand hand)
    {
        if (currentlyHitting && hand == Hand)
        {
            IslandVizVisualization.Instance.SelectAndFlyTo(hit.collider.transform);
        }
    }



    public enum RayMode
    {
        Laserpointer,
        Pistol
    }
}
