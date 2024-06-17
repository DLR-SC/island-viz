using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace IslandEntities.Construction
{
    public static class IslandConstructorBorderExtension
    {
        /// <summary>
        /// Check the neigbors of the region tile and add region border entites.
        /// Note: The default (without rotation) region border mesh is at hex direction SW. Hence, beginning from SW we rotate the border mesh by 60� for each direction step.
        /// </summary>
        /// <param name="constructionData">Data containing prefabs, materials, ...</param>
        /// <param name="tilePosition">The region tile position.</param>
        /// <param name="materialID">The color of the region border.</param>
        /// <param name="tileEntity">The parent entity of the border entites.</param>
        /// <param name="regionPositions">All hex positions of the region.</param>
        public static void CreateRegionTileBorders (ConstructorData constructionData, HexPosition tilePosition, int materialID, Entity tileEntity, List<HexPosition> regionPositions)
        {
            float rotationCounter = 0f; // Number of rotation steps.
            float rotationValue = 2f * math.PI * 1f / 6f; // RAD rotation for one rotation step.

            bool even = tilePosition.x % 2 == 0;

            // Check SW
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.SW] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.SW])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
            rotationCounter++;

            // Check NW
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.NW] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.NW])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
            rotationCounter++;

            // Check N
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.N] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.N])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
            rotationCounter++;

            // Check NE
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.NE] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.NE])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
            rotationCounter++;

            // Check SE
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.SE] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.SE])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
            rotationCounter++;

            // Check S
            if (!regionPositions.Contains(tilePosition + (even ? HexPosition.NeighborsEven[(int)HexPosition.HexDirection.S] : HexPosition.NeighborsOdd[(int)HexPosition.HexDirection.S])))
            {
                InstantiateRegionEntity(materialID, rotationCounter * rotationValue, tileEntity, constructionData);
            }
        }


        /// <summary>
        /// Instantiate a region border entity.
        /// </summary>
        /// <param name="materialID">The color of the entity.</param>
        /// <param name="rotation">The entity rotation in RAD.</param>
        /// <param name="parentEntity">The parent entity.</param>
        /// <param name="constructionData">Data containing materials and meshes.</param>
        /// <returns>The created entity.</returns>
        private static Entity InstantiateRegionEntity(int materialID, float rotation, Entity parentEntity, ConstructorData constructionData)
        {
            var entityManager = constructionData.EntityManager;
            var entity = entityManager.Instantiate(constructionData.EntityRegionBorderPrefab);
            // Set data
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(0,0.01f,0)
            });
            entityManager.SetComponentData(entity, new Rotation
            {
                Value = quaternion.Euler(0, rotation, 0, math.RotationOrder.XYZ) // Random rotation.
            });
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = constructionData.RegionBorderMesh,
                material = constructionData.GetBorderMaterial(materialID),
                layerMask = 1,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows = false
            });
            entityManager.AddComponentData(entity, new Parent
            {
                Value = parentEntity
            });
            entityManager.AddComponent<LocalToParent>(entity);
            entityManager.AddComponent<LocalToWorld>(entity);
            //entityManager.AddComponentData(entity, new NodeData
            //{
            //    Weight = 1f
            //});

            return entity;
        }

    }

}
