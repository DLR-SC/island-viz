using Unity.Entities;

namespace IslandEntities.IslandComponents
{
    [GenerateAuthoringComponent]
    public struct RelationshipData : IComponentData
    {
        public Entity Start; // Start Node of the Relationship.
        public Entity End; // End Node of the Relationship.

        public float Width; // Width of the Relationship mesh.
    }
}