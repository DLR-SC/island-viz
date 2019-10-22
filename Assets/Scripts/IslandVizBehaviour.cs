using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class of the IslandViz application. 
/// </summary>
public class IslandVizBehaviour : MonoBehaviour
{
    public static IslandVizBehaviour Instance; // The instance of this class.

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
        // Reset shader settings.
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
        Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
        Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);        
        Shader.SetGlobalVector("hologramCenter", new Vector3(0, 0, 0));
        Shader.SetGlobalFloat("hologramScale", 0.8f);
        
        // Start the islandviz coroutine.
        StartCoroutine(IslandVizRoutine());
	}

    /// <summary>
    /// The main routine of the islandviz application.
    /// </summary>
    IEnumerator IslandVizRoutine ()
    {
        // Load the data we want to visualize.
        yield return IslandVizData.Instance.ConstructOsgiProject();

        // Construct the basic visualization.
        yield return IslandVizVisualization.Instance.ConstructVisualization();

        // Load additional components
        yield return IslandVizVisualization.Instance.InitVisualizationComponents();
        yield return IslandVizInteraction.Instance.InitInputComponents();
    }
}
