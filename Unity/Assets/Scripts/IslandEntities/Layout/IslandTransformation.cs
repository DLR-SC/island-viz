using System.Collections;
using IslandData.SystemData;
using IslandLevelOfDetail;
using IslandSettings;
using IslandVis.ECS_Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using IslandTransformationSystem = IslandVis.ECS_Systems.IslandTransformationSystem;

namespace IslandViz.Core
{
    /// <summary>
    /// This class contains different island transformation methods.
    /// </summary>
    public class IslandTransformation : MonoBehaviour
    {
        /// <summary>
        /// Instance of this class. This is needed to start the transformation Coroutines.
        /// </summary>
        public static IslandTransformation Instance { private set; get; }

        /// <summary>
        /// Called by Unity when application starts.
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Move all islands to the center of the table.
        /// </summary>
        public void CenterIslands()
        {
            CenterIslands(VizManager.IslandData.GetAllIslandEntities());
        }

        /// <summary>
        /// Move all islands to center of the table with a collection of specific islands exactly at the center.
        /// </summary>
        /// <param name="islandEntities">The collection of islands that will be at the center.</param>
        public void CenterIslands(Entity[] islandEntities)
        {
            StartCoroutine(CenterScaleIslandRoutine(islandEntities));
        }

        /// <summary>
        /// Move all islands to center of the table with one specific island exactly at the center.
        /// </summary>
        /// <param name="centerIslandEntity">Entity of the island that will be at the center.</param>
        /// <param name="zoomLevel">Target zoom level.</param>
        public void CenterIslands(Entity centerIslandEntity, IslandLOD zoomLevel)
        {
            float3 islandPosition = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(centerIslandEntity).Value +
                                World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CenterOffsetData>(centerIslandEntity).Value;
            StartCoroutine(MoveIslandsRoutine(islandPosition, VizSettings.CenterPosition, 50, zoomLevel));
        }

        /// <summary>
        /// Centers all islands and changes the scale level to a point where all <paramref name="islandEntities"/> are visible.
        /// </summary>
        /// <param name="islandEntities">An array of entities that will be visible.</param>
        private static IEnumerator CenterScaleIslandRoutine(Entity[] islandEntities)
        {
            // Translate
            float3 centroid = CalculateTranslationCentroid(islandEntities);
            yield return MoveIslandsRoutine(centroid, VizSettings.CenterPosition, 50);

            // Scale
            float maxDistance = GetFurthestIslandDistance(islandEntities);
            if (maxDistance > VizSettings.ViewRadius)
            {
                float scaleDelta = (1 - VizSettings.ViewRadius / maxDistance) * VizSettings.CurrentIslandScale;
                Debug.Log("maxDistance = " + maxDistance + " - CurrentIslandScale = " + VizSettings.CurrentIslandScale + " - scaleDelta = " + scaleDelta);
                //yield return ScaleIslandsRoutine(-scaleDelta, 50);
            }
        }

        /// <summary>
        /// Moves the islands a specific distance in a specific direction (based on <paramref name="start"/> and <paramref name="target"/>) 
        /// using a specific amount of <paramref name="steps"/> (one step per FixedUpdate).
        /// </summary>
        private static IEnumerator MoveIslandsRoutine(float3 start, float3 target, int steps)
        {
            float distance = math.distance(start, target);
            float3 direction = math.normalize(target - start);

            float3 stepSize = direction * distance * 1f / (float)steps;
            int stepCount = 0;

            while (stepCount < steps)
            {
                IslandTransformationSystem.Translate(stepSize);
                stepCount++;
                yield return new WaitForFixedUpdate();
            }
        }

        private static IEnumerator MoveIslandsRoutine(float3 start, float3 target, int steps, IslandLOD zoomLevel)
        {
            if (LODManager.CurrentLOD == zoomLevel)
            {
                yield return MoveIslandsRoutine(start, target, steps);
            }
            else
            {
                yield return MoveIslandsRoutine(start, target, steps / 2);
                //yield return ScaleIslandsRoutine(ZoomLevelHelper.GetDeltaScaleToZoomLevel(zoomLevel), steps / 2);
            }
        }

        //private static IEnumerator ScaleIslandsRoutine(float scaleDelta, int steps)
        //{
        //    float stepSize = scaleDelta * 1f / (float)steps;
        //    int stepCount = 0;

        //    while (stepCount <= steps)
        //    {
        //        IslandTransformationSystem.Scale(stepSize);
        //        stepCount++;
        //        yield return new WaitForFixedUpdate();
        //    }
        //}


        private static float3 CalculateTranslationCentroid(Entity[] entities)
        {
            float3 positions = float3.zero;
            int positionCounter = 0;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (Entity entity in entities)
            {
                positions += entityManager.GetComponentData<Translation>(entity).Value;
                positionCounter++;
            }

            return positions / positionCounter;
        }

        private static float GetFurthestIslandDistance(Entity[] islandEntities)
        {
            float maxDistance = 0f;
            float distance;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (Entity entity in islandEntities)
            {
                distance = math.distance(entityManager.GetComponentData<Translation>(entity).Value, VizSettings.CenterPosition);
                if (distance > maxDistance)
                    maxDistance = distance;
            }

            return maxDistance;
        }

        

    }
}

