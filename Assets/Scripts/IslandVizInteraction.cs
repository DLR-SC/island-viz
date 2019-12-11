using OsgiViz;
using OsgiViz.Unity.Island;
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

    public static IslandVizInteraction Instance { get { return instance; } }

    public Player Player; // Set in Unity Editor.

    [Header("Additional Components Container")]
    public GameObject InteractionComponentsGameObject; // GameObject where all additional input components are located. 
                                                       // Note: Components are executed top down!

    private static IslandVizInteraction instance; // Current instance of this class.
    private AdditionalIslandVizComponent[] inputComponents; // Array of all additional input componets.
    private IslandSelectionComponent islandSelectionComponent;
    


    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Called by Unity on application stat up before the Start() method.
    /// </summary>
    void Awake()
    {
        instance = this;
        inputComponents = InteractionComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();

        islandSelectionComponent = InteractionComponentsGameObject.AddComponent<IslandSelectionComponent>();
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
            if (item.enabled)
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
    /// Called when the touchpad of a controller is pressed down.
    /// </summary>
    public ControllerTouchpadTouchStart OnControllerTouchpadTouchDown;
    /// <summary>
    /// Called when the touchpad of a controller is pressed down.
    /// </summary>
    public ControllerTouchpadTouchEnd OnControllerTouchpadTouchUp;
    /// <summary>
    /// Called when a controller entered a trigger.
    /// </summary>
    public ControllerEnter OnControllerEnter;
    /// <summary>
    /// Called when a controller exited a trigger.
    /// </summary>
    public ControllerExit OnControllerExit;
    /// <summary>
    /// Called when an island was selected or deselected.
    /// </summary>
    public IslandSelected OnIslandSelect;
    /// <summary>
    /// Called when an island was selected or deselected.
    /// </summary>
    public RegionSelected OnRegionSelect;
    /// <summary>
    /// Called when an island was selected or deselected.
    /// </summary>
    public BuildingSelected OnBuildingSelect;
    /// <summary>
    /// Called when an island was selected or deselected.
    /// </summary>
    public DockSelected OnDockSelect;
    /// <summary>
    /// Called when the grip button of a controller is pressed.
    /// </summary>
    public ControllerGripDown OnControllerGripDown;
    /// <summary>
    /// Called when the grip button of a controller is released.
    /// </summary>
    public ControllerGripUp OnControllerGripUp;
    /// <summary>
    /// Called when the grip button of a controller is pressed.
    /// </summary>
    public ControllerMenuDown OnControllerMenuDown;
    /// <summary>
    /// Called when the grip button of a controller is released.
    /// </summary>
    public ControllerMenuUp OnControllerMenuUp;

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
    /// <summary>
    /// Called when the touchpad of a controller is touched.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerTouchpadTouchStart(Hand hand);
    /// <summary>
    /// Called when the touchpad of a controller is not touched anymore.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerTouchpadTouchEnd(Hand hand);
    /// <summary>
    /// Called when the trigger button of a controller is pressed.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerGripDown(Hand hand);
    /// <summary>
    /// Called when the trigger button of a controller is released.
    /// </summary>
    /// <param name="hand">The hand where the button was released.</param>
    public delegate void ControllerGripUp(Hand hand);
    /// <summary>
    /// Called when the menu button of a controller is pressed.
    /// </summary>
    /// <param name="hand">The hand where the button was pressed.</param>
    public delegate void ControllerMenuDown(Hand hand);
    /// <summary>
    /// Called when the menu button of a controller is released.
    /// </summary>
    /// <param name="hand">The hand where the button was released.</param>
    public delegate void ControllerMenuUp(Hand hand);

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
    // Interaction - Visualization
    // ################

    #region Interaction - Visualization

    /// <summary>
    /// Called when an island GameObject was selected or deselected.
    /// </summary>
    /// <param name="island">The island that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void IslandSelected(IslandGO island, SelectionType selectionType, bool selected);
    /// <summary>
    /// Called when a region GameObject was selected or deselected.
    /// </summary>
    /// <param name="region">The region that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void RegionSelected(Region region, SelectionType selectionType, bool selected);
    /// <summary>
    /// Called when a building GameObject was selected or deselected.
    /// </summary>
    /// <param name="building">The building that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void BuildingSelected(Building building, SelectionType selectionType, bool selected);
    /// <summary>
    /// Called when a building GameObject was selected or deselected.
    /// </summary>
    /// <param name="dock">The dock that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void DockSelected(DependencyDock dock, SelectionType selectionType, bool selected);

    #endregion


    // ################
    // Interaction - Update
    // ################

    /// <summary>
    /// Called by Unity every frame.
    /// This goes through both hands, checks if buttons are pressed down and calls the delegates.
    /// </summary>
    void Update()
    {        
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
            if (OnControllerTouchpadTouchDown != null && Player.hands[i].controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerTouchpadTouchDown(Player.hands[i]);
            }
            if (OnControllerTouchpadTouchUp != null && Player.hands[i].controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerTouchpadTouchUp(Player.hands[i]);
            }
            if (OnControllerGripDown != null && Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            {
                OnControllerGripDown(Player.hands[i]);
            }
            if (OnControllerGripDown != null && Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
                OnControllerGripDown(Player.hands[i]);
            }
            if (OnControllerMenuDown != null && Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                OnControllerMenuDown(Player.hands[i]);
            }
            if (OnControllerMenuUp != null && Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                OnControllerMenuUp(Player.hands[i]);
            }
        }
    }



    // ################
    // Enums
    // ################

    public enum SelectionType
    {
        Select,
        Highlight        
    }


    // ################
    // Helper Functions
    // ################

    #region HelperFunctions

    //public float GetPlayerEyeHeight ()
    //{
    //    return Player.eyeHeight;
    //}    

    void DebugInput (Hand hand)
    {
        Debug.Log("Input with " + hand.GuessCurrentHandType().ToString() + " hand!");
    }
    #endregion
}
