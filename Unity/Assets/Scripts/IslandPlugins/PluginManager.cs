using System.Collections;
using IslandEvents;
using UnityEngine;

namespace IslandPlugins
{
    /// <summary>
    /// This class initiates plugins that were assigned via the Editor.
    /// </summary>
    public class PluginManager : MonoBehaviour
    {
        /// <summary>
        /// Array containing prefabs of additional input components that will be initiated. 
        /// </summary>
        public GameObject[] PluginPrefabs; 

        
        private void Start()
        {
            VizEvents.OnConstructionDone += InitPlugins;
        }

        /// <summary>
        /// Starts the plugin init coroutine.
        /// </summary>
        private void InitPlugins()
        {
            StartCoroutine(InstantiateAndInitiatePlugins());
        }

        /// <summary>
        /// Instantiates all plugin prefabs and calls the Init() method on each plugin.
        /// </summary>
        private IEnumerator InstantiateAndInitiatePlugins()
        {   
            foreach (var prefab in PluginPrefabs)
            {   
                if (prefab.GetComponent<IslandPluginBase>())
                {
                    yield return Instantiate(prefab, transform).GetComponent<IslandPluginBase>().Init();
                }
            }

            yield return null;
            VizEvents.OnPluginsLoaded?.Invoke(); // Throw event.
        }
    }
}
