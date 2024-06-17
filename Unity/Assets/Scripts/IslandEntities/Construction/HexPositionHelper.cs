using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This class contains helper functions for <see cref="HexPosition"/> placement.
    /// </summary>
    public static class HexPositionHelper
    {
        /// <summary>
        /// Get the best positions for a x-number of hex positions based on already existing hex positions.
        /// </summary>
        /// <param name="currentLocalRegionPositions">List of all local hex positions that are already occupied.</param>
        /// <param name="regionElementsCount">Number of hex positions that must be placed.</param>
        public static List<HexPosition> FindBestPositions(List<HexPosition> currentLocalRegionPositions, int regionElementsCount)
        {
            List<HexPosition> positions = new List<HexPosition>();

            int searchRadius = 0;

            while (positions.Count != regionElementsCount)
            {
                positions = new List<HexPosition>();
                List<HexPosition> freePositions = new List<HexPosition>();
                // Add all empty hex positions to freePositions list.
                for (int x = -searchRadius; x < searchRadius; x++)
                {
                    for (int y = 0; y < searchRadius; y++)
                    {
                        if (x != -searchRadius && x != searchRadius && y != -searchRadius && y != searchRadius)
                        {
                            continue;
                        }

                        if (!currentLocalRegionPositions.Any(position => (position.x == x && position.y == y)))
                        {
                            freePositions.Add(new HexPosition() { x = x, y = y });
                        }
                    }
                }
                // Try place regionElementsCount regions on each free hex position.
                foreach (HexPosition hexPosition in freePositions)
                {
                    positions = Place(hexPosition, regionElementsCount, currentLocalRegionPositions);

                    if (positions.Count == regionElementsCount)
                    {
                        break;
                    }
                }
                // If no place was suitable, increase search readius.
                if (positions.Count != regionElementsCount)
                {
                    searchRadius++;
                }
            }

            return positions;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="count"></param>
        /// <param name="currentLocalRegionPositions"></param>
        /// <returns></returns>
        private static List<HexPosition> Place(HexPosition startPosition, int count, List<HexPosition> currentLocalRegionPositions)
        {
            List<HexPosition> foundPositions = new List<HexPosition>(); // Free elements, that will be placed on.
            List<HexPosition> uncheckedPositions = new List<HexPosition>(); // Found Elements, that could still contain free neigbor positions.

            foundPositions.Add(startPosition);
            uncheckedPositions.Add(startPosition);

            // Check neighbors until we found enough positions or until there are no position with unchecked neighbors left.
            while (foundPositions.Count < count && uncheckedPositions.Count > 0)
            {
                //int nearestUncheckedPositionID = GetNearestPosition(uncheckedPositions);
                //HexPosition[] neighbors = HexPosition.GetNeighbors(uncheckedPositions[nearestUncheckedPositionID]);
                HexPosition[] neighbors = HexPosition.GetNeighbors(uncheckedPositions[0]);

                foreach (HexPosition hexPosition in neighbors)
                {
                    if (!currentLocalRegionPositions.Any(p => (p.x == hexPosition.x && p.y == hexPosition.y)) && !foundPositions.Any(p => (p.x == hexPosition.x && p.y == hexPosition.y)))
                    {
                        uncheckedPositions.Add(hexPosition);
                        foundPositions.Add(hexPosition);
                    }
                }

                //uncheckedPositions.RemoveAt(nearestUncheckedPositionID);
                uncheckedPositions.RemoveAt(0);
            }

            // Trim list, if there are to many positions.
            if (foundPositions.Count > count)
            {
                foundPositions.RemoveRange(count, foundPositions.Count - count);
            }

            return foundPositions;
        }

        /// <summary>
        /// Returns the position in list of the first Hexposition that is nearest to zero.
        /// </summary>
        private static int GetNearestPosition(List<HexPosition> positions)
        {
            int id = 0;
            float distance = 9999;
            float d;

            for (int i = 0; i < positions.Count; i++)
            {
                d = Vector2.Distance(Vector2.zero, HexPosition.ToVector2(positions[i]));
                if (d < distance)
                {
                    id = i;
                    distance = d;
                }
            }

            return id;
        }

        public static float2 GetCenterPosition (List<HexPosition> positions)
        {
            if (positions == null || positions.Count == 0)
                return float2.zero;

            float counter = 0;
            float3 sum = float3.zero;

            foreach (var item in positions)
            {
                sum += item.ToLocalWorldPosition();
                counter++;
            }

            float3 average = sum / counter;
            return new float2(average.x, average.z);
        }

        public static float GetFurthestRegionTileDistanceFromCenter (float2 center, List<HexPosition> regionTilePositions)
        {
            float maxDistance = 0;

            foreach (var item in regionTilePositions)
            {
                float distance = math.distance(center, item.ToLocalWorldPosition2D());
                if (distance > maxDistance)
                    maxDistance = distance;
            }

            return maxDistance;
        }

    }

}
