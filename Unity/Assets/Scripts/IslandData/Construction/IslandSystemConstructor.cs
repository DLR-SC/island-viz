using System.Collections.Generic;
using System.Diagnostics;
using IslandData.SystemData;
using Neo4j.Driver;

namespace IslandData.Construction
{
    /// <summary>
    /// This class converts Neo4j Nodes and Relationships to an <see cref="IslandSystem"/>.
    /// </summary>
    public static class IslandSystemConstructor
    {
        /// <summary>
        /// Convert a list of Neo4j nodes and relationships to an <see cref="IslandSystem"/> based on a given ruleset.
        /// </summary>
        public static IslandSystem Construct(List<INode> nodes, List<IRelationship> relationships, ScriptableConversionRules cRules)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // All island system elements without relationships.
            var islandSystem = IslandSystemElementConstructor.CreateIslandSystemFromNodes(nodes, cRules);

            // Assign "parent" and "child" dependencies to island system elements.
            IslandSystemDependencyConstructor.AssignParentsAndChildren(islandSystem, relationships, cRules);

            // Remove all elements that can not be visualized as islands.
            IslandSystemCleaner.RemoveIncompleteElements(islandSystem);
            
            // Assign "imports"/"exports" relationships to island system elements.
            IslandSystemDependencyConstructor.AssignImportsAndExports(islandSystem, relationships, cRules);

            // Clean up
            IslandSystemCleaner.FinalizeIslandSystem(islandSystem);

            stopWatch.Stop();
            UnityEngine.Debug.Log("<b>IslandStructureConverter</b>: Conversion done in " + stopWatch.Elapsed.TotalSeconds.ToString("0.000") + " seconds.");
            UnityEngine.Debug.Log($"<b>IslandStructureConverter</b>: IslandSystem contains {islandSystem.Islands.Count} islands, {islandSystem.Regions.Count} regions and {islandSystem.Buildings.Count} buildings.");

            return islandSystem;
        }
    }
}