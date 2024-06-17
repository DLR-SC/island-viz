using IslandData.SystemData;
using IslandEvents;
using UnityEngine;

namespace IslandData.Construction
{
    /// <summary>
    /// This class starts the <see cref="IslandSystem"/> construction and returns the resulting data through the OnDataLoadingDone event.
    /// </summary>
    public class IslandSystemDataManager : MonoBehaviour
    {
        [Header("Neo4j Database Connection")]
        public ScriptableNeo4jConnection Neo4jConnection; // Set in Unity inspector.

        [Header("Island Conversion Rules")]
        public ScriptableConversionRules ConversionRules; // Set in Unity inspector.

        /// <summary>
        /// Load all data and throw an OnDataLoadingDone event.
        /// </summary>
        public async void LoadData()
        {
            if (Neo4jConnection == null || ConversionRules == null)
                Debug.LogError("DataManager: Missing at least one ScriptableObject!");
            
            if (string.IsNullOrEmpty(Neo4jConnection.URI) || string.IsNullOrEmpty(Neo4jConnection.Database))
                Debug.LogError("DataManager: Neo4j login information is incomplete!");

            // Load data from neo4j database.
            var neo4jData = await Neo4jDataLoader.Load(Neo4jConnection);
            
            // Convert Neo4j data to island system data.
            var islandData = IslandSystemConstructor.Construct(neo4jData.Nodes, neo4jData.Relationships, ConversionRules);

            // Clean up generated island system data.
            islandData.RemoveEmptyRegions();
            islandData.RemoveNonSpawnableIslands();

            // Return island system data through OnDataLoadingDone event.
            VizEvents.OnDataLoadingDone?.Invoke(islandData);
        }
    }
}