using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IslandData.SystemData;
using Neo4j.Driver;

namespace IslandData.Construction
{
    public static class IslandSystemElementConstructor
    {
        /// <summary>
        /// Create all island system elements from Neo4j nodes and store them in a new <see cref="IslandSystem"/>. 
        /// </summary>
        public static IslandSystem CreateIslandSystemFromNodes (IEnumerable<INode> nodes, ScriptableConversionRules cRules)
        {
            var islands = new ConcurrentBag<Island>();
            var regions = new ConcurrentBag<Region>();
            var buildings = new ConcurrentBag<Building>();

            Parallel.ForEach(nodes, node =>
            {
                var nodeLabel = node.Labels[0]; // We use the first label for classification.

                // Island
                if (cRules.IsIsland(nodeLabel))
                {
                    var island = new Island(node.Properties, node.Labels, node.Id, cRules);
                    islands.Add(island);
                }
                // Region
                else if (cRules.IsRegion(nodeLabel))
                {
                    var region = new Region(node.Properties, node.Labels, node.Id, cRules);
                    regions.Add(region);
                }
                // Building
                else if (cRules.IsBuilding(nodeLabel, out int buildingType))
                {
                    var building = new Building(node.Properties, node.Labels, node.Id, cRules)
                    {
                        Type = buildingType
                    };
                    buildings.Add(building);
                }
            });

            return new IslandSystem(islands.ToList(), regions.ToList(), buildings.ToList());
        }
        
        /// <summary>
        /// Whether this node label is an island by the rules.
        /// </summary>
        private static bool IsIsland(this ScriptableConversionRules rules, string nodeLabel)
        {
            return rules.IslandNodeLabels.Contains(nodeLabel);
        }

        /// <summary>
        /// Whether this node label is an region by the rules.
        /// </summary>
        private static bool IsRegion(this ScriptableConversionRules rules, string nodeLabel)
        {
            return rules.RegionNodeLabels.Contains(nodeLabel);
        }

        /// <summary>
        /// Whether this node label is an building by the rules.
        /// </summary>
        public static bool IsBuilding(this ScriptableConversionRules rules, string nodeLabel, out int BuildingType)
        {
            // No building.
            if (!rules.BuildingNodeLabels.Contains(nodeLabel))
            {
                BuildingType = 0;
                return false;
            }

            // Only one building label exists -> only one bilding type is possible.
            if (rules.BuildingNodeLabels.Length == 1)
            {
                BuildingType = 0;
                return true;
            }

            // Multiple building labels exits -> find the building type.
            for (int i = 0; i < rules.BuildingNodeLabels.Length; i++)
            {
                if (nodeLabel == rules.BuildingNodeLabels[i])
                {
                    BuildingType = i;

                    if (i > 0)
                        UnityEngine.Debug.Log("AAAAAA");

                    return true;
                }
            }

            // We should not arrive here :)
            UnityEngine.Debug.LogError("Something went wrong!");
            BuildingType = 0;
            return false;
        }
    }
}