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
    // ################
    // Public - Code
    // ################

    public static IslandVizVisualization Instance { get; private set; } // The instance of this class.
    public Transform VisualizationRoot { get; private set; }
    public List<CartographicIsland> IslandStructures { get; private set; }
    public List<IslandGO> IslandGOs { get; private set; }
    public List<IslandGO> VisibleIslandGOs { get; private set; }
    public VisualizationTransformContainer TransformContainer { get; private set; } // 


    // ################
    // Public - Unity Editor
    // ################

    [Header("Settings")]
    public int RandomSeed; // The seed that is used for all procedrual generations, e.g. the island distribution.
    public Graph_Layout Graph_Layout; // You can change the spatial island distribution method in the editor.

    [Header("Prefabs")]
    public GameObject Water_Plane_Prefab; // Prefab of the water plane in which the islands "swim".
    public GameObject ImportArrowPrefab; //  = (GameObject) Resources.Load("Prefabs/ImportArrow")
    public GameObject ExportArrowPrefab; // = (GameObject) Resources.Load("Prefabs/ExportArrow");
    public GameObject ArrowHeadPrefab; // = (GameObject) Resources.Load("Prefabs/ArrowHead");
    public GameObject ImportDockPrefab; // = (GameObject) Resources.Load("Prefabs/Docks/iDock_1");
    public GameObject ExportDockPrefab; // = (GameObject) Resources.Load("Prefabs/Docks/eDock_1");
    public GameObject[] CUPrefabs; // = Resources.LoadAll<GameObject>("Prefabs/CU/LOD0").ToList<GameObject>();
    public GameObject[] SIPrefabs; // = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD0").ToList<GameObject>();
    public GameObject[] SDPrefabs; // = Resources.LoadAll<GameObject>("Prefabs/ServiceDeclare/LOD0").ToList<GameObject>();

    [Header("Materials")]
    public Material CombinedHoloMaterial; // = (Material) Resources.Load("Materials/Combined HoloMaterial")

    [Header("Environment GameObjects")]
    public GameObject Table; // GameObject of the table.

    [Header("Additional Components Container")]
    public GameObject VisualizationComponentsGameObject; // GameObject containing all additional visualization components, 

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

    [HideInInspector]
    public ZoomLevel CurrentZoomLevel;
    private bool zoomDirty; // This is set to TRUE when the current Zoom was changed (called by a IslandVizInteraction Component).
    private bool islandsDirty; // This is set to TRUE when ZoomLevel changed or when appearing island displays the wrong ZoomLevel.

    // Performance
    private int IslandsPerFrame = 50;


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
    /// Called when the scale of the visualization root was changed.
    /// </summary>
    public ScaleChanged OnVisualizationScaleChanged;
    /// <summary>
    /// Called when the position of the visualization root was changed.
    /// </summary>
    public ScaleChanged OnVisualizationPositionChanged; // TODO
    /// <summary>
    /// Called when a island is set visible.
    /// </summary>
    public IslandEnabled OnIslandVisible;
    /// <summary>
    /// Called when a island is set invisible.
    /// </summary>
    public IslandDisabled OnIslandInvisible;


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
    public delegate void ScaleChanged();
    /// <summary>
    /// Called when the position or rotation of the visualization root was changed.
    /// </summary>
    public delegate void PositionChanged();
    /// <summary>
    /// Called when the island GameObject is enabled.
    /// </summary>
    public delegate void IslandEnabled(IslandGO island);
    /// <summary>
    /// Called when the island GameObject is disabled.
    /// </summary>
    public delegate void IslandDisabled(IslandGO island);






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
    }

    public IEnumerator Init ()
    {
        // Since we stored all additional visualization components in "visualizationComponents", we can add the remaining mandatory visualization components.
        islandGOConstructor = VisualizationComponentsGameObject.AddComponent<IslandGOConstructor>();
        dockGOConstructor = VisualizationComponentsGameObject.AddComponent<DockGOConstructor>();
        //hierarchyConstructor = VisualizationComponentsGameObject.AddComponent<HierarchyConstructor>();
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
        VisibleIslandGOs = new List<IslandGO>();

        // Event subscribtions
        OnTableHeightChanged += ApplyTableHeight;
        OnVisualizationScaleChanged += SetZoomDirty;
        OnIslandVisible += AddVisibleIsland;
        OnIslandInvisible += RemoveInvisibleIsland;

        yield return null;
    }

    /// <summary>
    /// This Coroutine creates the visualization from the OSGI data generated by IslandVizData.
    /// </summary>
    public IEnumerator ConstructVisualization()
    {
        yield return Init();

        stopwatch.Start(); // Start the timer to measure total construction time.
        
        yield return isConstructor.Construct(IslandVizData.Instance.OsgiProject); //Construct CartographicIslands from bundles in the OsgiProject.

        // Construct the spatial distribution of the islands.
        if (Graph_Layout == Graph_Layout.ForceDirected)
        {
            yield return bdConstructor.ConstructFDLayout(IslandVizData.Instance.OsgiProject, 0.25f, 70000, RNG);
        }
        else if (Graph_Layout == Graph_Layout.Random)
        {
            //Vector3 minBounds = new Vector3(-10.5f, 1.31f, -10.5f);
            //Vector3 maxBounds = new Vector3(10.5f, 1.31f, 10.5f);
            Vector3 minBounds = new Vector3(-8.5f, 1.31f, -8.5f);
            Vector3 maxBounds = new Vector3(8.5f, 1.31f, 8.5f);
            yield return bdConstructor.ConstructRndLayout(IslandVizData.Instance.OsgiProject.getDependencyGraph(), minBounds, maxBounds, 0.075f, 10000, RNG);
        }

        IslandStructures = isConstructor.getIslandStructureList();

        // Construct and store the island GameObjects.
        yield return islandGOConstructor.Construct(IslandStructures, TransformContainer.IslandContainer.gameObject);
        IslandGOs = islandGOConstructor.getIslandGOs();
        
        yield return dockGOConstructor.Construct(islandGOConstructor.getIslandGOs(), TransformContainer.IslandContainer.gameObject); // Construct the dock GameObjects.

        // Construct the island hierarchy. TODO enable in the future?
        //yield return hierarchyConstructor.Construct(islandGOConstructor.getIslandGOs());

        yield return AutoZoom(); // TODO solve this with FlyToIsland()

        GlobalVar.CurrentZoom = VisualizationRoot.localScale.x;
        GlobalVar.MinZoom = VisualizationRoot.localScale.x;

        StartCoroutine(ZoomLevelRoutine()); // Start the ZoomLevelRoutine.

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
    public void AddVisibleIsland(IslandGO islandGO)
    {
        if (!VisibleIslandGOs.Contains(islandGO))
        {
            VisibleIslandGOs.Add(islandGO);

            // When a new island with a wrong ZoomLevel appears, the current ZoomLevel needs to be applied to it.
            if (islandGO.CurrentZoomLevel != CurrentZoomLevel)
            {
                islandsDirty = true; // This causes the ZoomLevelRoutine on the next FixedUpdate to check all current visible islands and
                                     // to apply the current zoomlevel if needed.
            }

            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)VisibleIslandGOs.Count/(float)GlobalVar.islandNumber) * 100f); // Update UI.
        }
    }

    /// <summary>
    /// Called by the IslandGO, when the island GameObject has exited the TableContent-Trigger, i.e. has become invisible. 
    /// Removes this island from the current visible islands List.
    /// </summary>
    /// <param name="islandGO">The island that exited the TableContent-Trigger.</param>
    public void RemoveInvisibleIsland(IslandGO islandGO)
    {
        if (VisibleIslandGOs.Contains(islandGO))
        {
            VisibleIslandGOs.Remove(islandGO);

            IslandVizUI.Instance.UpdateCurrentVisibleIslandsUI(((float)VisibleIslandGOs.Count / (float)GlobalVar.islandNumber) * 100f); // Update UI.
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
    public void SetZoomDirty ()
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
        float zoomLevelPercentage = 0; // The current ZoomLevel in %. 
        ZoomLevel zoomLevel;

        zoomDirty = true;
        islandsDirty = true;

        while (true) // Do this forever every FixedUpdate.
        {
            if (zoomDirty) // Zoom was changed.
            {
                zoomDirty = false;              

                zoomLevelPercentage = Mathf.Sqrt(Mathf.Sqrt((GlobalVar.CurrentZoom - GlobalVar.MinZoom) / (GlobalVar.MaxZoom - GlobalVar.MinZoom))) * 100;
                zoomLevel = PercentageToZoomLevel(zoomLevelPercentage);

                // Check if the current ZoomLevel must be changed.
                if (CurrentZoomLevel != zoomLevel)
                {
                    CurrentZoomLevel = zoomLevel;
                    islandsDirty = true;
                }

                IslandVizUI.Instance.UpdateZoomLevelUI(zoomLevelPercentage);
            }
                        
            if (islandsDirty) // Island(s) with wrong ZoomLevel, e.g. because zoomlevel was changed or a new island appeared.
            {
                islandsDirty = false; // This has to be done first, because dirty islands can appear at any point.

                int counter = 0;

                // Check every island and apply current ZoomLevel if needed.
                for (int i = 0; i < VisibleIslandGOs.Count; i++)
                {
                    if (i < VisibleIslandGOs.Count && VisibleIslandGOs[i].CurrentZoomLevel != CurrentZoomLevel) // The first condition is important, because islands can 
                                                                                                                // appear or disappear at any point.
                    {
                        IslandVizUI.Instance.ZoomLevelValue.text = "<color=yellow>" + (i / VisibleIslandGOs.Count) * 100 + " %</color>"; // Give simple feedback on progress // TODO
                        VisibleIslandGOs[i].ApplyZoomLevel(CurrentZoomLevel);
                        counter++;
                    }

                    if (counter >= IslandsPerFrame)
                    {
                        counter = 0;
                        yield return null;
                    }
                }

                IslandVizUI.Instance.UpdateZoomLevelUI(zoomLevelPercentage);
            }

            yield return new WaitForFixedUpdate();
        }
    }
    
    #endregion



    // ################
    // Fly To
    // ################

    public void FlyTo (Transform target)
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

        // When the user is already zoomed in, we do not want to zoom out again.
        if (endScale.x < GlobalVar.CurrentZoom)
        {
            endScale = Vector3.one * GlobalVar.CurrentZoom;
        }

        Vector3 startPosition = VisualizationRoot.position;
        Vector3 endPosition = (startPosition / GlobalVar.CurrentZoom - target.position / GlobalVar.CurrentZoom) * endScale.x;
        endPosition.y = GlobalVar.hologramTableHeight;

        StartCoroutine(FlyToPosition(endPosition, endScale));
    }

    public void FlyTo(Transform[] targets)
    {
        StartCoroutine(FlyToMultiple(targets));
    }
        
    private IEnumerator FlyToMultiple(Transform[] targetTransforms)
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

        Destroy(DebugCube); // TODO this is inly a quick hack! Remove in future;

        //Debug.Log("startScale: " + startScale + " --- endScale: " + endScale);
        //Debug.Log("startPosition: " + startPosition + " --- worldCenterPosition: " + DebugCube.transform.position + " --- endPosition: " + endPosition);
        
        yield return FlyToPosition(endPosition, endScale);
    }

    private IEnumerator FlyToPosition(Vector3 endPosition, Vector3 endScale, float speed = 0.5f)
    {
        Vector3 startScale = Vector3.one * GlobalVar.CurrentZoom;
        Vector3 startPosition = VisualizationRoot.localPosition;
        startPosition.y = GlobalVar.hologramTableHeight;

        if (endScale.x < GlobalVar.MinZoom)
        {
            endScale = Vector3.one * GlobalVar.MinZoom;
        }
        else if (endScale.x > GlobalVar.MaxZoom)
        {
            endScale = Vector3.one * GlobalVar.MaxZoom;
        }

        float value = 0;
        while (value <= 1)
        {
            VisualizationRoot.localScale = Vector3.Lerp(startScale, endScale, value); 
            VisualizationRoot.position = Vector3.Lerp(startPosition, endPosition, value); 
            GlobalVar.CurrentZoom = VisualizationRoot.localScale.x;

            OnVisualizationScaleChanged();

            value += 0.01f * speed;
            yield return new WaitForFixedUpdate();
        }
    }


    // ################
    // Helper Functions
    // ################

    #region HelperFunctions

    public void HighlightAllIslands (bool enable)
    {
        IslandVizInteraction.Instance.OnIslandSelect(null, IslandVizInteraction.SelectionType.Highlight, enable); // Highlight/Unhighlight all islands.

        if (!enable)
            IslandVizInteraction.Instance.OnIslandSelect(null, IslandVizInteraction.SelectionType.Select, false); // Deselect the current island.

        //foreach (var item in IslandGOs)
        //{
        //    IslandVizInteraction.Instance.OnIslandSelect(item, IslandVizInteraction.SelectionType.Highlight, enable);
        //}
    }

    public void HighlightAllDocks(bool enable)
    {
        IslandVizInteraction.Instance.OnDockSelect(null, IslandVizInteraction.SelectionType.Select, enable);
    }

    public void RecenterView ()
    {
        StartCoroutine(FlyToPosition(Vector3.up * GlobalVar.hologramTableHeight, Vector3.one * GlobalVar.MinZoom, 1f));
    }

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

    /// <summary>
    /// Converts a percentage value into a ZoomLevel.
    /// </summary>
    /// <param name="zoomLevelPercent"></param>
    /// <returns></returns>
    private ZoomLevel PercentageToZoomLevel (float zoomLevelPercent)
    {
        if (zoomLevelPercent <= GlobalVar.FarZoomLevelPercent) // ZoomLevel.Far 
        {
            return ZoomLevel.Far;
        }
        else if (zoomLevelPercent <= GlobalVar.MediumZoomLevelPercent) // ZoomLevel.Medium 
        {
            return ZoomLevel.Medium;
        }
        else if (zoomLevelPercent > GlobalVar.MediumZoomLevelPercent) // ZoomLevel.Near 
        {
            return ZoomLevel.Near;
        }

        Debug.LogError("PercentToZoomlevel percent value " + zoomLevelPercent + " + could not be assigend to a ZoomLevel!");
        return ZoomLevel.Near;
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
