using IslandData.Construction;
using IslandData.SystemData;

namespace IslandEvents
{
    /// <summary>
    /// This class contains events other classes can subscribe to.
    /// </summary>
    public static class VizEvents
    {
        /// <summary>
        /// Called when the visualization data was loaded.
        /// </summary>
        public static DataEvent OnDataLoadingDone { get; set; }

        /// <summary>
        /// Called when the island entities were constructed.
        /// </summary>
        public static BasicEvent OnConstructionDone { get; set; }

        /// <summary>
        /// Called when all plugins were loaded.
        /// </summary>
        public static BasicEvent OnPluginsLoaded { get; set; }

        /// <summary>
        /// Called when all plugins were loaded.
        /// </summary>
        public static BasicEvent OnVisualizationInitilized { get; set; }

        /// <summary>
        /// Called when right before the visualization quits.
        /// </summary>
        public static BasicEvent OnVisualizationQuit { get; set; }



        /// <summary>
        /// Basic event used for anything.
        /// </summary>
        public delegate void BasicEvent();

        /// <summary>
        /// Event used for data.
        /// </summary>
        /// <param name="neo4jData">The loaded neo4j data.</param>
        /// <param name="islandData">The loaded island data.</param>
        public delegate void DataEvent(IslandSystem islandData);        
    }
}
