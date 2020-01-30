using OsgiViz.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// This is the main class of the IslandViz application. 
/// This class handles the initiation of the islandViz and handles undo calls.
/// </summary>
public class IslandVizBehaviour : MonoBehaviour
{
    public static IslandVizBehaviour Instance { get; private set; } // The instance of this class.

    private List<Action> undoList; // List containing the last user actions.



    // ################
    // Events
    // ################

    /// <summary>
    /// Called when the IslandVizConstructionRoutine has finished.
    /// </summary>
    public ConstructionDone OnConstructionDone;

    /// <summary>
    /// Called when the IslandVizConstructionRoutine has finished.
    /// </summary>
    public delegate void ConstructionDone();

    



    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Called by Unity when the application is started. 
    /// Note: Awake() is called before Start().
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called by Unity when the application is started.
    /// </summary>
    void Start ()
    {
        // Reset shader settings. TODO
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
        Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
        Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);        
        Shader.SetGlobalVector("hologramCenter", new Vector3(0, 0, 0));
        Shader.SetGlobalFloat("hologramScale", 0.8f);

        undoList = new List<Action>();

        StartCoroutine(IslandVizInitiationRoutine()); // Start the islandviz construction coroutine.
    }
    
    /// <summary>
    /// The main construction routine of the islandviz application.
    /// </summary>
    IEnumerator IslandVizInitiationRoutine ()
    {
        yield return null;

        if (IslandVizConfig.Instance.UseConfig)
        {
            yield return IslandVizConfig.Instance.LoadAndApplyConfig();
        }

        yield return IslandVizData.Instance.ConstructOsgiProject(); // Load the data we want to visualize.
        
        yield return IslandVizVisualization.Instance.ConstructVisualization(); // Construct the basic visualization, i.e. islands, ports, and dependencies.

        yield return IslandVizData.Instance.InitInputComponents(); // Load additional data components.
        yield return IslandVizVisualization.Instance.InitVisualizationComponents(); // Load additional visualization components.
        yield return IslandVizInteraction.Instance.InitInputComponents(); // Load additional interaction components.

        //IslandVizUI.Instance.InitBundleNames(); // TODO Move 

        OnConstructionDone?.Invoke(); // Call the OnConstructionDone event.
    }

    #endregion





    // ################
    // Undo
    // ################

    #region Undo

    /// <summary>
    /// Add the current action to the undo list, so that the user can return to this action in the future. 
    /// Note that pressing undo will execute the second last action in the undo list.
    /// </summary>
    /// <param name="action">A action that will be executed when user presses undo.</param>
    public void AddUndoAction (Action action)
    {
        undoList.Add(action);
    }

    /// <summary>
    /// Currently called by a UI_Button. Pressing the undo button will execute the second last action in the undo list.
    /// Note that the last action in the undo list is the current action.
    /// </summary>
    public void Undo ()
    {
        if (undoList.Count >= 2) // [Last Action|Current Action] --> We want the last action, so we take the second last item.
        {
            IslandVizUI.Instance.MakeNotification(1f, "Undo");

            undoList[undoList.Count-2]?.Invoke();
            undoList.RemoveAt(undoList.Count - 2);
        }
        else
        {
            // No action to undo.
        }
    }

    #endregion
}
