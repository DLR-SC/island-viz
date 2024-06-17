using Unity.Mathematics;

namespace IslandSettings
{
    public static class VizSettings
    {   
        public static float IslandScaleDelta { set; get; } // This value is observed (and reset when applied once) by the IslandNavigationSystem.

        // Physics
        public const float RaycastDistance = 20;



        // Dynamic Values.
        public static float CurrentIslandScale = 1f;

        // Readonly Render Values.
        public const float ViewRadius = 7;

        // Readonly Constructor Values.
        public static readonly float IslandHeight = 5.1f; // TODO change for dynamic table height.
        public static readonly float3 CenterPosition = new float3 (0f, IslandHeight, -20f);

        public static bool ShowRelationshipsOnHighlight = true;
    }
}

