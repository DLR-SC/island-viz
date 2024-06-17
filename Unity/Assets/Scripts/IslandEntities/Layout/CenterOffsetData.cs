using Unity.Entities;
using Unity.Mathematics;

namespace IslandVis.ECS_Components
{
    [GenerateAuthoringComponent]
    public struct CenterOffsetData : IComponentData
    {
        /// <summary>
        /// The offset from this entity to the center of its island system element (in meters).
        /// </summary>
        public float3 Value;
    }
}