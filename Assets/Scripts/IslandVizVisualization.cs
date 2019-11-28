using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;
using OsgiViz.Island;
using OsgiViz.Unity.MainThreadConstructors;
using OsgiViz.Unity.Island;

/// <summary>
/// This class handles the basic software visualization. 
/// Additional visualization stuff if handled by the IslandVizBehavior through its 
/// VisualizationComponents.
/// </summary>
public class IslandVizVisualization : MonoBehaviour
{
    public static IslandVizVisualization Instance; // The instance of this class.

    [Header("Settings")]
    public int RandomSeed; // The seed that is used for all procedrual generations, e.g. the island distribution.
    public Graph_Layout Graph_Layout; // You can change the spatial island distribution method in the editor.

    [Header("Prefabs")]
    public GameObject Water_Plane_Prefab; // Prefab of the water plane in which the islands "swim".

    [Header("Environment GameObjects")]
    public GameObject Table; // GameObject of the table.

    [Header("Additional Components Container")]
    public GameObject VisualizationComponentsGameObject; // GameObject containing all additional visualization components, 
                                                         // that are being loaded at the end of the IslandViz construction.

    [HideInInspector]
    public VisualizationTransformContainer TransformContainer; // 

    [HideInInspector]
    public Transform VisualizationRoot;

    [HideInInspector]
    public List<CartographicIsland> IslandStructures;

    [HideInInspector]
    public List<IslandGO> IslandGameObjects;

    // Mandatory coomonents for visualization.
    private IslandGOConstructor islandGOConstructor;
    private DockGOConstructor dockGOConstructor;
    private HierarchyConstructor hierarchyConstructor;
    private IslandStructureConstructor isConstructor;
    private Graph_Layout_Constructor bdConstructor;
    private Neo4jOsgiConstructor neo4jConstructor;

    private AdditionalIslandVizComponent[] visualizationComponents; // Array of all additional input componets.

    private System.Random RNG;
    private System.Diagnostics.Stopwatch stopwatch;

    private List<IslandGO> currentIslands;

    public ZoomLevel CurrentZoomLevel;
    private bool zoomDirty; // This is set to TRUE when the current Zoom was changed (called by a IslandVizInteraction Component).
    private bool islandsDirty; // This is set to TRUE when ZoomLevel changed or when appearing island displays the wrong ZoomLevel.




    // ################
    // Events
    // ################

    /// <summary>
    /// Called when the table hight was changed.
    /// </summary>
    public TableHeightChanged OnTableHeightChanged;
    /// <summary>
    /// Called when the zoom level of the visualization has changed.
    /// </summary>
    public ZoomLevelChanged OnZoomLevelChanged;
    /// <summary>
    /// Called when the position or rotation of the visualization root was changed.
    /// </summary>
    public TransformChanged OnTransformChanged;


    // ################
    // Delegates
    // ################

    /// <summary>
    /// Called when the table hight was changed.
    /// </summary>
    /// <param name="newHeight">The new height of the table.</param>
    public delegate void TableHeightChanged(float newHeight);
    /// <summary>
    /// Called when the zoom level has changed.
    /// </summary>
    /// <param name="newZoomLevel">.</param>
    public delegate void ZoomLevelChanged(ZoomLevel newZoomLevel);
    /// <summary>
    /// Called when the position or rotation of the visualization root was changed.
    /// </summary>
    public delegate void TransformChanged();







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

        // Get all optimal additional visualization components
        visualizationComponents = VisualizationComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();

        // Since we saved all additional visualization components in "visualizationComponents", we can add the remaining mandatory visualization components.
        islandGOConstructor = VisualizationComponentsGameObject.AddComponent<IslandGOConstructor>();
        dockGOConstructor = VisualizationComponentsGameObject.AddComponent<DockGOConstructor>();
        hierarchyConstructor = VisualizationComponentsGameObject.AddComponent<HierarchyConstructor>();

        isConstructor = new IslandStructureConstructor(1, 2, 8);
        bdConstructor = new Graph_Layout_Constructor();

        // Create root transforms and add some stuff
        TransformContainer = new VisualizationTransformContainer();
        VisualizationRoot = new GameObject("Visualization").transform;
        TransformContainer.IslandContainer = new GameObject("VisualizationContainer").transform;
        TransformContainer.IslandContainer.SetParent(VisualizationRoot);
        TransformContainer.DependencyContainer = new GameObject("DependencyContainer").transform;
        TransformContainer.DependencyContainer.SetParent(VisualizationRoot);
        
