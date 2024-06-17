
using IslandLevelOfDetail;

namespace IslandEvents
{
    public static class ZoomLevelEvents
    {
        /// <summary>
        /// Event called when the zoom level changed.
        /// </summary>
        public static ZoomLevelChanged OnZoomLevelChanged { get; set; }
        
        /// <summary>
        /// Delegate of the OnZoomLevelChanged event.
        /// </summary>
        /// <param name="newLevel">The new zoom level.</param>
        /// <param name="prevLevel">The previous zoom level.</param>
        public delegate void ZoomLevelChanged(IslandLOD newLevel, IslandLOD prevLevel);
    }
}