using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsgiViz.SideThreadConstructors;
using OsgiViz.Core;
using OsgiViz.Island;
using OsgiViz.Unity.MainThreadConstructors;

/// <summary>
/// This class handles all abstract data the IslandViz visualizes.
/// </summary>
public class IslandVizData : MonoBehaviour
{
    public static IslandVizData Instance; // The instance of this class.

    [HideInInspector]
    public OsgiProject OsgiProject;

    private Neo4jObjConstructor neo4jConstructor;
    private JsonObjConstructor jConstructor;
    private OsgiProjectConstructor osgiConstructor;

    private bool waiting = true; // This only exists for the JsonObjConstructor. TODO remove in future.
    private System.Diagnostics.Stopwatch stopwatch;



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

        // TODO add in future
        //yield return neo4jConstructor.Construct();

        #region Remove in future
        // Read & construct a Json Object.
        jConstructor.Construct(GlobalVar.projectmodelPath, Done);
        // Wait for jConstructor.Construct.
        while (waiting)
            yield return null;
        #endregion

        // Construct a osgi Object from the Json Object.
        yield return osgiConstructor.Construct(jConstructor.getJsonModel()); // neo4jConstructor.GetNeo4JModel()

        // Store the osgi data
        OsgiProject = osgiConstructor.getProject();
        GlobalVar.islandNumber = OsgiProject.getBundles().Count;
        
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

}
