using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RaycastSelection : AdditionalIslandVizComponent
{
    public Hand Hand;
    //public float MaxDistance = 100f;    
    public Material laserMaterial;

    private float laserLength = 3f;
    private float laserThickness = 0.01f;

    private RaycastHit hit;

    private GameObject beamObj;

    private bool initiated = false;
    private bool currentlyHitting = false;
    private Collider currendHittingCollider;

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
        // Laser beam visuals
        beamObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beamObj.GetComponent<MeshRenderer>().sharedMaterial = laserMaterial;
        beamObj.name = "LaserBeam";
        Destroy(beamObj.GetComponent<BoxCollider>());
        beamObj.transform.SetParent(Hand.transform);
        beamObj.transform.localRotation = Quaternion.identity;
        beamObj.transform.localPosition = Vector3.forward * laserLength/2f;
        beamObj.transform.localScale = new Vector3(laserThickness, laserThickness, laserLength);

        IslandVizInteraction.Instance.OnControllerTriggerDown += OnTriggerDown;

        yield return null;

        initiated = true;
    }
    #endregion



    private void FixedUpdate()
    {
        if (!initiated)
            return;

        //if (Physics.Raycast(Hand.transform.position + Hand.transform.forward * 0.1f, Hand.transform.forward * 1.5f, out hit, 5f))
        if (Physics.SphereCast(Hand.transform.position + Hand.transform.forward * 0.1f, laserThickness/2, Hand.transform.forward, out hit))
        {
            if (hit.collider != currendHittingCollider)
            {
                if (currendHittingCollider != null)
                {
                    Deselect(currendHittingCollider);
                }
                Select(hit.collider);
                currendHittingCollider = hit.collider;
            }

            // Make laser visuals look like it stops at hit.
            float newLaserLength = Vector3.Distance(Hand.transform.position, hit.point);
            beamObj.transform.localPosition = Vector3.forward * newLaserLength / 2f;
            beamObj.transform.localScale = new Vector3(laserThickness, laserThickness, newLaserLength);

            if (!currentlyHitting)
            {
                currentlyHitting = true;
            }
        }
        else if (currentlyHitting || currendHittingCollider != null) // We hit something last update, but we do not now.
        {
            Deselect(currendHittingCollider);

            currentlyHitting = false;
            currendHittingCollider = null;

            // Reset laser visuals
            beamObj.transform.localPosition = Vector3.forward * laserLength / 2f;
            beamObj.transform.localScale = new Vector3(laserThickness, laserThickness, laserLength);
        }
    }


    public void Select(Collider collider)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), true);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), true);
        }
        else if (collider.GetComponent<Building>()) 
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), true);
        }
    }

    public void Deselect(Collider collider)
    {
        if (collider.GetComponent<IslandGO>())
        {
            IslandVizInteraction.Instance.OnIslandSelect(collider.GetComponent<IslandGO>(), false);
        }
        else if (collider.GetComponent<Region>())
        {
            IslandVizInteraction.Instance.OnRegionSelect(collider.GetComponent<Region>(), false);
        }
        else if (collider.GetComponent<Building>())
        {
            IslandVizInteraction.Instance.OnBuildingSelect(collider.GetComponent<Building>(), false);
        }
    }







    private void OnTriggerDown (Hand hand)
    {
        if (currentlyHitting && hand == Hand)
        {
            IslandVizVisualization.Instance.SelectAndFlyTo(hit.collider.transform);
        }
    }
}
