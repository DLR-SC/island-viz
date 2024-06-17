using IslandEvents;

namespace IslandLevelOfDetail
{
    /// <summary>
    /// This class contains the current level-of-detail (LOD).
    /// </summary>
    public static class LODManager
    {
        /// <summary>
        /// The current zoom level.
        /// </summary>
        public static IslandLOD CurrentLOD { private set; get; }

        /// <summary>
        /// Changes the current zoom level and throws a OnZoomLevelChanged event.
        /// </summary>
        /// <param name="newLevel">The new zoom level.</param>
        public static void SetZoomLevel (IslandLevelOfDetail.IslandLOD newLevel)
        {
            var previousLevel = CurrentLOD;
            CurrentLOD = newLevel;
            ZoomLevelEvents.OnZoomLevelChanged?.Invoke(CurrentLOD, previousLevel);

            UnityEngine.Debug.Log("ZoomLevel: Changed to " + CurrentLOD.ToString());
        }

    }

}
