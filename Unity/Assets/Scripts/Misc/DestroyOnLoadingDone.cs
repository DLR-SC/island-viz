using IslandEvents;
using UnityEngine;

/// <summary>
/// Attach this script to a GameObject to destroy it after the loading is done.
/// </summary>
public class DestroyOnLoadingDone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VizEvents.OnVisualizationInitilized += DestroyThis;
    }

    public void DestroyThis ()
    {
        Destroy(gameObject);
    }
}
