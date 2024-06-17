using UnityEngine;

namespace IslandData.Construction
{
    /// <summary>
    /// This class contains the required information to connect to a Neo4j database. 
    /// </summary>
    [CreateAssetMenu(fileName = "Neo4jConnection", menuName = "ScriptableObjects/Neo4j/Neo4jConnection", order = 1)]
    public class ScriptableNeo4jConnection : ScriptableObject
    {
        public string URI; // URI of the Neo4J server. (Default: "neo4j://localhost:7687")
        public string Database; // Name of the database on the Neo4J server.
        public string Username; // User name used for authentication of the Neo4J server.
        public string Password; // Password used for authentification of the Neo4J server.
    }    
}
