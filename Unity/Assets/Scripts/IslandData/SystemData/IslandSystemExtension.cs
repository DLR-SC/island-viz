using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace IslandData.SystemData
{
    public static class IslandSystemExtension
    {
        public static Dictionary<long, IIslandSystemElement> CombineElements(this IslandSystem islandSystem, List<Island> islands, List<Region> regions, List<Building> buildings)
        {
            var result = new Dictionary<long, IIslandSystemElement>();
            foreach (var island in islands)
            {
                result.Add(island.UID, island);
            }
            foreach (var region in regions)
            {
                result.Add(region.UID, region);
            }
            foreach (var building in buildings)
            {
                result.Add(building.UID, building);
            }
            return result;
        }
        
        /// <summary>
        /// Find an existing island by name.
        /// </summary>
        public static Island GetIslandByName (this IslandSystem islandSystem, string islandName)
        {
            return islandSystem.Islands.ToList().Find(x => x.Name == islandName);
        }

        /// <summary>
        /// Go through all buildings and find the minimum and maximum building height.
        /// </summary>
        /// <returns>[ Min height | Max height ]</returns>
        public static (int min, int max) GetBuildingHeightRange (this IslandSystem islandSystem)
        {
            ConcurrentBag<int> heights = new ConcurrentBag<int>();

            Parallel.ForEach(islandSystem.Buildings, building =>
            {
                heights.Add(building.Height);
            });

            return ( heights.ToList().Min(), heights.ToList().Max() );
        }

        public static void RemoveNonSpawnableIslands (this IslandSystem islandSystem)
        {
            for (int i = islandSystem.Islands.Count - 1; i >= 0; i--)
            {
                if (!islandSystem.Islands[i].IsSpawnable())
                {
                    Debug.LogWarning("Removing unspawnable island " + islandSystem.Islands[i].Name);
                    islandSystem.Islands.RemoveAt(i);
                }
            }
        }

        public static void RemoveEmptyRegions (this IslandSystem islandSystem)
        {
            for (int i = islandSystem.Regions.Count - 1; i >= 0; i--)
            {
                if (islandSystem.Regions[i].Buildings.Count == 0)
                    islandSystem.Regions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Check if it makes sense to spawn this island.
        /// </summary>
        public static bool IsSpawnable (this Island island)
        {
            // Island is empty -> island is not spawnable.
            if (island == null || island.Regions.Count == 0)
                return false;

            // Try find a region with buildings -> island is spawnable.
            foreach (Region region in island.Regions)
            {
                if (region != null && region.Buildings.Count > 0)
                    return true;
            }

            // We did not find a region with building -> island is not spawnable.
            return false;
        }

        public static Island GetParentIsland (this IIslandSystemElement dataElement)
        {
            if (dataElement is Island island)
            {
                return island;
            }
            else if (dataElement is Region region)
            {
                return region.Parent;
            }
            else if (dataElement is Building building)
            {
                return building.Parent.Parent;
            }
            else
            {
                Debug.LogWarning("Did not find the parent island of the IslandDataElement!");
                return null;
            }
        }

        public static Entity[] GetAllIslandEntities (this IslandSystem islandSystem)
        {
            List<Entity> islandEntities = new List<Entity>();

            foreach (Island island in islandSystem.Islands)
            {
                if (island.Entity == Entity.Null)
                    continue;

                islandEntities.Add(island.Entity);
            }

            return islandEntities.ToArray();
        }
    }

}
