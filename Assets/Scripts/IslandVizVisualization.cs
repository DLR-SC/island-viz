using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;
using OsgiViz.Island;
using OsgiViz.Unity.MainThreadConstructors;

public class IslandVizVisualization : MonoBehaviour
{
    [Header("Settings")]
    public int RandomSeed;
    public Graph_Layout Graph_Layout;
    
    [Header("Tranforms")]
    public Transform VisualizationRoot;
    public Transform IslandContainer;
    public Transform DependencyContainer;
    public Transform ServiceSliceContainer;
    public Transform DownwardConnectionContainer;


    private IslandVizData islandVizData;

    private IslandGOConstructor islandGOConstructor;
    private ServiceGOConstructor serviceGOConstructor;
    private DockGOConstructor dockGOConstructor;
    private HierarchyConstructor hierarchyConstructor;
    private IslandStructureConstructor isConstructor;
    private Graph_Layout_Constructor bdConstructor;
    private Neo4jObjConstructor neo4jConstructor;

    private bool waiting = true;

    private System.Random RNG;
    private System.Diagnostics.Stopwatch stopwatch;




    /// <summary>
    /// This Method is called when the application is started.
    /// </summary>
    void Start()
    {
        islandVizData = GetComponent<IslandVizData>();
        
        GameObject visualizationConstructor = new GameObject("VisualizationConstructor");

        islandGOConstructor = visualizationConstructor.AddComponent<IslandGOConstructor>();
        serviceGOConstructor = visualizationConstructor.AddComponent<ServiceGOConstructor>();
        dockGOConstructor = visualizationConstructor.AddComponent<DockGOConstructor>();
        hierarchyConstructor = visualizationConstructor.AddComponent<HierarchyConstructor>();

        isConstructor = new IslandStructureConstructor(1, 2, 8);
        bdConstructor = new Graph_Layout_Constructor();

        RNG = new System.Random(RandomSeed);
        stopwatch = new System.Diagnostics.Stopwatch();
    }

    /// <summary>
    /// This Coroutine creates the OSGI visualization from a JSON file located at projectModelFile.
    /// </summary>
    public IEnumerator Construction()
    {
        yield return null;

        // Start the timer to measure total construction time.
        stopwatch.Start();
                
        //Construct islands from bundles in the osgi Object.
        yield return isConstructor.Construct(islandVizData.osgiProject);

        // Construct the spatial distribution of the islands.
        if (Graph_Layout == Graph_Layout.ForceDirected)
        {
            yield return bdConstructor.ConstructFDLayout(islandVizData.osgiProject, 0.25f, 70000, RNG);
        }
        else
        {
            Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            yield return bdConstructor.ConstructRndLayout(islandVizData.osgiProject.getDependencyGraph(), minBounds, maxBounds, 0.075f, 10000, RNG);
        }

        GlobalVar.islandNumber = islandVizData.osgiProject.getBundles().Count;
        List<CartographicIsland> islandStructures = isConstructor.getIslandStructureList();

        // Construct the island GameObjects.
        yield return islandGOConstructor.Construct(islandStructures, IslandContainer.gameObject);

        // Construct the connections between the islands from services in the osgi Object.
        yield return serviceGOConstructor.Construct(islandVizData.osgiProject.getServices(), islandGOConstructor.getIslandGOs());

        // Construct the dock GameObjects.
        yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), IslandContainer.gameObject);

        // Construct the island hierarchy. TODO enable in the future
        //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

        yield return AutoZoom();

        stopwatch.Stop();
        Debug.Log("IslandVizVisualization Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");
    }


    // Scales the VisualizationContainer, so all islands are visible on start. The CurrentZoomLevel is saved in the GlobalVar.
    IEnumerator AutoZoom()
    {
        Transform furthestIslandTransform = null; // Transfrom of the island which is furthest away from the center.
        float furthestDistance = 0; // Furthest distance of a island to the center.
        float distance_temp = 0;

        float maxDistance = 0.7f; // TODO move to Settings

        // Search island which is furthest away from the center.
        foreach (var islandGO in islandGOConstructor.getIslandGOs())
        {
            distance_temp = Vector3.Distance(islandGO.transform.position, Vector3.zero);
            if (furthestIslandTransform == null || distance_temp > furthestDistance)
            {
                furthestDistance = distance_temp;
                furthestIslandTransform = islandGO.transform;
            }
        }

        yield return null;

        IslandContainer.localScale *= maxDistance / furthestDistance; // Scales the islands to make all of them fit on the table.
        GlobalVar.CurrentZoomLevel = IslandContainer.localScale.x;
        GlobalVar.MinZoomLevel = IslandContainer.localScale.x;
    }

}

public enum Graph_Layout
{
    ForceDirected,
    Random
}
