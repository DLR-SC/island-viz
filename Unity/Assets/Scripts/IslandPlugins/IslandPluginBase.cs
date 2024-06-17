using System.Collections;
using UnityEngine;

namespace IslandPlugins
{
    public abstract class IslandPluginBase : MonoBehaviour
    {
        public abstract IEnumerator Init();
    }
}
