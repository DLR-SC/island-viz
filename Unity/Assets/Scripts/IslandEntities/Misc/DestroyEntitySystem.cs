using IslandVis.ECS_Components;
using Unity.Entities;

namespace IslandVis.ECS_Systems
{
    public partial class DestroyEntitySystem : SystemBase
    {

        private EntityQuery query;
        private EntityManager em;

        protected override void OnCreate()
        {
            base.OnCreate();

            query = GetEntityQuery(ComponentType.ReadOnly<DestroyTag>());
            em = EntityManager;
        }

        protected override void OnUpdate()
        {
            em.DestroyEntity(query);
        }
    }
}
