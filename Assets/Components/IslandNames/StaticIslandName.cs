using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OsgiViz.Core;


public class StaticIslandName : MonoBehaviour
{
    public Text Name;
    public Text Line;

    public void ChangeName (string name)
    {
        Name.text = name;
    } 

    public void DisableText ()
    {
        Name.gameObject.SetActive(false);
        Line.gameObject.SetActive(false);
    }

    public void EnableText ()
    {
        Name.gameObject.SetActive(true);
        Line.gameObject.SetActive(true);
    }
}
