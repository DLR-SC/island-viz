using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// This class is attached to a SteamVR controller and is calling OnTriggerEnter and OnTriggerExit delegates from 
/// the IslandVizInteraction class.
/// </summary>
public class IslandVizInteractionController : MonoBehaviour {

    private Hand hand; // The hand this script is attached.

    private void Awake()
    {
        hand = GetComponent<Hand>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (IslandVizInteraction.Instance.OnControllerEnter != null)
        {
            IslandVizInteraction.Instance.OnControllerEnter(other, hand);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IslandVizInteraction.Instance.OnControllerExit != null)
        {
            IslandVizInteraction.Instance.OnControllerExit(other, hand);
        }
    }
}
