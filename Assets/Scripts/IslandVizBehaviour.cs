using OsgiViz.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using DatabasePreprocessing;

/// <summary>
/// This is the main class of the IslandViz application. 
/// This class handles the initiation of the islandViz and handles undo calls.
/// </summary>
public class IslandVizBehaviour : MonoBehaviour
{
    public enum VisualizationType
    {
        Static, History
    }

    public static IslandVizBehaviour Instance { get; private set; }

    public VisualizationType vizType; // Wether the osgi project is loaded from a json file or a neo4j database.

    private List<Action> undoList; // List of the last user actions.


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

        if(vizType == VisualizationType.Static)
        {
            //Static View, only one commit
            StartCoroutine(IslandVizInitiationRoutine()); // Start the islandviz construction coroutine.
        }
        else if(vizType == VisualizationType.History)
        {
            StartCoroutine(IslandVizInitiationRoutine_History());
        }

    }
    
    /// <summary>
    /// The main construction routine of the islandviz application.
    /// </summary>
    IEnumerator IslandVizInitiationRoutine ()
    {
        yield return null;

        yield return IslandVizData.Instance.ConstructOsgiProject(); // Load the data we want to visualize.
       
        yield return IslandVizVisualization.Instance.ConstructVisualization(); // Construct the basic visualization, i.e. islands, ports, and dependencies.
       
        yield return IslandVizData.Instance.InitInputComponents(); // Load additional data components.
        yield return IslandVizVisualization.Instance.InitVisualizationComponents(); // Load additional visualization components.
        yield return IslandVizInteraction.Instance.InitInputComponents(); // Load additional interaction components.
        
        IslandVizUI.Instance.InitBundleNames(); // TODO Move */

        OnConstructionDone?.Invoke(); // Call the OnConstructionDone event.
    }

    IEnumerator IslandVizInitiationRoutine_History()
    {
        yield return new WaitForSeconds(1f);
        //Database Preprocessing for History-Information
        yield return DatabasePreprocessingScript.Instance.PreprocessingMain();
        //Loading History Data
        yield return DataLoading.Instance.LoadingProject();
        //Island Layout
        yield return LayoutCreation.Instance.AllBundlesGridCreation();
        //Dynamic Graph Layout
        IslandVizUI.Instance.UpdateLoadingScreenUI("Start DynGraph", "");
        yield return DynamicGraphCalculation.Instance.GraphMain();
        //Gameobject Creation
        IslandVizUI.Instance.UpdateLoadingScreenUI("Start GoCreation", "");
        yield return GOCreation_Script.Instance.GOCreationMain();

        IslandVizUI.Instance.UpdateLoadingScreenUI("Finished", "100%"); // Update UI.


        yield return null;

        yield return IslandVizData.Instance.InitInputComponents(); // Load additional data components.
        yield return IslandVizVisualization.Instance.InitVisualizationComponents(); // Load additional visualization components.
        yield return IslandVizInteraction.Instance.InitInputComponents(); // Load additional interaction components.


        GameObject IV_Container = GameObject.Find("VisualizationContainer");
        GameObject objectContainer = GameObject.Find("IslandObjectContainer");

        objectContainer.transform.SetParent(IV_Container.transform, false);

        GameObject vis = GameObject.Find("Visualization");
        //TODO Richtige Höhe für Visualisierung
        vis.transform.position = new Vector3(0f, 1f, 0f);
        vis.transform.localScale = new Vector3(0.00145f, 0.00145f, 0.00145f);

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
    /// <param name="hand">The hand where the undo button was pressed.</param>
    public void Undo (Hand hand)
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
