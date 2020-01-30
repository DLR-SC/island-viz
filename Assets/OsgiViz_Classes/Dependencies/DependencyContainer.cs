using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependencyContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        IslandVizInteraction.Instance.OnClearVisForNextCommit += DeleteAllDepedencyArrows;
    }

    public void DeleteAllDepedencyArrows()
    {
        foreach(Transform child in gameObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
