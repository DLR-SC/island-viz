using OsgiViz.Unity.Island;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RaycastSelection : AdditionalIslandVizComponent
{
    public Hand Hand;

    private RaycastHit hit;

    private GameObject DebugObject;

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
        DebugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DebugObject.transform.localScale = Vector3.one * 0.2f;
        Destroy(DebugObject.GetComponent<SphereCollider>());

        yield return null;

        initiated = true;
    }
    #endregion



    private void FixedUpdate()
    {
        if (!initiated)
            return;

        if (Physics.Raycast(Hand.transform.position + Hand.transform.forward * 0.1f, Hand.transform.forward * 1.5f, out hit))
        {
            //DebugObject.transform.position = hit.point;
            if (hit.collider.GetComponent<IslandGO>() != null)
            {
                IslandSelectionComponent.Instance.SelectIsland(hit.collider.GetComponent<IslandGO>());
            }
        }
    }
}
