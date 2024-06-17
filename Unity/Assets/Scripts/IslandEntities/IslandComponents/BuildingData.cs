using Unity.Entities;
using Unity.Mathematics;

namespace IslandEntities.IslandComponents
{
    [GenerateAuthoringComponent]
    public struct BuildingData : IComponentData
    {
        public int Color;
        public float2 CenterOffset;
    }
}