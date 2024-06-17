using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace IslandInterface.Selection
{
    /// <summary>
    /// This class contains functionality for entity raycasts.
    /// </summary>
    public static class EntityRaycast
    {
        /// <summary>
        /// Casts a ray and return true if a entity with a physics collider is hit.
        /// </summary>
        /// <param name="fromPosition">Start position of the ray.</param>
        /// <param name="toPosition">Forward vector of the ray.</param>
        /// <param name="hit">Entity that was hit.</param>
        public static bool Raycast(float3 fromPosition, float3 toPosition, out Entity hit, out float3 hitPos)
        {
            var buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            var raycastInput = new RaycastInput
            {
                Start = fromPosition,
                End = toPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };

            if (collisionWorld.CastRay(raycastInput, out var raycastHit))
            {
                hit = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                hitPos = raycastHit.Position;
                return true;
            }
            else
            {
                hit = Entity.Null;
                hitPos = float3.zero;
                return false;
            }
        }
    }
}
