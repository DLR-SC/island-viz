namespace IslandSettings
{
    public static class ZoomLevelSettings
    {
        /// <summary>
        /// Stay in the Region-ZoomLevel, while player camera height is below this value (but above the BuildingHeight).
        /// </summary>
        public const float RegionHeight = 15f;
        
        /// <summary>
        /// Stay in the Building-ZoomLevel, while player camera height is below this value.
        /// </summary>
        public const float BuildingHeight = 6f;
    }
}