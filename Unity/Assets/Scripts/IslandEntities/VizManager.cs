using System.Collections;
using IslandData.SystemData;
using IslandEvents;
using UnityEngine;

namespace IslandViz.Core
{
    /// <summary>
    /// This class listens to and throws key events related to the visualization.
    /// </summary>
    public class VizManager : MonoBehaviour
    {
        public static VizManager Instance { private set; get; }
        public static IslandSystem IslandData { private set; get; }


        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Called by Unity when application starts.
        /// </summary>
        void Start()
        {
            VizEvents.OnDataLoadingDone += SetData;
            VizEvents.OnPluginsLoaded += FinalizeInitialization;
        }
        

        private void SetData(IslandSystem islandData)
        {
            IslandData = islandData;
        }
        
        private void FinalizeInitialization ()
        {
            StartCoroutine(DelayedFinalization());
        }

        private IEnumerator DelayedFinalization()
        {
            yield return null; // Wait for other processes to finish.
            VizEvents.OnVisualizationInitilized?.Invoke();
        }

        /// <summary>
        /// Called by Unity on appllication quit.
        /// </summary>
        public void OnApplicationQuit()
        {
            VizEvents.OnVisualizationQuit?.Invoke();
        }
    }
}

