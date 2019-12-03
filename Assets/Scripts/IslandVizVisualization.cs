﻿using System.Collections;
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

    private List<IslandGO> visibleIslandGOs;

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

        // Get all optimal additional visualization components.
        visualizationComponents = VisualizationComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();

        // Since we stored all additional visualization components in "visualizationComponents", we can add the remaining mandatory visualization components.
        islandGOConstructor = VisualizationComponentsGameObject.AddComponent<IslandGOConstructor>();
        dockGOConstructor = VisualizationComponentsGameObject.AddComponent<DockGOConstructor>();
        hierarchyConstructor = VisualizationComponentsGameObject.AddComponent<HierarchyConstructor>();

        isConstructor = new IslandStructureConstructor(1, 2, 8);
        bdConstructor = new Graph_Layout_Constructor();

        // Create root transforms and add some stuff.
        TransformContainer = new VisualizationTransformContainer();
        VisualizationRoot = new GameObject("Visualization").transform;
        TransformContainer.IslandContainer = new GameObject("VisualizationContainer").transform;
        TransformContainer.IslandContainer.SetParent(VisualizationRoot);
        TransformContainer.DependencyContainer = new GameObject("DependencyContainer").transform;
        TransformContainer.DependencyContainer.SetParent(VisualizationRoot);
        
        // Create water visual.
        GameObject water = (GameObject)Instantiate(Water_Plane_Prefab, TransformContainer.IslandContainer);
        water.name = Water_Plane_Prefab.name; // Just making sure since there are still a alot GameObject.Find... TODO: remove in future
        water.transform.localPosition = Vector3.zero;
        water.transform.localScale = new Vector3(1000, 1, 1000);

        RNG = new System.Random(RandomSeed);
        stopwatch = new System.Diagnostics.Stopwatch();
        visibleIslandGOs = new List<IslandGO>();

        OnTableHeightChanged += ApplyTableHeight;
    }

    /// <summary>
    /// This Coroutine creates the visualization from the OSGI data generated by IslandVizData.
    /// </summary>
    public IEnumerator ConstructVisualization()
    {
        yield return null;

        stopwatch.Start(); // Start the timer to measure total construction time.
        
        yield return isConstructor.Construct(IslandVizData.Instance.OsgiProject); //Construct CartographicIslands from bundles in the OsgiProject.

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

        IslandStructures = isConstructor.getIslandStructureList();

        // Construct and store the island GameObjects.
        yield return islandGOConstructor.Construct(IslandVizVisualization.Instance.IslandStructures, TransformContainer.IslandContainer.gameObject);
        IslandGameObjects = islandGOConstructor.getIslandGOs();
        
        yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), TransformContainer.IslandContainer.gameObject); // Construct the dock GameObjects.

        // Construct the island hierarchy. TODO enable in the future?
        //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

        yield return AutoZoom(); // TODO solve this with FlyToIsland()
        
        StartCoroutine(ZoomLevelRoutine()); // Starts the ZoomLevelRoutine.

        // TODO reenable in a smarter way
        //GlobalVar.hologramTableHeight = IslandVizInteraction.Instance.GetPlayerEyeHeight() - 0.75f; 
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
            if (item.enabled)
                yield return item.Init();
        }
    }

    #endregion



    // ################
    // Current Visible Islands Handling
    // ################

    #region Current Visible Islands Handling

    /// <summary>
    /// Called by the IslandGO, when the island GameObject has entered the TableContent-Trigger, i.e. has become visible. 
    /// Adds this island to the current visible islands List and sets islandDirty=true.
    /// </summary>
    /// <param name="islandGO">The island that entered the TableContent-Trigger.</param>
    public void MakeIslandVisible(IslandGO islandGO)
    {
        if (!visibleIslandGOs.Contains(islandGO))
        {
            visibleIslandGOs.Add(islandGO);

            // When a new island with a wrong ZoomLevel appears, the current ZoomLevel needs to be applied to it.
            if (islandGO.CurrentZoomLevel != CurrentZoomLevel)
            {
                islandsDirty = true; // This causes the ZoomLevelRoutine on the next FixedUpdate to check all current visible islands and
                                     // to apply the current zoomlevel if needed.
            }

            // Enable all children, i.e. make island visible.
            for (int i = 0; i < islandGO.transform.childCount; i++)
            {
                islandGO.transform.GetChild(i).gameObject.SetActive(true);
            }

            islandGO.OnIslandVisible?.Invoke();

            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)visibleIslandGOs.Count/(float)GlobalVar.islandNumber) * 100f); // Update UI.
            //Debug.Log(currentIslands.Count);
        }
    }

    /// <summary>
    /// Called by the IslandGO, when the island GameObject has exited the TableContent-Trigger, i.e. has become invisible. 
    /// Removes this island from the current visible islands List.
    /// </summary>
    /// <param name="islandGO">The island that exited the TableContent-Trigger.</param>
    public void MakeIslandInvisible(IslandGO islandGO)
    {
        if (visibleIslandGOs.Contains(islandGO))
        {
            visibleIslandGOs.Remove(islandGO);

            // Disable all children, i.e. make island invisible.
            for (int i = 0; i < islandGO.transform.childCount; i++)
            {
                islandGO.transform.GetChild(i).gameObject.SetActive(false);
            }

            islandGO.OnIslandInvisible?.Invoke();

            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)visibleIslandGOs.Count / (float)GlobalVar.islandNumber) * 100f); // Update UI.
            //Debug.Log(currentIslands.Count);
        }
    }

    #endregion



    // ################
    // Zoom Level
    // ################

    #region Zoom Level

    /// <summary>
    /// Called by (additional) input components when the visualization was zoomed in or out.
    /// </summary>
    public void OnVisualizationScaleChanged ()
    {
        zoomDirty = true;
    }

    /// <summary>
    /// Called at the end of the ConstructVisualization() Coroutine.
    /// The ZoomLevelRoutine() Coroutine handles both the "zoomDirty" and the "islandDirty" flags. When the zoom was changed, i.e. zoomDirty = true, 
    /// it checks if a new zoomLevel was entered. If so, this coroutine changes the current zoomLevel and sets the flag, that the
    /// current visible islands must be checked wether they have the correct zoomLevel, i.e. islandDirty = true. 
    /// Checking the flags happens every every Unity FixedUpdate (https://docs.unity3d.com/2019.3/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html).
    /// </summary>
    IEnumerator ZoomLevelRoutine ()
    {
        float zoomLevelPercent = 0; // The current ZoomLevel in %. 

        zoomDirty = true;
        islandsDirty = true;

        while (true) // Do this forever every FixedUpdate.
        {
            if (zoomDirty) // Zoom was changed.
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
                        
            if (islandsDirty) // Island(s) with wrong ZoomLevel.
            {
                islandsDirty = false; // This has to be done first, because dirty islands can appear at any point.

                // Check every island and apply current ZoomLevel if needed.
                for (int i = 0; i < visibleIslandGOs.Count; i++)
                {
                    if (i < visibleIslandGOs.Count && visibleIslandGOs[i].CurrentZoomLevel != CurrentZoomLevel) // The first condition is important, because islands can 
                                                                                                                // appear or disappear at any point.
                    {
                        IslandVizUI.Instance.ZoomLevelValue.text = "<color=yellow>" + (i / visibleIslandGOs.Count) * 100 + " %</color>"; // Give simple feedback on progress // TODO
                        yield return visibleIslandGOs[i].ApplyZoomLevel(CurrentZoomLevel);
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
        Vector3 endScale = Vector3.one;
        if (target.GetComponent<IslandGO>() != null)
        {
            endScale *= GlobalVar.MinZoom * 5;
        }
        else if (target.GetComponent<Region>() != null)
        {
            endScale *= GlobalVar.MaxZoom / 4;
        }
        else if (target.GetComponent<Building>() != null)
        {
            endScale *= GlobalVar.MaxZoom / 2;
        }
        else
        {
            endScale *= GlobalVar.MinZoom * 4;
        }

        Vector3 startPosition = VisualizationRoot.position;
        Vector3 endPosition = (startPosition / GlobalVar.CurrentZoom - target.position / GlobalVar.CurrentZoom) * endScale.x;
        endPosition.y = GlobalVar.hologramTableHeight;

        StartCoroutine(FlyToPosition(endPosition, endScale));
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

            IslandVizVisualization.Instance.OnVisualizationScaleChanged();

            value += 0.01f * speed;
            yield return new WaitForFixedUpdate();
        }
    }

    // TODO remove in future! Only for Debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(FlyToMultiple(new Transform[] { IslandGameObjects[0].transform, IslandGameObjects[1].transform}));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //List<Transform> islands = new List<Transform>();
            //foreach (var item in IslandGameObjects)
            //{
            //    islands.Add(item.transform);
            //}
            //SelectAndFlyTo(islands.ToArray());
            IslandSelectionComponent.Instance.SelectIslands(IslandGameObjects);
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
