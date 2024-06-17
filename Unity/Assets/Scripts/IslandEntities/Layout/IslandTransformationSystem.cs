using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace IslandVis.ECS_Systems
{
    /// <summary>
    /// This system applies different transformations (translation, scale) to all entites with an <c>IslandTag</c> component.
    /// </summary>
    public partial class IslandTransformationSystem : SystemBase
    {
        /// <summary>
        /// Delta position that will be applied on the next Update.
        /// </summary>
        private static float3 _islandPositionDelta;

        /// <summary>
        /// Modify the islands translation on the next Update by <paramref name="deltaPosition"/>. 
        /// </summary>
        public static void Translate (float3 deltaPosition)
        {
            _islandPositionDelta += deltaPosition;
        }

        /// <summary>
        /// Called by Unity every frame.
        /// </summary>
        protected override void OnUpdate()
        {   
            if (!(_islandPositionDelta.x == 0 && _islandPositionDelta.z == 0))
            {
                float3 delta = _islandPositionDelta;
                _islandPositionDelta = float3.zero;

                Entities.
                    WithAll<IslandEntities.IslandComponents.IslandData>().
                    ForEach((ref Translation pos) =>
                    {
                        pos.Value.x += delta.x; 
                        pos.Value.z += delta.z;

                    }).ScheduleParallel();
            }
        }
    }
}
