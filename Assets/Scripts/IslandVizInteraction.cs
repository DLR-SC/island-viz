using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// This class handles all interactions in the application with delegates.
/// Basically other classes, who depend on interaction events, subscribe to delegates, like "OnControllerTriggerDown", 
/// which then are called.
/// Input delegates are called by this class in the Update method.
/// Physics delegates are called by the class IslandVizInteractionController which is attached to both controllers.
/// </summary>
public class IslandVizInteraction : MonoBehaviour {

    public static IslandVizInteraction Instance;

    public Player Player;

    [Header("Additional Components Container")]
    public GameObject InteractionComponentsGameObject; // GameObject where all additional input components are located. 
                                                       // Note: Components are executed top down!

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

        //OnControllerTriggerDown += DebugInput;
    }

    /// <summary>
    /// Initialize all input components. Called by IslandVizBehavior.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitInputComponents()
    {        
        foreach (var item in inputComponents)
        {
            yield return item.Init();
        }
    }

    #endregion

       

    // ################
    // Interaction - Events
    // ################

    #region Interaction - Events

    /// <summary>
    /// Called when the trigger button of a controller is pressed.
    /// </summary>
    public ControllerTriggerDown OnControllerTriggerDown;
    /// <summary>
    /// Called when the trigger button of a controller is released.
    /// </summary>
    public ControllerTriggerUp OnControllerTriggerUp;
    /// <summary>
    /// Called when the touchpad of a controller is pressed down.
    /// </summary>
    public ControllerTouchpadDown OnControllerTouchpadDown;
    /// <summary>
    /// Called when the touchpad of a controller is released.
    /// </summary>
    public ControllerTouchpadUp OnControllerTouchpadUp;
    /// <summary>
    /// Called when a controller entered a trigger.
    /// </summary>
    public ControllerEnter OnControllerEnter;
    /// <summary>
    /// Called when a controller exited a trigger.
    /// </summary>
    public ControllerExit OnControllerExit;

    #endregion


    // ################
    // Interaction - Input
    // ################

    #region Interaction - Input

    /// <summary>
    /// Called when the trigger button of a controller is pressed.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerTriggerDown(Hand hand);
    
    /// <summary>
    /// Called when the trigger button of a controller is released.
    /// </summary>
    /// <param name="hand">The hand where the button was released.</param>
    public delegate void ControllerTriggerUp(Hand hand);

    /// <summary>
    /// Called when the touchpad of a controller is pressed down.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerTouchpadDown(Hand hand);

    /// <summary>
    /// Called when the touchpad of a controller is released.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerTouchpadUp(Hand hand);

    #endregion


    // ################
    // Interaction - Physics
    // ################

    #region Interaction - Physics

    /// <summary>
    /// Called when a controller entered a trigger.
    /// </summary>
    /// <param name="col">Collider component of the trigger GameObject.</param>
    /// <param name="hand">The hand that entered the trigger.</param>
    public delegate void ControllerEnter(Collider col, Hand hand);

    /// <summary>
    /// Called when a controller exited a trigger.
    /// </summary>
    /// <param name="col">Collider component of the trigger GameObject.</param>
    /// <param name="hand">The hand that exited the trigger.</param>
    public delegate void ControllerExit(Collider col, Hand hand);

    #endregion


    // ################
    // Interaction - Update
    // ################

    /// <summary>
    /// Called by Unity every frame.
    /// </summary>
    void Update()
    {
        // Go through both hands, check buttons and call delegates when they are not empty.
        for (int i = 0; i < 2; i++)
        {
            if (Player == null || Player.hands[i] == null || Player.hands[i].controller == null)
            {
                continue;
            }
            if (OnControllerTriggerDown != null && Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                OnControllerTriggerDown(Player.hands[i]);
            }
            if (OnControllerTriggerUp != null && Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                OnControllerTriggerUp(Player.hands[i]);
            }
            if (OnControllerTouchpadDown != null && Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerTouchpadDown(Player.hands[i]);
            }
            if (OnControllerTouchpadUp != null && Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerTouchpadUp(Player.hands[i]);
            }
        }
    }


    // ################
    // Interaction - FixedUpdate
    // ################

    /// <summary>
    /// Called by Unity every fixed time stamp.
    /// </summary>
    void FixedUpdate()
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

    void DebugInput (Hand hand)
    {
        Debug.Log("Input with " + hand.GuessCurrentHandType().ToString() + " hand!");
    }
    #endregion
}
