using OsgiViz.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandVizBehaviour : MonoBehaviour
{
    public static IslandVizBehaviour Instance;
    public static IslandVizData IslandVizData;
    public static IslandVizVisualization IslandVizVisualization;


    [Header("Additional Components")]
    public GameObject InputComponents;
    public GameObject VisualizationComponents;




    private InputComponent[] inputComponents;
    //private VisualizationComponent[] visualizationComponents;



    // Use this for initialization
    void Start ()
    {
        Instance = this;
        IslandVizData = GetComponent<IslandVizData>();
        IslandVizVisualization = GetComponent<IslandVizVisualization>();

        inputComponents = InputComponents.GetComponents<InputComponent>();

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
        yield return IslandVizData.ConstructOsgiProject();

        yield return IslandVizVisualization.Construction();

        // Init Input Components
        foreach (var item in inputComponents)
        {
            item.Init();
            yield return null;
        }
    }
	
	
}
