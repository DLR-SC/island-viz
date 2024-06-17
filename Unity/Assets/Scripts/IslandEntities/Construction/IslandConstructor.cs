using System.Collections;
using System.Collections.Generic;
using IslandData.SystemData;
using IslandEntities.IslandComponents;
using IslandVis.ECS_Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This class constructs (i.e., positions & instantiates) all island entites (island parent + regions + buildings).
    /// The island parent entities contain no meshes, regions consist of multiple entities (one mesh per entity per region component), 
    /// and a building entity contains one building mesh.
    /// </summary>
    public class IslandConstructor : MonoBehaviour
    {
        // Fixed Entity placement. Only change this if you changed the table height or the Hex meshes.
        public static readonly float IslandHeight = 0f; // TODO change for dynamic table height. // TODO

        /// <summary>
        /// This contains data (entities, materials, meshes, default values, and rng) used by all constructors.
        /// </summary>
        private static ConstructorData _constructionData;

        public static Dictionary<float3,float> existingIslandPositionsAndSizes;


        #region Island_Construction

        /// <summary>
        /// Spawn all islands inside an island system. (Only islands that contain at least one region are spawned).
        /// </summary>
        public static IEnumerator SpawnAllIslands(IslandSystem islandData, ConstructorData constructionData)
        {
            _constructionData = constructionData;
            existingIslandPositionsAndSizes = new Dictionary<float3, float>();

            foreach (var island in islandData.Islands)
            {
                // Generate island position.
                float3 rootPosition = GetRandomIslandPosition();

                // Instantiate island parent entity.
                island.Entity = InstantiateIslandEntity(rootPosition, island);

                // Spawn all islands child entites(i.e., region tiles & buildings).
                yield return ConstructIslandChildren(island, rootPosition);
            }
        }

        /// <summary>
        /// Go through all island children (regions & buildings) and instantiate them at certain positions.
        /// </summary>
        /// <param name="island">The parent island data.</param>
        private static IEnumerator ConstructIslandChildren(Island island, float3 rootPosition)
        {
            var occupiedHexPositions = new List<HexPosition>();

            foreach (Region region in island.Regions)
            {
                var regionHexPositions = HexPositionHelper.FindBestPositions(occupiedHexPositions, region.Buildings.Count);

                if (regionHexPositions.Count != region.Buildings.Count)
                    Debug.LogError("IslandConstructor: Could not find enough free hex positions for region -" + region.Name + "- on the island -" + island.Name + "- !");

                float2 regionCenterPos = HexPositionHelper.GetCenterPosition(regionHexPositions);
                int regionMaterialID = _constructionData.GetRandomRegionMaterialID();
                int buildingID = 0;
                Building[] regionBuildings = region.Buildings.ToArray();

                foreach (HexPosition hexPosition in regionHexPositions)
                {
                    Building building = regionBuildings[buildingID];

                    float tileHight = math.clamp(building.Height / 50f, 0f, 1f) / 2f;
                    float3 tilePos = hexPosition.ToLocalWorldPosition();

                    //tileHight = 0.8f; // TODO

                    float3 regionCenterOffset = new float3(regionCenterPos.x - tilePos.x, tileHight, regionCenterPos.y - tilePos.z);

                    Entity regionTileEntity = InstantiateRegionTileEntity(region.UID, regionMaterialID, regionCenterOffset, tilePos, tileHight, building.UID, island.Entity);
                    region.Entities.Add(regionTileEntity);
                    //building.Entity = regionTileEntity;

                    // Making one region tile entity to the main region entity makes relationship creation a lot easier :)
                    if (region.Entity == null || region.Entity == Entity.Null)
                        region.Entity = regionTileEntity;

                    // Building
                    
                    //int heightLevel = 0; // MapLocToHeightLevel(building.Height);
                    //Entity buildingEntity = InstantiateBuildingEntity(heightLevel, new float3(0, 0.1f, 0), building.Type, regionTileEntity); //  GetRandomBuildingColor()
                    Entity buildingEntity = InstantiateEmptyBuildingEntity(new float3(0, 0.1f, 0), building.Type, regionTileEntity);
                    building.Entity = buildingEntity;

                    buildingID++;

                    // Border

                    IslandConstructorBorderExtension.CreateRegionTileBorders(_constructionData, hexPosition, regionMaterialID, regionTileEntity, regionHexPositions);
                }

                occupiedHexPositions.AddRange(regionHexPositions);
            }
            yield return null;

            // After all region entites are placed, we can fill in the missing island stats.

            float2 islandCenterPosition = HexPositionHelper.GetCenterPosition(occupiedHexPositions);
            float islandSize = HexPositionHelper.GetFurthestRegionTileDistanceFromCenter(islandCenterPosition, occupiedHexPositions) * 2f + 1;
            SetEntityIslandData(island, new float3(islandCenterPosition.x, 2f, islandCenterPosition.y), islandSize);
            existingIslandPositionsAndSizes.Add(rootPosition, islandSize);
        }

        #endregion

        #region Entity_Instantiation

        /// <summary>
        /// Instantiate a new island entity.
        /// </summary>
        /// <param name="worldPosition">World position of the island entity.</param>
        /// <param name="island">The island system data.</param>
        /// <returns>Returns the instantiated entity.</returns>
        private static Entity InstantiateIslandEntity(float3 worldPosition, Island island)
        {
            var entityManager = _constructionData.EntityManager;

            Entity entity = entityManager.CreateEntity(
             typeof(LocalToWorld),
             typeof(Translation),
             typeof(IslandComponents.IslandData),
             typeof(VelocityData)
            );

#if UNITY_EDITOR
            entityManager.SetName(entity, "Island - " + island.Name);
#endif
            entityManager.SetComponentData(entity, new Translation
            {
                Value = worldPosition
            });

            return entity;
        }

        private static void SetEntityIslandData(Island island, float3 islandCenterPosition, float islandSize)
        {
            var entityManager = _constructionData.EntityManager;

            entityManager.SetComponentData(island.Entity, new IslandComponents.IslandData
            {
                IslandUID = island.UID,
                Size = islandSize
            });

            entityManager.AddComponentData(island.Entity, new CenterOffsetData
            {
                Value = islandCenterPosition
            });
        }

        /// <summary>
        /// Instantiate a new region tile entity.
        /// </summary>
        /// <param name="localTilePos">Local position (relative ot the parent island entity) of the region entity.</param>
        /// <param name="tileHeight">The hight of the region tile.</param>
        /// <param name="regionMaterialID">Material id (i.e., color) of the region.</param>
        /// <param name="islandEntity">Entity of the parent island.</param>
        /// <returns>Returns the instantiated entity.</returns>
        private static Entity InstantiateRegionTileEntity(long regionUID, int regionMaterialID, float3 regionCenterOffset, float3 localTilePos, float tileHeight, long buildingUID, Entity islandEntity)
        {
            var entityManager = _constructionData.EntityManager;
            Entity entity = entityManager.Instantiate(_constructionData.EntityHexPrefab);

            // Set Position.
            entityManager.SetComponentData(entity, new Translation
            {
                Value = localTilePos + new float3(0,tileHeight,0)
            });

            // Set Mesh.
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _constructionData.RegionMesh,
                material = _constructionData.GetRegionMaterial(regionMaterialID),
                layerMask = 1,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true
            });

            // Add region tile data.
            entityManager.AddComponentData(entity, new RegionTileData
            {
                RegionUID = regionUID,
                BuildingUID = buildingUID,
                RegionColor = regionMaterialID
            });

            entityManager.AddComponentData(entity, new CenterOffsetData
            {
                Value = regionCenterOffset
            });

            // Add parent island.
            entityManager.AddComponentData(entity, new Parent
            {
                Value = islandEntity
            });

            entityManager.AddComponent<LocalToParent>(entity);
            entityManager.AddComponent<LocalToWorld>(entity);

            return entity;
        }


        private static Entity InstantiateEmptyBuildingEntity (float3 localPosition, int materialID, Entity parentEntity)
        {
            var entityManager = _constructionData.EntityManager;

            Entity entity = entityManager.CreateEntity(
             typeof(LocalToWorld),
             typeof(LocalToParent),
             typeof(Translation),
             typeof(BuildingData)
            );

            entityManager.SetComponentData(entity, new Translation
            {
                Value = localPosition
            });

            entityManager.SetComponentData(entity, new BuildingData
            {
                Color = materialID
            });

            entityManager.AddComponentData(entity, new CenterOffsetData
            {
                Value = float3.zero
            });

            // Add parent island.
            entityManager.AddComponentData(entity, new Parent
            {
                Value = parentEntity
            });

            return entity;
        }

        /// <summary>
        /// Instantiate a new building entity.
        /// </summary>
        /// <param name="heightlevel">The index of building prefab that will be used, i.e., the height of the building.</param> // TODO
        /// <param name="localPosition">Local position of the building entity.</param>
        /// <param name="materialID">Material id (i.e., color) of the building.</param>
        /// <param name="parentEntity">Entity of the parent region.</param>
        /// <returns>Returns the instantiated entity.</returns>
        private static Entity InstantiateBuildingEntity(int heightlevel, float3 localPosition, int materialID, Entity parentEntity)
        {
            EntityManager entityManager = _constructionData.EntityManager;
            Entity entity = entityManager.Instantiate(_constructionData.EntityBuildingPrefabs[heightlevel]);
            // Set data
            entityManager.SetComponentData(entity, new Translation
            {
                Value = localPosition
            });
            entityManager.SetComponentData(entity, new Rotation
            {
                Value = quaternion.Euler(0, _constructionData.RNG.Next(0, 360), 0, math.RotationOrder.XYZ) // Random rotation.
            });
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _constructionData.BuildingMeshes[heightlevel],
                material = _constructionData.GetBuildingMaterial(materialID),
                layerMask = 1,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows = false
            }); ;
            // Add data
            entityManager.AddComponentData(entity, new BuildingData
            {
                Color = materialID
            });
            entityManager.AddComponentData(entity, new CenterOffsetData
            {
                Value = float3.zero
            });
            entityManager.AddComponentData(entity, new HighlightData
            {
                Value = false
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

        

        #endregion

        #region Helper_Functions

        /// <summary>
        /// Returns a random position [-maxDistance/2, maxDistance/2].
        /// </summary>
        private static float3 RandomWorldPosition(float diameter, int counter = 0)
        {            
            var pos = new float3(((float)_constructionData.RNG.NextDouble() - 0.5f) * diameter, 0f, ((float)_constructionData.RNG.NextDouble() - 0.5f) * diameter);

            if (counter < 50 && IslandsNearThisPosition(pos))
            {
                counter ++;
                return RandomWorldPosition(diameter, counter);
            }
            else 
            {
                return pos;
            }
        }        
                
        private static float3 GetRandomIslandPosition()
        {
            return RandomWorldPosition(_constructionData.WorldSize/2) + new float3(0, IslandHeight, 0);
        }

        private static bool IslandsNearThisPosition(float3 pos)
        {
            foreach (var existingPos in existingIslandPositionsAndSizes)
            {
                if (math.distance(existingPos.Key, pos) <= existingPos.Value)
                    return true;
            }

            return false;
        }

        private static int GetRandomBuildingColor ()
        {
            float rng = (float)_constructionData.RNG.NextDouble();
            if (rng < 0.6f)
                return 0;
            else if (rng < 0.8f)
                return 1;
            else
                return 2;



        }

        #endregion
    }
}
