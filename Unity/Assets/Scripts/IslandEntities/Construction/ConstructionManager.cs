using System.Collections;
using System.Diagnostics;
using IslandData.SystemData;
using IslandEvents;
using UnityEngine;

namespace IslandEntities.Construction
{
    /// <summary>
    /// This class calls all constructors that instantiate all entites (island components, spring forces, and relationship arrows) of the visualization.
    /// </summary>
    public class ConstructionManager : MonoBehaviour
    {
        /// <summary>
        /// This scriptable object contains all prefabs, base materials, and colors needed for construction.
        /// (Set in the Unity inspector)
        /// </summary>
        public ScriptableConstructorData ConstructorSettings;

        private ConstructorData _cData; // Contains e.g. entites and materials based on the <see cref="ScriptableConstructorData"/>.

        private void Start()
        {
            VizEvents.OnDataLoadingDone += StartConstruction; // Subscribe to event.
            VizEvents.OnVisualizationQuit += Dispose; // Subscribe to event.
        }

        /// <summary>
        /// Start the construction coroutine.
        /// </summary>
        private void StartConstruction (IslandSystem islandData)
        {
            StartCoroutine(ConstructRoutine(islandData));
        }

        /// <summary>
        /// Create all islands entites.
        /// </summary>
        public IEnumerator ConstructRoutine(IslandSystem islandData)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Process data needed by constructors.
            _cData = new ConstructorData(ConstructorSettings, islandData);

            // Create the island entities.
            yield return IslandConstructor.SpawnAllIslands(islandData, _cData);

            // Initiate relationship construction (relationships are be built on the fly).
            yield return RelationshipConstructor.Initiate(_cData);

            stopWatch.Stop();
            UnityEngine.Debug.Log("<b>ConstructorManager</b>: Island DOTS construction finished in " + stopWatch.Elapsed.TotalSeconds.ToString("0.000") + " seconds.");

            yield return null;

            VizEvents.OnConstructionDone?.Invoke(); // Throw event.
        }

        /// <summary>
        /// Dispose native data on visualization quit.
        /// </summary>
        private void Dispose() 
        {
            _cData.Dispose();
        }
    }
}
