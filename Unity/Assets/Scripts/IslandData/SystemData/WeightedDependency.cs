
namespace IslandData.SystemData
{
    public class WeightedDependency
    {
        public IIslandSystemElement Target { get; set; }
        public int Weight { get; set; }
        public string Label { get; set; }
    }
}