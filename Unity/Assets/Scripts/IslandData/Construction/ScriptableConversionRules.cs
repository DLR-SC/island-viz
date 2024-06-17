using UnityEngine;

namespace IslandData.Construction
{
    /// <summary>
    /// This class contains string labels that are used to convert the neo4j nodes and relationships to an <see cref="IslandSystem"/>.  
    /// </summary>
    [CreateAssetMenu(fileName = "ConversionRules", menuName = "ScriptableObjects/Neo4j/ConversionRules", order = 1)]
    public class ScriptableConversionRules : ScriptableObject
    {
        [Header("Node Labels")]

        public string[] IslandNodeLabels; // The Neo4j node labels for islands.

        public string[] RegionNodeLabels; // The Neo4j node labels for island regions.

        public string[] BuildingNodeLabels; // The Neo4j node label for island tiles and buildings.

        [Header("Property Names")]

        public string IslandNodeNameProperty; // The Neo4j node property that is the island name (string).

        public string RegionNodeNameProperty; // The Neo4j node property that is the region name (string).

        public string BuildingNodeNameProperty; // The Neo4j node property that is the building name (string).

        public string RegionNodeHeightProperty; // The Neo4j node property that is the region height (int).

        public string BuildingNodeHeightProperty; // The Neo4j node property that is the region height (int).

        [Header("Relationship Types")]

        public string[] IslandContainsRegionRelationshipTypes; // The Neo4j relationship label for island-CONTAINS->regions relationships.

        public string[] RegionContainsBuildingRelationshipTypes; // The Neo4j relationship label for region-CONTAINS->buildings relationships.

        public string[] ImportRelationshipTypes; // The Neo4j relationship label for region-CONTAINS->buildings relationships.
    }
}