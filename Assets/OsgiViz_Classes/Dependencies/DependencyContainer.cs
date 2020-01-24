using OSGI_Datatypes.OrganisationElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DependencyContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        IslandVizInteraction.Instance.OnNewCommit += DeleteAllDepedencyArrows;
    }

    public void DeleteAllDepedencyArrows(Commit oldCommit, Commit newCommit)
    {
        foreach(Transform child in gameObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
