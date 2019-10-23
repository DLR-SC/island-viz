using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class IslandVizInteractionController : MonoBehaviour {

    private Hand hand; 

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
