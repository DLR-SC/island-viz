using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceSlice : MonoBehaviour {

    public float height;

    void Start()
    {
    }

    /*
    private bool checkInterfaceCollision(GameObject target)
    {
        foreach (IGroup grp in serviceInterfaceGroups)
        {
            if (grp.serviceInterface.GetComponent<ServiceNodeScript>().origin == target)
                return true;
        }
        return false;
    }
    */

    /*
    public void hideSlice()
    {
        foreach (IGroup grp in serviceInterfaceGroups)
            grp.hideGrp();
    }

    public void showSlice()
    {
        foreach (IGroup grp in serviceInterfaceGroups)
            grp.showGrp();
    }
    */
}
