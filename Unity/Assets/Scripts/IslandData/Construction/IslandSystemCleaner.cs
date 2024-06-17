using System.Threading.Tasks;
using IslandData.SystemData;

namespace IslandData.Construction
{
    public static class IslandSystemCleaner
    {
        /// <summary>
        /// Remove all generated island system elements that can not be used to generate islands.
        /// </summary>
        public static void RemoveIncompleteElements(IslandSystem islandSystem)
        {
            // Remove islands without regions, i.e., namespaces without classes.
            for (int i = islandSystem.Islands.Count - 1; i >= 0; i--)
            {
                if (islandSystem.Islands[i].Regions.Count == 0)
                {
                    islandSystem.AllElements.Remove(islandSystem.Islands[i].UID);
                    islandSystem.Islands.Remove(islandSystem.Islands[i]);
                }
            }
            
            // Remove regions without parents or regions without buildings.
            for (int i = islandSystem.Regions.Count - 1; i >= 0; i--)
            {
                if (islandSystem.Regions[i].Parent == null || islandSystem.Regions[i].Buildings.Count == 0)
                {
                    islandSystem.AllElements.Remove(islandSystem.Regions[i].UID);
                    islandSystem.Regions.Remove(islandSystem.Regions[i]);
                }
            }
            
            // Remove buildings without regions or buildings with island-missing regions.  
            for (int i = islandSystem.Buildings.Count - 1; i >= 0; i--)
            {
                if (islandSystem.Buildings[i].Parent == null || islandSystem.Buildings[i].Parent.Parent == null)
                {
                    islandSystem.AllElements.Remove(islandSystem.Buildings[i].UID);
                    islandSystem.Buildings.Remove(islandSystem.Buildings[i]);
                }
            }
        }
        
        /// <summary>
        /// Convert all import/export ConcurrentBags to more usable lists. 
        /// </summary>
        public static void FinalizeIslandSystem (IslandSystem islandSystem)
        {
            Parallel.ForEach(islandSystem.Islands, item =>
            {
                item.Imports.AddRange(item.GetCleanImportList());
                item.Exports.AddRange(item.GetCleanExportList());
            });

            Parallel.ForEach(islandSystem.Regions, item =>
            {
                item.Imports.AddRange(item.GetCleanImportList());
                item.Exports.AddRange(item.GetCleanExportList());
            });

            Parallel.ForEach(islandSystem.Buildings, item =>
            {
                item.Imports.AddRange(item.GetCleanImportList());
                item.Exports.AddRange(item.GetCleanExportList());
            });
        }
    }
}