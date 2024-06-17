using Unity.Entities;
using Unity.Mathematics;

namespace IslandVis.ECS_Components
{
    [InternalBufferCapacity(512)]
    public struct ConnectionBufferElement : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator float3(ConnectionBufferElement e) { return e.Value; }
        public static implicit operator ConnectionBufferElement(float3 e) { return new ConnectionBufferElement { Value = e }; }

        // Actual value each buffer element will store.
        public float3 Value;
    }
}