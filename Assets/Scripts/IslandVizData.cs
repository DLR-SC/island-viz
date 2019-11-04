using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;
using OsgiViz.Island;
using OsgiViz.Unity.MainThreadConstructors;
using System.Linq;

/// <summary>
/// This class handles all abstract data the IslandViz visualizes.
/// </summary>
public class IslandVizData : MonoBehaviour
{
    public static IslandVizData Instance; // The instance of this class.
    
    public DataLoadingType DataLoading;
    public string DataLocation;

    [HideInInspector]
    public OsgiProject OsgiProject;

    private Neo4jObjConstructor neo4jConstructor;
    private JsonObjConstructor jConstructor;
    private OsgiProjectConstructor osgiConstructor;

    private System.Diagnostics.Stopwatch stopwatch;

    bool waiting = true; // This only exists for the JsonObjConstructor. TODO remove in future.



    // ################
    // Initiation
    // ################

    #region Initiation

    /// <summary>
    /// Called by Unity on application start up before the Start() method.
    /// </summary>
    void Awake()
    {
        Instance = this;

        GameObject neo4jObject = new GameObject("Neo4j");
        neo4jObject.transform.parent = IslandVizBehaviour.Instance.transform;
        neo4jConstructor = neo4jObject.AddComponent<Neo4jObjConstructor>();
        jConstructor = new JsonObjConstructor();
        osgiConstructor = new OsgiProjectConstructor();
        stopwatch = new System.Diagnostics.Stopwatch();
    }


    public IEnumerator ConstructOsgiProject()
    {
        yield return null;

        // Start the timer to measure construction time.
        stopwatch.Start();

        // TODO
        // yield return neo4jConstructor.Construct();

        if (DataLoading == DataLoadingType.Json)
        {
            jConstructor.Construct(GlobalVar.projectmodelPath, Done); // Read & construct a Json Object.

            // Wait for jConstructor.Construct. TODO remove in future
            while (waiting)
                yield return null;

            yield return osgiConstructor.Construct(jConstructor.getJsonModel()); // Construct a osgi Object from the Json Object.
        }
        else if (DataLoading == DataLoadingType.Neo4J)
        {
            // Read & construct a Json Object.
            yield return neo4jConstructor.Construct();
            // Construct a osgi Object from the Json Object.
            yield return osgiConstructor.Neo4jConstruct(neo4jConstructor.GetNeo4JModel()); // Construct a osgi Object from the neo4J Object.
        }


            

        // Store the osgi data
        OsgiProject = osgiConstructor.getProject();
        GlobalVar.islandNumber = OsgiProject.getBundles().Count;

        // Debug
        //List<OsgiViz.Relations.GraphVertex> allVertices = OsgiProject.getDependencyGraph().Vertices.ToList();
        //foreach (var item in allVertices)
        //{
        //    Debug.Log(item.getName());
        //}
        //Debug.Log(allVertices.Count);
        //Debug.Log(OsgiProject.getBundles().Count);
        //Debug.Log(OsgiProject.getServices().Count);

        stopwatch.Stop();
        Debug.Log("IslandVizData Construction finished after " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds!");
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

    public enum DataLoadingType
    {
        Json,
        Neo4J
    }

}
