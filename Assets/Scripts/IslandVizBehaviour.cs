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
    /// Called by Unity on application stat up before the Start() method.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        // Reset Shader Settings
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1f;
        Shader.SetGlobalFloat("hologramOutlineWidth", GlobalVar.hologramOutlineWidth);
        Shader.SetGlobalVector("hologramOutlineColor", GlobalVar.hologramOutlineColor);        
        Shader.SetGlobalVector("hologramCenter", new Vector3(0, 0, 0));
        Shader.SetGlobalFloat("hologramScale", 0.8f);

        //float3 hologramCenter = float3(0, 0, -1.42f);
        //float hologramScale = 0.8f;
        //float hologramOutlineWidth = 0.5f;
        //float3 hologramOutlineColor;


        StartCoroutine(IslandVizRoutine());
	}


    IEnumerator IslandVizRoutine ()
    {
        yield return IslandVizData.Instance.ConstructOsgiProject();

        yield return IslandVizVisualization.Instance.Construction();

        yield return IslandVizInteraction.Instance.InitInputComponents();
    }
	
	
}
