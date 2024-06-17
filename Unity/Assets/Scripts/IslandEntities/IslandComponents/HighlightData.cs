using Unity.Entities;

namespace IslandEntities.IslandComponents
{
    [GenerateAuthoringComponent]
    public struct HighlightData : IComponentData
    {
        public bool Value;
    }
}