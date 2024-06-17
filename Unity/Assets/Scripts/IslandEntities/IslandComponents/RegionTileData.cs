using Unity.Entities;

namespace IslandEntities.IslandComponents
{
    [GenerateAuthoringComponent]
    public struct RegionTileData : IComponentData
    {
        /// <summary>
        /// The UID of the parent region.
        /// </summary>
        public long RegionUID;

        /// <summary>
        /// The UID of the building that is located on this tile.
        /// </summary>
        public long BuildingUID;

        /// <summary>
        /// The color ID of the parent region (needed, e.g., for temporary color changes).
        /// </summary>
        public int RegionColor;
    }
}