using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IslandData.SystemData;
using Neo4j.Driver;

namespace IslandData.Construction
{
    /// <summary>
    /// This class resolves Neo4j relationships to "parent", "child", "import" and "export" dependencies. 
    /// </summary>
    public static class IslandSystemDependencyConstructor
    {
        /// <summary>
        /// Resolve the CONTAINS relationships and store them as parents nad children.
        /// </summary>
        public static void AssignParentsAndChildren (IslandSystem islandSystem, IEnumerable<IRelationship> relationships, ScriptableConversionRules cRules)
        {
            Parallel.ForEach(relationships, relationship =>
            {
                var rType = relationship.Type;

                // Island-CONTAINS->Region
                if (cRules.IslandContainsRegionRelationshipTypes.Contains(rType))
                {
                    var startExists = islandSystem.AllElements.TryGetValue(relationship.StartNodeId, out var startElement);
                    var endExists = islandSystem.AllElements.TryGetValue(relationship.EndNodeId, out var endElement);

                    if (startExists && endExists && startElement is Island island && endElement is Region region)
                    {
                        island.Regions.Add(region);
                        region.Parent = island;
                    }
                }
                // Region-CONTAINS->Building
                if (cRules.RegionContainsBuildingRelationshipTypes.Contains(rType))
                {
                    var startExists = islandSystem.AllElements.TryGetValue(relationship.StartNodeId, out var startElement);
                    var endExists = islandSystem.AllElements.TryGetValue(relationship.EndNodeId, out var endElement);

                    if (startExists && endExists && startElement is Region region && endElement is Building building)
                    {
                        region.Buildings.Add(building);
                        building.Parent = region;
                    }
                }
            });
        }

        /// <summary>
        /// Resolve the IMPORTS relationships and store them as imports and exports on different levels.
        /// (For example, a Region-IMPORTS->Region relationships also get stored on the Island-IMPORTS->Region level etc.)
        /// </summary>
        public static void AssignImportsAndExports (IslandSystem islandSystem, List<IRelationship> relationships, ScriptableConversionRules cRules)
        {
            Parallel.ForEach(relationships, relationship =>
            {
                var rType = relationship.Type;

                // Node-IMPORTS/EXPORTS->Node
                if (cRules.ImportRelationshipTypes.Contains(rType))
                {
                    var startExists = islandSystem.AllElements.TryGetValue(relationship.StartNodeId, out var start);
                    var endExists = islandSystem.AllElements.TryGetValue(relationship.EndNodeId, out var end);

                    if (startExists && endExists && start != null && end != null)
                    {
                        start.AddConcurrentImport(end, rType);
                        BruteForcePropagation(start, end, rType);
                    }
                }
            });
        }

        /// <summary>
        /// Connect all parent elements of the start element with all parent elements of the end element.
        /// Note: This is a brute-force approach, as some of these relationships might already exists, but duplicates
        /// will be filtered out when finalizing the IslandSystem.
        /// </summary>
        private static void BruteForcePropagation(IIslandSystemElement start, IIslandSystemElement end, string rType)
        {   
            if (start is Building startBuilding)
            {
                startBuilding.Parent.AddConcurrentImport(end, rType);
                startBuilding.Parent.Parent.AddConcurrentImport(end, rType);

                if (end is Building endBuilding)
                {
                    startBuilding.AddConcurrentImport(endBuilding.Parent, rType);
                    startBuilding.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                    startBuilding.Parent.AddConcurrentImport(endBuilding.Parent, rType);
                    startBuilding.Parent.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                    startBuilding.Parent.Parent.AddConcurrentImport(endBuilding.Parent, rType);
                    startBuilding.Parent.Parent.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                }
                else if (end is Region endRegion)
                {
                    startBuilding.AddConcurrentImport(endRegion.Parent, rType);
                    startBuilding.Parent.AddConcurrentImport(endRegion.Parent, rType);
                    startBuilding.Parent.Parent.AddConcurrentImport(endRegion.Parent, rType);
                }
            }
            else if (start is Region startRegion)
            {
                startRegion.Parent.AddConcurrentImport(end, rType);
                
                if (end is Building endBuilding)
                {
                    startRegion.AddConcurrentImport(endBuilding.Parent, rType);
                    startRegion.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                    startRegion.Parent.AddConcurrentImport(endBuilding.Parent, rType);
                    startRegion.Parent.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                }
                else if (end is Region endRegion)
                {
                    startRegion.AddConcurrentImport(endRegion.Parent, rType);
                    startRegion.Parent.AddConcurrentImport(endRegion.Parent, rType);
                }
            }
            else if (start is Island startIsland)
            {
                if (end is Building endBuilding)
                {
                    startIsland.AddConcurrentImport(endBuilding.Parent, rType);
                    startIsland.AddConcurrentImport(endBuilding.Parent.Parent, rType);
                }
                else if (end is Region endRegion)
                {
                    startIsland.AddConcurrentImport(endRegion.Parent, rType);
                }
            }
        }
    }
}