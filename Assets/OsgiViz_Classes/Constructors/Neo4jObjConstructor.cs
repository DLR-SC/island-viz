using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver;
using System.Linq;
using System;
using Neo4j.Driver.V1;

/// <summary>
/// THIS ONLY WORKS IN UNITY 2019!
/// </summary>
public class Neo4jObjConstructor : MonoBehaviour{

    private Neo4JDriver.Neo4J neo4j;
    private IStatementResult neo4jModel;

    
    private void Start()
    {
        neo4j = new Neo4JDriver.Neo4J("bolt://localhost:7687", "neo4j", "123"); 
    }

    /// <summary>
    /// Extracts the data from the Neo4J server.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Construct() // TODO
    {
        string release = "";

        yield return null;
        try
        {
            //IStatementResult result = neo4j.Transaction("MATCH (cloudAtlas {title: \"Cloud Atlas\"}) " +
            //                "RETURN cloudAtlas.released");
            //release = result.Single()[0].As<string>();
            //Debug.Log(release);

            IStatementResult result = neo4j.Transaction("MATCH (b:Bundle) RETURN b.name as name");
            List<string> bundles = result.Select(record => record["name"].As<string>()).ToList();
            foreach (var bundle in bundles)
            {
                Debug.Log(bundle);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Data);
            Debug.LogError("Neo4jObjConstructor Failed!");
        }
        
        Debug.Log(release);
    }

    /// <summary>
    /// Returns the extracted data from the Neo4J server.
    /// </summary>
    /// <returns></returns>
    public IStatementResult GetNeo4JModel ()
    {
        return neo4jModel;
    }
}
