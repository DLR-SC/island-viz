using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class IslandVizInteraction : MonoBehaviour {

    public static IslandVizInteraction Instance;

    public Player Player;

    [Header("Additional Components Container")]
    public GameObject InteractionComponentsGameObject; // GameObject where all additional input 
                                                       // components are located. Components are 
                                                       // executed from top to bottom!


    private AdditionalIslandVizComponent[] inputComponents; // Array of all additional input componets.


    

    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Called by Unity on application stat up before the Start() method.
    /// </summary>
    void Awake()
    {
        Instance = this;
        inputComponents = InteractionComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();
    }

    /// <summary>
    /// Initialize all input components. Called by IslandVizBehavior.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitInputComponents()
    {        
        foreach (var item in inputComponents)
        {
            item.Init();
            yield return null;
        }
    }

    #endregion


    

    // ################
    // Interaction - Input
    // ################

    public void OnControllerTriggerDown ()
    {

    }

    public void OnControllerTriggerUp()
    {

    }

    public void OnControllerTouchpadDown ()
    {

    }

    public void OnControllerTouchpadUp()
    {

    }


    // ################
    // Interaction - Physics
    // ################

    public void OnControllerEnter()
    {

    }

    public void OnControllerExit()
    {

    }




    // ################
    // Helper Functions
    // ################

    #region HelperFunctions

    public float GetPlayerEyeHeight ()
    {
        return Player.eyeHeight;
    }

    public Vector3 GetHandPosition (Hand.HandType handType)
    {
        switch (handType)
        {
            case Hand.HandType.Left:
                return Player.leftHand.transform.position;
            case Hand.HandType.Right:
                return Player.rightHand.transform.position;
            default:
                Debug.LogError("IslandVizInteraction.GetHandPosition(): No Hand specified! -> returning Vector3.zero");
                return Vector3.zero;
        }
    }

    #endregion
}
