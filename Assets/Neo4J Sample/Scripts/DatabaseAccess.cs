using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4JDriver;

/// <summary>
/// only holds neo4j object as accesspoint to database
/// </summary>
public class DatabaseAccess : MonoBehaviour
{

    public string databaseAdress = "bolt://localhost:7687";
    public string databaseUserName = "neo4j";
    public string databasePasswort = "asdf";

    // Start is called before the first frame update
    private Neo4J dataBase;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        dataBase = new Neo4J(databaseAdress, databaseUserName, databasePasswort);
    }

    public Neo4J GetDatabase()
    {
        return dataBase;
    }

    /// <summary>
    /// Disposes neo4j database access and destroys this gameobject
    /// </summary>
    public void DisposeDatabase()
    {
        dataBase.Dispose();
        Destroy(gameObject);
    }
}
