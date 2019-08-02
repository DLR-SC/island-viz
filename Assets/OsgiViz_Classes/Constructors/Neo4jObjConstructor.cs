using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver.V1;
using System.Linq;
using System;

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
            IStatementResult result = neo4j.Transaction("MATCH (n) RETURN n");
            release = result.Single()[0].As<string>();
        } catch (Exception e)
        {
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
