using Unity.Entities;

namespace IslandEntities.IslandComponents
{
    [GenerateAuthoringComponent]
    public struct IslandData : IComponentData
    {
        /// <summary>
        /// The unique ID of the island.
        /// </summary>
        public long IslandUID;

        /// <summary>
        /// The size of the total island entity (in meters), i.e., twice the radius to the furthest region tile.
        /// </summary>
        public float Size;
    }
}