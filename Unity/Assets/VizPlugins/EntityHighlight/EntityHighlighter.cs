using Unity.Entities;
using UnityEngine;
using Unity.Rendering;
using System.Collections;
using IslandData.SystemData;
using IslandPlugins;

namespace EntityHighlighter
{
    /// <summary>
    /// This plugin reacts to the OnSelectOrHighlight event thrown by the EntityInteractionManger.
    /// On highlight, the EntityHighlighter applies a highlight material to the highlighted entity.
    /// On dehighlight, the highlight material is removed.
    /// </summary>
    public class EntityHighlighter : IslandPluginBase
    {
        private EntityManager entityManager; // Controller for the Unity ECS.

        public Material HighlightMaterial;

        /// <summary>
        /// This is called after the Plugin was initiated.
        /// </summary>
        public override IEnumerator Init()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //OSGiInteraction.OnHighlight += HighlightOrDehighlightEntity;
            //EntityRaycaster.OnRaycastHitStart += OnRayHightlight;
            //EntityRaycaster.OnRaycastHitEnd += OnRayDehightlight;

            yield return null;
        }


        public void OnRayHightlight (Entity entity, Transform hand)
        {
            HighlightEntity(entity);
        }
        public void OnRayDehightlight(Entity entity, Transform hand)
        {
            DehighlightEntity(entity);
        }


        /// <summary>
        /// Process the OnSelectOrHighlight event thrown by the EntityInteractionManger.
        /// </summary>
        public void HighlightOrDehighlightEntity(IslandSystemElement oSGiElement, bool selected)
        {
            if (selected)
            {
                HighlightEntity(oSGiElement.Entity);
            }
            else
            {
                DehighlightEntity(oSGiElement.Entity);
            }
        }


        /// <summary>
        /// Changes the material to a highlight material.
        /// </summary>
        /// <param name="entity">Entity, that is highlighted.</param>
        public void HighlightEntity(Entity entity)
        {
            if (entityManager.HasComponent<RenderMesh>(entity)) // TODO Islands?
            {
                RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(entity);
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = renderMesh.mesh,
                    material = HighlightMaterial
                });
            }
        }

        /// <summary>
        /// Changes the material back to the hex material.
        /// </summary>
        /// <param name="entity">Entity, that is dehighlighted.</param>
        public void DehighlightEntity(Entity entity)
        {
            //if (entityManager.HasComponent<RenderMesh>(entity)) // TODO Islands?
            //{
            //    RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(entity);

            //    UnityEngine.Material mat;
            //    if (entityManager.HasComponent<RegionData>(entity))
            //    {
            //        RegionData regionData = entityManager.GetComponentData<RegionData>(entity);
            //        mat = ConstructionData.Instance.GetRegionMaterial(regionData.Color);
            //    }
            //    else if (entityManager.HasComponent<BuildingData>(entity))
            //    {
            //        BuildingData buildingData = entityManager.GetComponentData<BuildingData>(entity);
            //        mat = ConstructionData.Instance.GetBuildingMaterial(buildingData.Color);
            //    }
            //    else
            //    {
            //        mat = null;
            //    }

            //    entityManager.SetSharedComponentData(entity, new RenderMesh
            //    {
            //        mesh = renderMesh.mesh,
            //        material = mat
            //    });
            //}
        }
    }
}