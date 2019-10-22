using OsgiViz.Unity.MainThreadConstructors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceVisualization : AdditionalIslandVizComponent
{

    private ServiceGOConstructor serviceGOConstructor;


    public override void Init()
    {
        serviceGOConstructor = gameObject.AddComponent<ServiceGOConstructor>();

        // Construct the connections between the islands from services in the osgi Object.
        StartCoroutine(serviceGOConstructor.Construct(IslandVizData.Instance.OsgiProject.getServices(), IslandVizVisualization.Instance.IslandGameObjects));
    }
}
