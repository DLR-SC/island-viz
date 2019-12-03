using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OsgiViz.Core;


public class StaticIslandName : MonoBehaviour
{
    public GameObject NameParent;
    public Text Name;
    public Text Line;

    public void ChangeName (string name)
    {
        Name.text = name;
    } 

    public void DisableText ()
    {
        NameParent.SetActive(false);
        Line.gameObject.SetActive(false);
    }

    public void EnableText ()
    {
        NameParent.SetActive(true);
        Line.gameObject.SetActive(true);
    }
}