        // Create water visual
        GameObject water = (GameObject)Instantiate(Water_Plane_Prefab, TransformContainer.IslandContainer);
        water.name = Water_Plane_Prefab.name; // Just making sure since there are still a alot GameObject.Find... TODO: remove in future
        water.transform.localPosition = Vector3.zero;
        water.transform.localScale = new Vector3(1000, 1, 1000);

        RNG = new System.Random(RandomSeed);
        stopwatch = new System.Diagnostics.Stopwatch();

        OnTableHeightChanged += ApplyTableHeight;

        currentIslands = new List<IslandGO>();
    }

    /// <summary>
    /// This Coroutine creates the visualization from the OSGI data generated by IslandVizData.
    /// </summary>
    public IEnumerator ConstructVisualization()
    {
        yield return null;

        stopwatch.Start(); // Start the timer to measure total construction time.
        
        yield return isConstructor.Construct(IslandVizData.Instance.OsgiProject); //Construct islands from bundles in the osgi Object.

        // Construct the spatial distribution of the islands.
        if (Graph_Layout == Graph_Layout.ForceDirected)
        {
            yield return bdConstructor.ConstructFDLayout(IslandVizData.Instance.OsgiProject, 0.25f, 70000, RNG);
        }
        else if (Graph_Layout == Graph_Layout.Random)
        {
            Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            yield return bdConstructor.ConstructRndLayout(IslandVizData.Instance.OsgiProject.getDependencyGraph(), minBounds, maxBounds, 0.075f, 10000, RNG);
        }

        GlobalVar.islandNumber = IslandVizData.Instance.OsgiProject.getBundles().Count;
        IslandStructures = isConstructor.getIslandStructureList();

        // Construct and store the island GameObjects.
        yield return islandGOConstructor.Construct(IslandVizVisualization.Instance.IslandStructures, TransformContainer.IslandContainer.gameObject);
        IslandGameObjects = islandGOConstructor.getIslandGOs();
        
        yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), TransformContainer.IslandContainer.gameObject); // Construct the dock GameObjects.

        // Construct the island hierarchy. TODO enable in the future?
        //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

        yield return AutoZoom();
        
        StartCoroutine(ZoomLevelRoutine()); // Starts the ZoomLevelRoutine.
        
        //GlobalVar.hologramTableHeight = IslandVizInteraction.Instance.GetPlayerEyeHeight() - 0.75f; // TODO reenable
        OnTableHeightChanged(GlobalVar.hologramTableHeight); // Set table height

        stopwatch.Stop();
        Debug.Log("IslandVizVisualization Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");
    }       

    /// <summary>
    /// Initialize all additional input components. Called by IslandVizBehavior.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitVisualizationComponents()
    {
        foreach (var item in visualizationComponents)
        {
            yield return item.Init();
        }
    }

    #endregion



    // ################
    // Current Visible Islands Handling
    // ################

    #region Current Visible Islands Handling

    /// <summary>
    /// Called when a island GameObject has entered the TableContent-Trigger, i.e. has become visible. Adds this island to the currentIslands List.
    /// </summary>
    /// <param name="islandGO">The island that entered the TableContent-Trigger.</param>
    public void AddCurrentIsland(IslandGO islandGO)
    {
        if (!currentIslands.Contains(islandGO))
        {
            currentIslands.Add(islandGO);

            // When a new island with a wrong ZoomLevel appears, the current ZoomLevel needs to be applied to it.
            if (islandGO.CurrentZoomLevel != CurrentZoomLevel)
            {
                islandsDirty = true;
            }

            for (int i = 0; i < islandGO.transform.childCount; i++)
            {
                islandGO.transform.GetChild(i).gameObject.SetActive(true);
            }

            if (islandGO.OnIslandEnabled != null)
            {
                islandGO.OnIslandEnabled();
            }

            //Debug.Log(currentIslands.Count);
            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)currentIslands.Count/(float)GlobalVar.islandNumber) * 100f);
        }
    }

    /// <summary>
    /// Called when a island GameObject has exited the TableContent-Trigger, i.e. has become invisible. Removes this island from the currentIslands List.
    /// </summary>
    /// <param name="islandGO">The island that exited the TableContent-Trigger.</param>
    public void RemoveCurrentIsland(IslandGO islandGO)
    {
        if (currentIslands.Contains(islandGO))
        {
            currentIslands.Remove(islandGO);
            //Debug.Log(currentIslands.Count);

            for (int i = 0; i < islandGO.transform.childCount; i++)
            {
                islandGO.transform.GetChild(i).gameObject.SetActive(false);
            }

            if (islandGO.OnIslandDisabled != null)
            {
                islandGO.OnIslandDisabled();
            }

            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)currentIslands.Count / (float)GlobalVar.islandNumber) * 100f);
        }
    }

    #endregion



    // ################
    // Zoom Level
    // ################

    #region Zoom Level

    /// <summary>
    /// Called when the visualization was zoomed in or out.
    /// </summary>
    public void ZoomChanged ()
    {
        zoomDirty = true;
    }    

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ZoomLevelRoutine ()
    {
        float zoomLevelPercent = 0; // The current ZoomLevel in %. 

        zoomDirty = true;
        islandsDirty = true;

        while (true)
        {
            // Zoom was changed.
            if (zoomDirty) 
            {
                zoomDirty = false; // This has to be done first, because the zoom can be changed at any point.               

                // Calculate current zoom level in percent
                zoomLevelPercent = Mathf.Sqrt(Mathf.Sqrt((GlobalVar.CurrentZoom - GlobalVar.MinZoom) / (GlobalVar.MaxZoom - GlobalVar.MinZoom))) * 100;

                // Check if the ZoomLevel changed.
                if (zoomLevelPercent <= GlobalVar.FarZoomLevelPercent && CurrentZoomLevel != ZoomLevel.Far)
                {
                    CurrentZoomLevel = ZoomLevel.Far;
                    islandsDirty = true;
                }
                else if (zoomLevelPercent > GlobalVar.FarZoomLevelPercent && zoomLevelPercent <= GlobalVar.MediumZoomLevelPercent && CurrentZoomLevel != ZoomLevel.Medium)
                {
                    CurrentZoomLevel = ZoomLevel.Medium;
                    islandsDirty = true;
                }
                else if (zoomLevelPercent > GlobalVar.MediumZoomLevelPercent && CurrentZoomLevel != ZoomLevel.Near)
                {                    
                    CurrentZoomLevel = ZoomLevel.Near;
                    islandsDirty = true;
                }

                IslandVizUI.Instance.UpdateZoomLevelUI(zoomLevelPercent);
                
                // Debug
                //Debug.Log(GlobalVar.MinZoomLevel + " - " + GlobalVar.CurrentZoomLevel + " - " + GlobalVar.MaxZoomLevel + " -> " + zoomPercent + "%");
            }

            // Island with wrong ZoomLevel.
            if (islandsDirty) 
            {
                islandsDirty = false; // This has to be done first, because dirty islands can appear at any point.

                //Debug.Log(CurrentZoomLevel);

                // Apply current ZoomLevel to all current islands.
                for (int i = 0; i < currentIslands.Count; i++)
                {
                    if (i < currentIslands.Count && currentIslands[i].CurrentZoomLevel != CurrentZoomLevel) // The first condition is important, because islands can 
                                                                                                            // appear or disappear at any point.
                    {
                        IslandVizUI.Instance.ZoomLevelValue.text = "<color=yellow>" + (i / currentIslands.Count) * 100 + " %</color>"; // Give simple feedback on progress // TODO
                        yield return currentIslands[i].ApplyZoomLevel(CurrentZoomLevel);
                    }
                }

                IslandVizUI.Instance.UpdateZoomLevelUI(zoomLevelPercent);
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    #endregion



    // ################
    // Selection
    // ################

    public void SelectAndFlyTo (Transform target)
    {
        Debug.Log(target.position);
    }

    public void SelectAndFlyTo(Transform[] targets)
    {
        List<IslandGO> islands = new List<IslandGO>();

        foreach (var item in targets)
        {
            if (item.GetComponent<IslandGO>() != null)
            {
                islands.Add(item.GetComponent<IslandGO>());
            }
        }

        IslandSelectionComponent.Instance.SelectIslands(islands);

        StartCoroutine(FlyToMultiple(targets));
    }

    IEnumerator FlyToPosition(Vector3 endPosition, Vector3 endScale, float speed = 0.5f)
    {
        Vector3 startScale = Vector3.one * GlobalVar.CurrentZoom;
        Vector3 startPosition = VisualizationRoot.localPosition;
        startPosition.y = GlobalVar.hologramTableHeight;

        float value = 0;
        while (value <= 1)
        {
            VisualizationRoot.localScale = Vector3.Lerp(startScale, endScale, value);
            VisualizationRoot.position = Vector3.Lerp(startPosition, endPosition, value);
            GlobalVar.CurrentZoom = VisualizationRoot.localScale.x;

            value += 0.01f * speed;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator FlyToMultiple(Transform[] targetTransforms)
    {
        float distance;
        Vector3 centerLocalPosition = FindCentroid(targetTransforms, out distance);
        Vector3 centerWorldPosition = centerLocalPosition * GlobalVar.CurrentZoom + VisualizationRoot.transform.position;

        float zoomMultiplier = 0.75f / (GlobalVar.CurrentZoom * distance);        
        Vector3 startScale = Vector3.one * GlobalVar.CurrentZoom;
        Vector3 endScale = startScale * zoomMultiplier;

        // Debug
        GameObject DebugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DebugCube.transform.localScale = new Vector3(0.01f, 100, 0.01f);
        DebugCube.transform.parent = VisualizationRoot;
        DebugCube.transform.localPosition = centerLocalPosition;
        Debug.Log("DebugCube Position: " + DebugCube.transform.position + " --- WorldPosition: " + centerWorldPosition);
        
        Vector3 startPosition = VisualizationRoot.position;
        Vector3 endPosition = (startPosition / GlobalVar.CurrentZoom - DebugCube.transform.position / GlobalVar.CurrentZoom) * endScale.x;
        endPosition.y = startPosition.y;

        Debug.Log("startScale: " + startScale + " --- endScale: " + endScale);
        Debug.Log("startPosition: " + startPosition + " --- worldCenterPosition: " + DebugCube.transform.position + " --- endPosition: " + endPosition);

        Destroy(DebugCube); // TODO this is inly a quick hack! Remove in future;

        yield return FlyToPosition(endPosition, endScale);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(FlyToMultiple(new Transform[] { IslandGameObjects[0].transform, IslandGameObjects[1].transform}));
        }
    }


    // ################
    // Helper Functions
    // ################

    #region HelperFunctions

    /// <summary>
    /// Scales the VisualizationContainer, so all islands are visible on start. The CurrentZoomLevel is saved in the GlobalVar.
    /// </summary>
    /// <returns></returns>
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

        VisualizationRoot.localScale *= maxDistance / furthestDistance;
        GlobalVar.CurrentZoom = VisualizationRoot.localScale.x;
        GlobalVar.MinZoom = VisualizationRoot.localScale.x;
    }

    /// <summary>
    /// Moves the table to a new hight.
    /// </summary>
    /// <param name="newHeight">The new height (in meters) of the table.</param>
    public void ApplyTableHeight (float newHeight)
    {
        Table.transform.position = new Vector3(Table.transform.position.x, newHeight, Table.transform.position.z);
        VisualizationRoot.transform.position = new Vector3(VisualizationRoot.transform.position.x, newHeight, VisualizationRoot.transform.position.z);
        GlobalVar.hologramTableHeight = newHeight;
    }

    private Vector3 FindCentroid(Transform[] targets, out float distance)
    {
        Vector3 centroid;
        Vector3 minPoint = targets[0].localPosition;
        Vector3 maxPoint = targets[0].localPosition;

        for (int i = 1; i < targets.Length; i++)
        {
            Vector3 pos = targets[i].localPosition;
            if (pos.x < minPoint.x)
                minPoint.x = pos.x;
            if (pos.x > maxPoint.x)
                maxPoint.x = pos.x;
            if (pos.y < minPoint.y)
                minPoint.y = pos.y;
            if (pos.y > maxPoint.y)
                maxPoint.y = pos.y;
            if (pos.z < minPoint.z)
                minPoint.z = pos.z;
            if (pos.z > maxPoint.z)
                maxPoint.z = pos.z;
        }

        centroid = minPoint + 0.5f * (maxPoint - minPoint);

        distance = Vector3.Distance(maxPoint, minPoint);

        return centroid;
    }

    #endregion
}

public enum Graph_Layout
{
    ForceDirected,
    Random
}

public enum ZoomLevel
{
    Near,
    Medium,
    Far
}

public class VisualizationTransformContainer
{
    public Transform IslandContainer;
    public Transform DependencyContainer;
}
