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
public class IslandVizInteraction : MonoBehaviour
{
    public static IslandVizInteraction Instance { get; private set; }

    public Player Player; // Set in Unity Editor.

    [Header("Additional Components Container")]
    public GameObject InteractionComponentsGameObject; // GameObject where all additional input components are located. 
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
        Instance = this;
        inputComponents = InteractionComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();

        islandSelectionComponent = InteractionComponentsGameObject.AddComponent<IslandSelectionComponent>(); // TODO
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
    // Events
    // ################

    #region Events

    /// <summary>
    /// Called when a button of a controller is pressed, released or touched.
    /// </summary>
    public ControllerButtonEvent OnControllerButtonEvent;

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
    /// Called when a ui button was selected or deselected.
    /// </summary>
    public UIButtonSelected OnUIButtonSelected;
    /// <summary>
    /// Called when any other object was selected or deselected.
    /// </summary>
    public OtherSelected OnOtherSelected;
    #endregion





    // ################
    // Delegates - Input
    // ################

    #region Delegates - Input

    /// <summary>
    /// This is called when a controller button is pressed, released or touched.
    /// </summary>
    /// <param name="button">The button that was pressed, released or touched.</param>
    /// <param name="type">Weather the button was pressed, released or touched.</param>
    /// <param name="hand">The hand that pressed the button.</param>
    public delegate void ControllerButtonEvent(Button button, PressType type, Hand hand);

    #endregion


    // ################
    // Delegates - Physics
    // ################

    #region Delegates - Physics

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
    // Delegates - Selection
    // ################

    #region Delegates - Selection

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
    /// <summary>
    /// Called when a building GameObject was selected or deselected.
    /// </summary>
    /// <param name="button">The UI button that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void UIButtonSelected(UI_Button button, SelectionType selectionType, bool selected);
    /// <summary>
    /// Called when any other GameObject with a collider was selected or deselected.
    /// Use this event for custom additional components.
    /// </summary>
    /// <param name="go">The object that was selected.</param>
    /// <param name="selectionType">The type of the selection.</param>
    /// <param name="selected">True = select, false = deselect.</param>
    public delegate void OtherSelected(GameObject go, SelectionType selectionType, bool selected);
    #endregion


    // ################
    // Input Update
    // ################

    /// <summary>
    /// Called by Unity every frame.
    /// This goes through both hands, checks if buttons are pressed down and calls the delegates.
    /// </summary>
    void Update()
    {        
        for (int i = 0; i < 2; i++)
        {
            if (Player == null || Player.hands[i] == null || Player.hands[i].controller == null || OnControllerButtonEvent == null)
            {
                continue;
            }
            if (Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                OnControllerButtonEvent(Button.Trigger, PressType.PressDown, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                OnControllerButtonEvent(Button.Trigger, PressType.PressUp, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerButtonEvent(Button.Touchpad, PressType.PressDown, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerButtonEvent(Button.Touchpad, PressType.PressUp, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerButtonEvent(Button.Touchpad, PressType.TouchDown, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                OnControllerButtonEvent(Button.Touchpad, PressType.TouchUp, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            {
                OnControllerButtonEvent(Button.Grip, PressType.PressDown, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
                OnControllerButtonEvent(Button.Grip, PressType.PressUp, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                OnControllerButtonEvent(Button.Menu, PressType.PressDown, Player.hands[i]);
            }
            if (Player.hands[i].controller.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                OnControllerButtonEvent(Button.Menu, PressType.PressUp, Player.hands[i]);
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

    public enum Button
    {
        Trigger, 
        Menu,
        Touchpad,
        Grip
    }

    public enum PressType
    {
        PressDown,
        PressUp,
        TouchDown,
        TouchUp
    }


    // ################
    // Helper Functions
    // ################

    #region HelperFunctions


    void DebugInput (Hand hand)
    {
        Debug.Log("Input with " + hand.GuessCurrentHandType().ToString() + " hand!");
    }

    #endregion
}
