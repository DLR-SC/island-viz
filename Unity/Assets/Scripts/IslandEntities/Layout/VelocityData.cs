using Unity.Entities;
using Unity.Mathematics;

namespace IslandVis.ECS_Components
{
    [GenerateAuthoringComponent]
    public struct VelocityData : IComponentData
    {
        public float3 Value;
    }

}

