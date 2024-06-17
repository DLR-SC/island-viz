using IslandEntities.IslandComponents;
using IslandVis.ECS_Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace IslandVis.ECS_Systems
{
    /// <summary>
    /// This system updates all relationship transforms every frame so they "connect" their two node entites.
    /// </summary>
    [UpdateAfter(typeof(IslandVis.ECS_Systems.IslandTransformationSystem))]
    public partial class RelationshipTransformUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            ComponentDataFromEntity<CenterOffsetData> offsetGroup = GetComponentDataFromEntity<CenterOffsetData>(true);
            ComponentDataFromEntity<LocalToWorld> worldMatrixGroup = GetComponentDataFromEntity<LocalToWorld>(true);

            Entities.
                WithAll<RelationshipTag>().
                ForEach((ref Translation pos, ref Rotation rot, ref NonUniformScale scale, ref RelationshipData data) =>
                {
                    float3 startPos = worldMatrixGroup[data.Start].Position + offsetGroup[data.Start].Value;
                    float3 endPos = worldMatrixGroup[data.End].Position + offsetGroup[data.End].Value;

                    float3 forward = endPos - startPos;
                    float distance = math.length(forward);
                    float3 forwardUnit = forward / distance;

                    rot.Value = quaternion.LookRotation(forwardUnit, new float3(0,1,0)); // math.cross(end.Value, start.Value)
                    scale.Value = new float3(0.05f * data.Width, data.Width + math.max(distance / 2f, 5f), distance);
                    pos.Value = (startPos + endPos) / 2f;

                }).Run(); // Very low impact --> no multithreading required!
        }
    }
}