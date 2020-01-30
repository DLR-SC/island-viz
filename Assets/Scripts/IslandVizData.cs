using System.Collections;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;

/// <summary>
/// This class loads and handles all abstract data that will be visualized.
/// </summary>
public class IslandVizData : MonoBehaviour
{
    public static IslandVizData Instance { get { return instance; } } 
    public OsgiProject OsgiProject { get { return osgiProject; } }

    // Set in Unity Editor
    public DataLoadingType DataLoading; // Wether the osgi project is loaded from a json file or a neo4j database.
    //public string DataLocation; // TODO

    [Header("Additional Components Container")]
    public GameObject DataComponentsGameObject; // GameObject where all additional data components are located. 

    private AdditionalIslandVizComponent[] inputComponents; // Array of all additional data componets.

    private static IslandVizData instance; // The instance of this class.
    private OsgiProject osgiProject; // The OSGI project.

    // Constructors
    private JsonObjConstructor jConstructor; // JsonFile->JsonObject constructor.
    private OsgiProjectConstructor jsonOsgiProjectConstructor; // JsonObject->OsgiProject constructor.
    private Neo4jOsgiConstructor neo4jOsgiProjectConstructor; // Neo4J->OsgiProject constructor.

    private bool waiting = true; // This only exists for the JsonObjConstructor. TODO remove in future.



    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Called by Unity on application start up before the Start() method. 
    /// This only initiates constructors and creates the neo4j component container.
    /// </summary>
    void Awake()
    {
        instance = this;
        inputComponents = DataComponentsGameObject.GetComponents<AdditionalIslandVizComponent>();
    }

    /// <summary>
    /// Called by IslandVizBehavior.IslandVizConstructionRoutine(). 
    /// This creates a OsgiProject from either a json file or a neo4j database.
    /// </summary>
    public IEnumerator ConstructOsgiProject()
    {
        yield return null;

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch.Start(); // Timer to measure construction time.

        if (DataLoading == DataLoadingType.Json)
        {
            jConstructor = new JsonObjConstructor(); // Initiate Json->JsonObject constructor.
            jsonOsgiProjectConstructor = new OsgiProjectConstructor(); // Initiate JsonObject->OsgiProject constructor.

            IslandVizUI.Instance.UpdateLoadingScreenUI("OSGi-Project from Json", ""); // Update UI.

            jConstructor.Construct(GlobalVar.projectmodelPath, Done); // Read & construct a Json Object.
                        
            while (waiting) // Wait for jConstructor.Construct. TODO remove in future
                yield return null;

            yield return jsonOsgiProjectConstructor.Construct(jConstructor.getJsonModel()); // Construct a OsgiProject from the JsonObject.
            osgiProject = jsonOsgiProjectConstructor.getProject(); 
        }
        else if (DataLoading == DataLoadingType.Neo4J)
        {
            GameObject neo4jObject = new GameObject("Neo4j");
            neo4jObject.transform.parent = IslandVizBehaviour.Instance.transform;
            neo4jOsgiProjectConstructor = neo4jObject.AddComponent<Neo4jOsgiConstructor>(); // Initiate Neo4J->OsgiProject constructor.

            IslandVizUI.Instance.UpdateLoadingScreenUI("Connecting to Neo4J", ""); // Update UI.

            yield return new WaitForSeconds(1f); // We need to wait a little bit until the Neo4jOsgiConstructor connected to the neo4j server. 

            //yield return neo4jOsgiProjectConstructor.Construct(); // Construct a osgi Object from the neo4J database.
            yield return neo4jOsgiProjectConstructor.Construct(802); // Construct a osgi Object from the neo4J database.
            osgiProject = neo4jOsgiProjectConstructor.GetOsgiProject();
        }

        GlobalVar.islandNumber = osgiProject.getBundles().Count;
        
        stopwatch.Stop();
        Debug.Log("IslandVizData Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");
    }


    /// <summary>
    /// Initialize all input components. Called by IslandVizBehavior.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitInputComponents()
    {
        foreach (var item in inputComponents)
        {
            if (item.enabled)
                yield return item.Init();
        }
    }

    #endregion


    // ################
    // Helper Functions
    // ################

    #region Helper Functions   

    // Remove in future
    public void Done()
    {
        waiting = false;
    }

    #endregion




    // ################
    // Enums
    // ################

    #region Enums 

    /// <summary>
    /// The type of data loading.
    /// </summary>
    public enum DataLoadingType
    {
        Json,
        Neo4J
    }

    #endregion
}
