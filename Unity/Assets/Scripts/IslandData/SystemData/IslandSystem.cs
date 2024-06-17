using System.Collections.Generic;

namespace IslandData.SystemData
{
    /// <summary>
    /// Contains all island system elements.
    /// </summary>
    public class IslandSystem
    {
        public List<Island> Islands { get; }
        public List<Region> Regions { get; }
        public List<Building> Buildings { get; }
        public Dictionary<long, IIslandSystemElement> AllElements { get; }

        
        public IslandSystem (List<Island> islands, List<Region> regions, List<Building> buildings)
        {
            Islands = islands;
            Regions = regions;
            Buildings = buildings;
            AllElements = this.CombineElements(islands, regions, buildings);
        }
    }
}